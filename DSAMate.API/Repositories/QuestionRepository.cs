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

        public async Task<List<QuestionDTO>> GetAllAsync(
        string? search,
        string? difficulty,
        string? topic,
        bool? solved,
        string? sortBy,
        bool isAscending,
        int pageNumber,
        int pageSize)
        {
            var userId = _userContext.GetUserId();
            var questionsQuery = _dbContext.Questions.AsNoTracking();

            // 1. Search (Title, Description, Topic)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.ToLower();
                questionsQuery = questionsQuery.Where(q =>
                    q.Title.ToLower().Contains(normalizedSearch) ||
                    q.Description.ToLower().Contains(normalizedSearch) ||
                    q.Topic.ToString().ToLower().Contains(normalizedSearch));
            }

            // 2. Difficulty Filter
            if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<Difficulty>(difficulty, true, out var diffEnum))
            {
                questionsQuery = questionsQuery.Where(q => q.Difficulty == diffEnum);
            }

            // 3. Topic Filter
            if (!string.IsNullOrWhiteSpace(topic) && Enum.TryParse<Topic>(topic, true, out var topicEnum))
            {
                questionsQuery = questionsQuery.Where(q => q.Topic == topicEnum);
            }

            // 4. Solved Status Filter
            if (solved.HasValue)
            {
                questionsQuery = questionsQuery.Where(q =>
                    _dbContext.UserQuestionStatuses.Any(uqs =>
                        uqs.QuestionId == q.Id &&
                        uqs.UserId == userId &&
                        uqs.IsSolved) == solved.Value);
            }

            // 5. Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                questionsQuery = (sortBy.ToLower(), isAscending) switch
                {
                    ("title", true) => questionsQuery.OrderBy(q => q.Title),
                    ("title", false) => questionsQuery.OrderByDescending(q => q.Title),
                    ("difficulty", true) => questionsQuery.OrderBy(q => q.Difficulty),
                    ("difficulty", false) => questionsQuery.OrderByDescending(q => q.Difficulty),
                    _ => questionsQuery
                };
            }

            // 6. Pagination & Projection
            var skip = (pageNumber - 1) * pageSize;

            return await questionsQuery
                .Skip(skip)
                .Take(pageSize)
                .Select(q => new QuestionDTO
                {
                    Id = q.Id,
                    Title = q.Title,
                    Difficulty = q.Difficulty.ToString(),
                    Topic = q.Topic.ToString(),
                    Description = q.Description,
                    Solved = _dbContext.UserQuestionStatuses.Any(uqs =>
                        uqs.QuestionId == q.Id &&
                        uqs.UserId == userId &&
                        uqs.IsSolved),
                    SolvedAt = _dbContext.UserQuestionStatuses
                        .Where(uqs => uqs.QuestionId == q.Id && uqs.UserId == userId && uqs.IsSolved)
                        .Select(uqs => uqs.SolvedAt)
                        .FirstOrDefault(),
                    Hint = q.Hint,
                })
                .ToListAsync();
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
                .Select(uqs => new {
                    QuestionDomain = uqs.Question,
                    Solved = uqs.IsSolved,
                    SolvedAt = uqs.SolvedAt
                })
                .ToListAsync();
            return solved.Select(s =>
            {
                var questionDTO = _mapper.Map<QuestionDTO>(s.QuestionDomain);
                questionDTO.Solved = s.Solved;
                questionDTO.SolvedAt = s.SolvedAt;
                return questionDTO;
            })
            .ToList() ;
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

        public async Task<QuestionDTO> GetRandomAsync()
        {
            var userId = _userContext.GetUserId();
            var unsolvedQuestions = _dbContext.Questions
                .AsNoTracking()
                .Where(q => !_dbContext.UserQuestionStatuses.Any(uqs =>
                        uqs.QuestionId == q.Id &&
                        uqs.UserId == userId &&
                        uqs.IsSolved));

            var questionsCount = unsolvedQuestions.Count();

            var random = new Random();
            int skipCount = random.Next(0, questionsCount);

            return await unsolvedQuestions.Skip(skipCount).Take(1).Select(q => new QuestionDTO
            {
                Id = q.Id,
                Title = q.Title,
                Difficulty = q.Difficulty.ToString(),
                Topic = q.Topic.ToString(),
                Description = q.Description,
                Solved = _dbContext.UserQuestionStatuses.Any(uqs =>
                    uqs.QuestionId == q.Id &&
                    uqs.UserId == userId &&
                    uqs.IsSolved),
                SolvedAt = _dbContext.UserQuestionStatuses
                        .Where(uqs => uqs.QuestionId == q.Id && uqs.UserId == userId && uqs.IsSolved)
                        .Select(uqs => uqs.SolvedAt)
                        .FirstOrDefault(),
                Hint = q.Hint,
            }).FirstOrDefaultAsync();
        }
    }
}