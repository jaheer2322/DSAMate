using DSAMate.API.Data.Domains;
using DSAMate.API.Models.Dtos;

namespace DSAMate.API.Repositories
{
    public interface IQuestionRepository
    {
        Task<QuestionDTO?> GetAsync(Guid id);
        Task<List<QuestionDTO>> GetAllAsync(string? column, 
                                            string? query, 
                                            string? sortBy, 
                                            bool isAscending, 
                                            int pageNumber, 
                                            int pageSize);
        Task<QuestionDTO> CreateAsync(CreateQuestionDTO createQuestionDTO);
    }
}
