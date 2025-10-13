using AutoMapper;
using DSAMate.API.Filters;
using DSAMate.API.Models.Dtos;
using DSAMate.API.Repositories;
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
            "topic"
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

        [HttpGet]
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
                return BadRequest($"Invalid column to sortBy {column}. Allowed column is title");
            }

            var questionDTOs = await _questionRepository.GetAllAsync(column, query, sortBy, isAscending, pageNumber, pageSize);
            return Ok(questionDTOs);
        }

        [HttpPost]
        [ValidateModelAttribute]
        public async Task<IActionResult> Create([FromBody] CreateQuestionDTO createQuestionDTO)
        {
            var questionDTO = await _questionRepository.CreateAsync(createQuestionDTO);
            return Ok(questionDTO);
        }
    }
}
