using AutoMapper;
using DSAMate.API.Data;
using DSAMate.API.Data.Domains;
using DSAMate.API.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace DSAMate.API.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        public QuestionRepository(AppDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public async Task<List<QuestionDTO>> GetAllAsync(string? column, string? query, string? sortBy, bool isAscending, int pageNumber, int pageSize)
        {
            var questions = _dbContext.Questions.AsNoTracking();

            // Filtering
            if (string.IsNullOrWhiteSpace(column) == false && string.IsNullOrWhiteSpace(query) == false)
            {
                questions = column.ToLower() switch
                {
                    "title" => questions.Where(q => q.Title.ToLower().Contains(query)),
                    "difficulty" => questions.Where(q => q.Difficulty.ToString().ToLower() == query),
                    "topic" => questions.Where(q => q.Topic.ToString().ToLower() == query),
                    _ => questions
                };
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

            questions = questions.Skip(skippableQuestions).Take(pageSize);
            return _mapper.Map<List<QuestionDTO>>(await questions.ToListAsync());
        }
        public async Task<QuestionDTO> CreateAsync(CreateQuestionDTO createQuestionDTO)
        {
            var question = _mapper.Map<Question>(createQuestionDTO);
            await _dbContext.AddAsync(question);
            await _dbContext.SaveChangesAsync();
            return _mapper.Map<QuestionDTO>(question);
        }
    }
}