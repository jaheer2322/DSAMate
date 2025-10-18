using DSAMate.API.Data.Domains;
using Microsoft.AspNetCore.Identity;

namespace DSAMate.API.Models.Domains
{
    public class UserQuestionStatus
    {
        public string UserId { get; set; }
        public Guid QuestionId { get; set; }
        public bool IsSolved { get; set; }
        public DateTime? SolvedAt { get; set; }

        // Nav prop
        public Question Question { get; set; }
    }
}
