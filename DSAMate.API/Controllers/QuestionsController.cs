using DSAMate.API.Filters;
using DSAMate.API.Models.Dtos;
using DSAMate.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSAMate.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly List<string> queryableColumns = new List<string> {
            "title",
            "difficulty",
            "topic",
            "solved"
        };
        private readonly List<string> allowedParameters = new List<string> {
            "column",
            "query",
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
        [ValidateModelAttribute]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateQuestionDTO createQuestionDTO)
        {
            var questionDTO = await _questionRepository.CreateAsync(createQuestionDTO);
            return Ok(questionDTO);
        }

        [HttpPost("bulk")]
        [ValidateModelAttribute]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBulk([FromBody] List<CreateQuestionDTO> createQuestionDTOs)
        {
            var questionDTOs = await _questionRepository.CreateBulkAsync(createQuestionDTOs);
            return Ok(questionDTOs);
        }
        [HttpGet("{id}")]
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
        public async Task<IActionResult> GetAll([FromQuery] string? column,
                                                [FromQuery] string? query, 
                                                [FromQuery] string? sortBy, 
                                                [FromQuery] bool isAscending = true, 
                                                [FromQuery] int pageNumber = 1, 
                                                [FromQuery] int pageSize = 10)
        {
            // Check if any extra query parameters are passed
            var extraParameters = HttpContext.Request.Query.Keys.Except(allowedParameters, StringComparer.OrdinalIgnoreCase).ToList();
            if (extraParameters.Any())
            {
                return BadRequest($"Invalid query parameters: {string.Join(", ", extraParameters)}. Allowed parameters are: {string.Join(", ", allowedParameters)}");
            }

            // Validate columns passed in query
            if (string.IsNullOrWhiteSpace(column) == false && !queryableColumns.Contains(column.ToLower()))
            {
                return BadRequest($"Invalid column to filter {column}. Allowed columns are: {string.Join(", ", queryableColumns)}");
            }

            if (string.IsNullOrWhiteSpace(sortBy) == false && sortBy.ToLower() != "title")
            {
                return BadRequest($"Invalid column to sortBy {sortBy}. Allowed column is title");
            }

            var questionDTOs = await _questionRepository.GetAllAsync(column, query, sortBy, isAscending, pageNumber, pageSize);
            return Ok(questionDTOs);
        }

        [HttpPost("{questionId}/mark-solved")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> MarkSolved(Guid questionId)
        {
            await _questionRepository.MarkAsSolvedAsync(questionId);
            return Ok();
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
    }
}
