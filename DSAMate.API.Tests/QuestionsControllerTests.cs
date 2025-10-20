using DSAMate.API.Controllers;
using DSAMate.API.Models.Dtos;
using DSAMate.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace DSAMate.API.Tests
{
    [TestClass]
    public class QuestionsControllerTests
    {
        private Mock<IQuestionRepository> _mockQuestionRepository;
        private QuestionsController _controller;

        private readonly QuestionDTO _sampleQuestionDto = new QuestionDTO
        {
            Id = Guid.NewGuid(),
            Title = "Sample Question",
            Description = "A simple question",
            Difficulty = "Easy",
            Topic = "Array",
            Hint = "Use a loop",
            Solved = false,
            SolvedAt = null
        };

        private readonly string _testUserId = "test-user-123";

        [TestInitialize]
        public void TestInitialize()
        {
            _mockQuestionRepository = new Mock<IQuestionRepository>();

            _controller = new QuestionsController(_mockQuestionRepository.Object);

            var httpContext = new DefaultHttpContext();

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _testUserId) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            httpContext.User = principal;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        // --- 1. GET (Single Question) Tests ---

        [TestMethod]
        public async Task Get_ReturnsOk_WhenQuestionExists()
        {
            // Arrange
            var testId = _sampleQuestionDto.Id;
            _mockQuestionRepository.Setup(repo => repo.GetAsync(testId))
                .ReturnsAsync(_sampleQuestionDto);

            // Act
            var result = await _controller.Get(testId);

            // Assert (HTTP 200 OK)
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(_sampleQuestionDto, okResult.Value);
        }

        [TestMethod]
        public async Task Get_ReturnsNotFound_WhenQuestionDoesNotExist()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockQuestionRepository.Setup(repo => repo.GetAsync(testId))
                .ReturnsAsync((QuestionDTO?)null);

            // Act
            var result = await _controller.Get(testId);

            // Assert (HTTP 404 Not Found)
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // --- 2. CREATE (POST) Tests ---

        [TestMethod]
        public async Task Create_ReturnsOk_WhenCreationIsSuccessful()
        {
            // Arrange
            var createDto = new CreateQuestionDTO { Title = "New Test", Description = "Test", Difficulty = "Easy", Topic = "Array", Hint = "" };
            var returnedDto = new QuestionDTO { Id = Guid.NewGuid(), Title = createDto.Title, Difficulty = createDto.Difficulty, Topic = createDto.Topic };

            _mockQuestionRepository.Setup(repo => repo.CreateAsync(createDto))
                .ReturnsAsync(returnedDto);

            // Act
            var result = await _controller.Create(createDto);

            // Assert (HTTP 200 OK)
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            // Verify that the repository method was called
            _mockQuestionRepository.Verify(repo => repo.CreateAsync(createDto), Times.Once());
        }

        // --- 3. GET ALL (Query Parameter & Validation) Tests ---

        [TestMethod]
        public async Task GetAll_ReturnsBadRequest_ForInvalidFilterColumn()
        {
            // Arrange: Invalid column name (e.g., 'category')
            string invalidColumn = "category";

            // Act
            var result = await _controller.GetAll(invalidColumn, "test", null, true, 1, 10);

            // Assert (HTTP 400 Bad Request)
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsTrue(badRequestResult.Value.ToString().Contains("Invalid column to filter category. Allowed columns are: title, difficulty, topic, solved"));

            // Verify that the repository query was NOT called
            _mockQuestionRepository.Verify(repo => repo.GetAllAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public async Task GetAll_ReturnsBadRequest_ForInvalidSortColumn()
        {
            // Arrange: Invalid sort column name (only 'title' is allowed in the controller)
            string invalidSortBy = "description";

            // Act
            var result = await _controller.GetAll(null, null, invalidSortBy, true, 1, 10);

            // Assert (HTTP 400 Bad Request)
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsTrue(badRequestResult.Value.ToString().Contains("Invalid column to sortBy description. Allowed column is title"));

            // Verify that the repository query was NOT called
            _mockQuestionRepository.Verify(repo => repo.GetAllAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public async Task GetAll_ReturnsOk_WithValidParameters()
        {
            // Arrange
            var listDto = new List<QuestionDTO> { _sampleQuestionDto };
            _mockQuestionRepository.Setup(repo => repo.GetAllAsync("topic", "array", "title", true, 2, 5))
                .ReturnsAsync(listDto);

            // Act
            var result = await _controller.GetAll("topic", "array", "title", true, 2, 5);

            // Assert (HTTP 200 OK)
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            // Verify the repository method was called with the exact parameters
            _mockQuestionRepository.Verify(
                repo => repo.GetAllAsync("topic", "array", "title", true, 2, 5),
                Times.Once());
        }

        // --- 4. MarkSolved Tests (POST /api/questions/{questionId}/mark-solved) ---

        [TestMethod]
        public async Task MarkSolved_ReturnsOk_WhenRepositorySucceeds()
        {
            // Arrange
            var questionId = _sampleQuestionDto.Id;
            // The repository method MarkAsSolvedAsync is void/Task and handles authorization internally.
            _mockQuestionRepository.Setup(repo => repo.MarkAsSolvedAsync(questionId))
                                    .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.MarkSolved(questionId);

            // Assert (HTTP 200 OK)
            Assert.IsInstanceOfType(result, typeof(OkResult));

            // Verify that the repository method was called exactly once
            _mockQuestionRepository.Verify(
                repo => repo.MarkAsSolvedAsync(questionId),
                Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public async Task MarkSolved_ReturnsInternalServerError_WhenRepositoryThrowsUnauthorizedAccess()
        {
            // Arrange
            var questionId = Guid.NewGuid();
            _mockQuestionRepository.Setup(repo => repo.MarkAsSolvedAsync(questionId))
                .ThrowsAsync(new UnauthorizedAccessException("User context is missing."));

            // Act
            await _controller.MarkSolved(questionId);
        }

        // --- 5. SolvedQuestions Tests (GET /api/questions/solved) ---

        [TestMethod]
        public async Task SolvedQuestions_ReturnsOk_WithListOfSolvedQuestions()
        {
            // Arrange
            var solvedList = new List<QuestionDTO> { _sampleQuestionDto };
            _mockQuestionRepository.Setup(repo => repo.GetUserSolvedQuestionsAsync())
                                    .ReturnsAsync(solvedList);

            // Act
            var result = await _controller.SolvedQuestions();

            // Assert (HTTP 200 OK)
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            // Check that the returned value matches the mocked data
            Assert.AreEqual(solvedList, okResult.Value);

            // Verify the repository method was called
            _mockQuestionRepository.Verify(
                repo => repo.GetUserSolvedQuestionsAsync(),
                Times.Once());
        }

        // --- 6. Progress Tests (GET /api/questions/progress) ---

        [TestMethod]
        public async Task Progress_ReturnsOk_WithUserProgressData()
        {
            // Arrange
            var progressData = new Dictionary<string, TopicProgressDTO>
            {
                { "Array", new TopicProgressDTO { Solved = 5, Total = 10 } }
            };
            _mockQuestionRepository.Setup(repo => repo.GetProgressForUserAsync())
                                    .ReturnsAsync(progressData);

            // Act
            var result = await _controller.Progress();

            // Assert (HTTP 200 OK)
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;

            // Check that the returned value matches the mocked data
            Assert.AreEqual(progressData, okResult.Value);

            // Verify the repository method was called
            _mockQuestionRepository.Verify(
                repo => repo.GetProgressForUserAsync(),
                Times.Once());
        }
    }
}
