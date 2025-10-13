using DSAMate.API.Models.Domains;

namespace DSAMate.API.Data.Domains
{
    public class Question
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Difficulty Difficulty { get; set; }
        public Topic Topic { get; set; }
        public string Hint { get; set; }
    }
}
