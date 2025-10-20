using AutoMapper;
using DSAMate.API.Data;
using DSAMate.API.Data.Domains;
using DSAMate.API.Models.Domains;
using DSAMate.API.Models.Dtos;
using DSAMate.API.Services;
using Microsoft.EntityFrameworkCore;

namespace DSAMate.API.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContext;
        public QuestionRepository(AppDbContext dbContext, IMapper mapper, IUserContextService userContext)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _userContext = userContext;
        }
        public async Task<QuestionDTO?> GetAsync(Guid id)
        {
            var userId = _userContext.GetUserId();
            var question = await _dbContext.Questions.FirstOrDefaultAsync(q => q.Id == id);
            if (question == null)
            {
                return null;
            }
            var questionDTO = _mapper.Map<QuestionDTO>(question);
            var solved = await _dbContext.UserQuestionStatuses.FirstOrDefaultAsync(uqs => uqs.UserId == userId && uqs.QuestionId == id);
            if(solved != null)
            {
                questionDTO.Solved = true;
                questionDTO.SolvedAt = solved.SolvedAt;
            }
            return questionDTO;
        }
        public async Task<List<QuestionDTO>> GetAllAsync(string? column, string? query, string? sortBy, bool isAscending, int pageNumber, int pageSize)
        {
            var userId = _userContext.GetUserId();
            var questions = _dbContext.Questions.AsNoTracking();
            var solvedQuestions = await _dbContext.UserQuestionStatuses.AsNoTracking()
                .Where(uqs => uqs.UserId == userId)
                .Select(uqs => new {
                    Id = uqs.QuestionId,
                    SolvedAt = uqs.SolvedAt
                })
                .ToDictionaryAsync(q => q.Id, q => q.SolvedAt);

            // Filtering
            if (string.IsNullOrWhiteSpace(column) == false && string.IsNullOrWhiteSpace(query) == false)
            {
                query = query.ToLower();
                if(column.ToLower() == "solved")
                {
                    var solvedQuestionIds = solvedQuestions.Keys;
                    if (query == "true")
                    {
                        questions = questions.Where(q => solvedQuestionIds.Contains(q.Id));
                    }
                    else
                    {
                        questions = questions.Where(q => !solvedQuestionIds.Contains(q.Id));
                    }
                }
                else
                {
                    questions = column.ToLower() switch
                    {
                        "title" => questions.Where(q => q.Title.ToLower().Contains(query)),
                        "difficulty" => Enum.TryParse<Difficulty>(query, ignoreCase: true, out var difficultyVal)
                                        ? questions.Where(q => q.Difficulty == difficultyVal)
                                        : questions.Where(q => false),
                        "topic" => Enum.TryParse<Topic>(query, ignoreCase: true, out var topicVal)
                                   ? questions.Where(q => q.Topic == topicVal)
                                   : questions.Where(q => false),
                        _ => questions
                    };
                }
            }

            // Sorting
            if (string.IsNullOrWhiteSpace(sortBy) == false)
            {

                if (isAscending == false)
                {
                    questions = sortBy.ToLower() switch
                    {
                        "title" => questions.OrderByDescending(q => q.Title),
                        _ => questions
                    };
                }
                else
                {
                    questions = sortBy.ToLower() switch
                    {
                        "title" => questions.OrderBy(q => q.Title),
                        _ => questions
                    };
                }
            }

            // Pagination
            var skippableQuestions = (pageNumber - 1) * pageSize;


            var allQuestions = await questions.Skip(skippableQuestions).Take(pageSize).ToListAsync();

            var questionDTOs = allQuestions.Select(q =>
            {
                var dto = _mapper.Map<QuestionDTO>(q);
                dto.Solved = solvedQuestions.ContainsKey(q.Id);
                dto.SolvedAt = solvedQuestions.GetValueOrDefault(q.Id);
                return dto;
            }).ToList();

            return questionDTOs;
        }
        public async Task<QuestionDTO> CreateAsync(CreateQuestionDTO createQuestionDTO)
        {
            var exists = await _dbContext.Questions
                .AnyAsync(q => q.Title == createQuestionDTO.Title);
            if (exists)
            {
                throw new InvalidOperationException("Question already exists.");
            }
            var question = _mapper.Map<Question>(createQuestionDTO);
            await _dbContext.AddAsync(question);
            await _dbContext.SaveChangesAsync();
            return _mapper.Map<QuestionDTO>(question);
        }
        public async Task<List<QuestionDTO>> CreateBulkAsync(List<CreateQuestionDTO> createQuestionDTOs)
        {
            var duplicatesInTitles = createQuestionDTOs
                .GroupBy(q => q.Title.Trim().ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatesInTitles.Any())
            {
                throw new InvalidOperationException($"Duplicate title(s) found in request : {string.Join(", ", duplicatesInTitles)}");
            }

            var titles = createQuestionDTOs.Select(q => q.Title.ToLower()).ToHashSet();

            var existingTitles = await _dbContext.Questions.AsNoTracking()
                .Where(q => titles.Contains(q.Title.ToLower()))
                .Select(q => q.Title)
                .ToListAsync();

            if (existingTitles.Any())
            {
                throw new InvalidOperationException($"These title(s) already exist: {string.Join(", ", existingTitles)}");
            }

            var questions = _mapper.Map<List<Question>>(createQuestionDTOs);
            await _dbContext.Questions.AddRangeAsync(questions);
            await _dbContext.SaveChangesAsync();
            return _mapper.Map<List<QuestionDTO>>(questions);
        }
        public async Task MarkAsSolvedAsync(Guid questionId)
        {
            var userId = _userContext.GetUserId();
            if(userId == null)
            {
                throw new UnauthorizedAccessException();
            }

            var question = await _dbContext.Questions.FirstOrDefaultAsync(q => q.Id == questionId);
            if (question == null)
            {
                throw new InvalidOperationException("QuestionId doesn't exist");
            }
            var uqs = await _dbContext.UserQuestionStatuses.FirstOrDefaultAsync(uqs => uqs.UserId == userId && uqs.QuestionId == questionId);

            if(uqs == null)
            {
                _dbContext.UserQuestionStatuses.Add(new UserQuestionStatus
                {
                    UserId = userId,
                    QuestionId = questionId,
                    IsSolved = true,
                    SolvedAt = DateTime.UtcNow
                });
            }
            else
            {
                uqs.IsSolved = true; // Just to make sure
                uqs.SolvedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<QuestionDTO>> GetUserSolvedQuestionsAsync()
        {
            var userId = _userContext.GetUserId();
            var solved = await _dbContext.UserQuestionStatuses.AsNoTracking()
                .Where(uqs => uqs.UserId == userId)
                .Select(uqs => uqs.Question)
                .ToListAsync();
            return _mapper.Map<List<QuestionDTO>>(solved);
        }

        public async Task<Dictionary<string, TopicProgressDTO>> GetProgressForUserAsync()
        {
            var userId = _userContext.GetUserId();
            var totalsPerTopic = await _dbContext.Questions.AsNoTracking()
                .GroupBy(q => q.Topic)
                .Select(g => new
                {
                    Topic = g.Key.ToString(),
                    Total = g.Count()
                })
                .ToDictionaryAsync(g => g.Topic, g => g.Total);

            var solvedCount = await _dbContext.UserQuestionStatuses.AsNoTracking()
                .Where(uqs => uqs.UserId == userId)
                .Include(uqs => uqs.Question)
                .GroupBy(uqs => uqs.Question.Topic)
                .Select(grp => new
                {
                    Topic = grp.Key.ToString(),
                    Solved = grp.Count(),
                })
                .ToDictionaryAsync(g => g.Topic, g => g.Solved);

            var progress = totalsPerTopic.ToDictionary(kvp => kvp.Key, kvp => new TopicProgressDTO {
                Solved = solvedCount.GetValueOrDefault(kvp.Key),
                Total = kvp.Value
            });
            return progress;
        }
    }
}