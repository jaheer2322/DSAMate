using DSAMate.API.Models.Dtos;
using DSAMate.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace DSAMate.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigin")]
    public class QuestionsController : ControllerBase
    {
        private readonly List<string> allowedParameters = new List<string> {
            "search",
            "difficulty",
            "topic",
            "solved",
            "sortBy",
            "isAscending",
            "pageNumber",
            "pageSize"
        };
        private readonly IQuestionRepository _questionRepository;
        public QuestionsController(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateQuestionDTO createQuestionDTO)
        {
            var questionDTO = await _questionRepository.CreateAsync(createQuestionDTO);
            return Ok(questionDTO);
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBulk([FromBody] List<CreateQuestionDTO> createQuestionDTOs)
        {
            var questionDTOs = await _questionRepository.CreateBulkAsync(createQuestionDTOs);
            return Ok(questionDTOs);
        }
        [HttpGet("random")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Random()
        {
            var questionDTO = await _questionRepository.GetRandomAsync();
            return Ok(questionDTO);
        }

        [HttpGet("solved")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> SolvedQuestions()
        {
            var questionsSolved = await _questionRepository.GetUserSolvedQuestionsAsync();
            return Ok(questionsSolved);
        }

        [HttpGet("progress")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Progress()
        {
            var progress = await _questionRepository.GetProgressForUserAsync();
            return Ok(progress);
        }
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Get(Guid id)
        {
            var questionDTO = await _questionRepository.GetAsync(id);
            if (questionDTO == null)
            {
                return NotFound();
            }
            return Ok(questionDTO);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> GetAll([FromQuery] string? search,
                                                [FromQuery] string? difficulty,
                                                [FromQuery] string? topic,
                                                [FromQuery] bool? solved,
                                                [FromQuery] string? sortBy,
                                                [FromQuery] bool isAscending = true,
                                                [FromQuery] int pageNumber = 1,
                                                [FromQuery] int pageSize = 10)
        {
            // Check if any extra query parameters are passed
            var extraParameters = HttpContext.Request.Query.Keys.Except(allowedParameters, StringComparer.OrdinalIgnoreCase).ToList();
            if (extraParameters.Any())
            {
                return BadRequest(new { Response = $"Invalid query parameters: {string.Join(", ", extraParameters)}. Allowed parameters are: {string.Join(", ", allowedParameters)}" });
            }

            if (string.IsNullOrWhiteSpace(sortBy) == false && sortBy.ToLower() != "title" && sortBy.ToLower() != "difficulty")
            {
                return BadRequest(new { Response = $"Invalid column to sortBy {sortBy}. Allowed column is title" });
            }

            if (string.IsNullOrWhiteSpace(difficulty) == false && Enum.TryParse<Models.Domains.Difficulty>(difficulty, true, out _) == false)
            {
                return BadRequest(new { Response = "Invalid difficulty filter. Allowed values are Easy, Medium, Hard" });
            }
            
            var questionDTOs = await _questionRepository.GetAllAsync(search, difficulty, topic, solved, sortBy, isAscending, pageNumber, pageSize);

            return Ok(questionDTOs);
        }

        [HttpPost("{questionId}/mark-solved")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> MarkSolved(Guid questionId)
        {
            await _questionRepository.MarkAsSolvedAsync(questionId);
            return Ok();
        }

        [HttpPost("reset-progress")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> ResetProgress()
        {
            await _questionRepository.ResetProgressAsync();
            return Ok();
        }
    }
}
