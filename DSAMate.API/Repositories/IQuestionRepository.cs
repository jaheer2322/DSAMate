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
        Task MarkAsSolvedAsync(Guid questionId);
        Task<List<QuestionDTO>> GetUserSolvedQuestionsAsync();
        Task<Dictionary<string, TopicProgressDTO>> GetProgressForUserAsync();
        Task<List<QuestionDTO>> CreateBulkAsync(List<CreateQuestionDTO> createQuestionDTOs);
    }
}
