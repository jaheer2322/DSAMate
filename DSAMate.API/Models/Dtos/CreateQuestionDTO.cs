using System.ComponentModel.DataAnnotations;
using DSAMate.API.Models.Domains;

namespace DSAMate.API.Models.Dtos
{
    public class CreateQuestionDTO
    {
        [Required]
        public string Title {  get; set; }
        [Required]
        public string Description {  get; set; }
        [Required]
        [EnumDataType(typeof(Difficulty))]
        public string Difficulty {  get; set; }
        [EnumDataType(typeof(Topic))]
        public string Topic {  get; set; }
        [StringLength(100)]
        public string Hint {  get; set; }

    }
}
