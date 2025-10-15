namespace DSAMate.API.Models.Dtos
{
    public class QuestionDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Difficulty { get; set; }
        public string Topic { get; set; }
        public string Hint { get; set; }
    }
}
