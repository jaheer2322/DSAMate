using AutoMapper;
using DSAMate.API.Data;
using DSAMate.API.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using DSAMate.API.Services;
using DSAMate.API.Data.Domains;
using DSAMate.API.Models.Dtos;
using DSAMate.API.Models.Domains;

namespace DSAMate.API.Tests;

[TestClass]
public class QuestionRepositoryTests
{
    private AppDbContext _dbContext;
    private Mock<IMapper> _mapperMock;
    private Mock<IUserContextService> _userContextServiceMock;
    private string _testUserId;
    private QuestionRepository _questionRepository;

    [TestInitialize]
    public async Task TestInitialize()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"TestDb{Guid.NewGuid()}")
            .Options;
        _dbContext = new AppDbContext(options);
        _userContextServiceMock = new Mock<IUserContextService>();
        _mapperMock = new Mock<IMapper>();

        _testUserId = "TestUser123";
        _userContextServiceMock.Setup(x => x.GetUserId()).Returns(_testUserId);

        _questionRepository = new QuestionRepository(_dbContext, _mapperMock.Object, _userContextServiceMock.Object);
    }

    [TestCleanup]
    public void CleanUp()
    {
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task GetAsync_ReturnsMappedDto_WhenQuestionExists()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var question = new Question { Id = questionId, Title = "Two Sum", Description = "Desc", Hint = "Hint"};
        var expectedDTO = new QuestionDTO { Id = questionId, Title = "Two Sum", Description = "Desc", Hint = "Hint"};
        _mapperMock.Setup(m => m.Map<QuestionDTO>(question)).Returns(expectedDTO);
        await _dbContext.Questions.AddAsync(question);
        await _dbContext.SaveChangesAsync();

        // Act
        var questionDTO = await _questionRepository.GetAsync(questionId);

        // Assert
        Assert.IsNotNull(questionDTO);
        Assert.IsInstanceOfType(questionDTO, typeof(QuestionDTO));
        Assert.IsTrue(questionDTO.Id == question.Id);
        Assert.IsTrue(questionDTO.Title == question.Title);
    }
    [TestMethod]
    public async Task GetAsync_ReturnsNull_WhenNoQuestionExists()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        
        // Act
        var questionDTO = await _questionRepository.GetAsync(questionId);

        // Assert
        Assert.IsNull(questionDTO);
    }
    [TestMethod]
    public async Task CreateAsync_AddsQuestion_WhenNotExists()
    {
        // Arrange
        var createQuestionDTO = new CreateQuestionDTO { Title = "TestQuestion", Description = "Desc", Difficulty = "Easy", Hint = "HInt" };
        var question = new Question { Id=Guid.NewGuid(), Title = "TestQuestion", Description = "Desc", Difficulty = Difficulty.Easy, Hint = "HInt" };
        var mappedDTO = new QuestionDTO { Id = question.Id, Title = "TestQuestion", Description = "Desc", Difficulty = "Easy", Hint = "HInt" };
        _mapperMock.Setup(m => m.Map<Question>(createQuestionDTO)).Returns(question);
        _mapperMock.Setup(m => m.Map<QuestionDTO>(question)).Returns(mappedDTO);

        // Act
        var questionDTO = await _questionRepository.CreateAsync(createQuestionDTO);

        // Assert
        Assert.AreEqual(1, _dbContext.Questions.Count());
        Assert.IsNotNull(questionDTO);
        Assert.IsInstanceOfType(questionDTO, typeof(QuestionDTO));
        Assert.AreEqual(questionDTO.Title, question.Title);
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException), "Question already exists")]
    public async Task CreateAsync_Throws_WhenQuestionExists()
    {
        // Arrange
        var question = new Question { Id = Guid.NewGuid(), Title = "TestQuestion", Description = "Desc", Difficulty = Difficulty.Easy, Hint = "HInt" };
        await _dbContext.Questions.AddAsync(question);
        await _dbContext.SaveChangesAsync();

        var createQuestionDTO = new CreateQuestionDTO { Title = "TestQuestion", Description = "Desc", Difficulty = "Easy", Hint = "HInt" };

        // Act
        var questionDTO = await _questionRepository.CreateAsync(createQuestionDTO);
    }
    [TestMethod]
    public async Task CreateBulkAsync_AddsMultiple_WhenValid()
    {
        // Arrange
        var dtos = new List<CreateQuestionDTO>
            {
                new() { Title = "Q1", Description = "Desc", Difficulty = "Easy", Hint = "HInt" },
                new() { Title = "Q2", Description = "Desc", Difficulty = "Easy", Hint = "HInt" }
            };
        var entities = new List<Question>
            {
                new() { Id = Guid.NewGuid(), Title = "Q1", Description = "Desc", Difficulty = Difficulty.Easy, Hint = "HInt"  },
                new() { Id = Guid.NewGuid(), Title = "Q2", Description = "Desc", Difficulty = Difficulty.Easy, Hint = "HInt"  }
            };
        var mappedDtos = entities.Select(q => new QuestionDTO { Id = q.Id, Title = q.Title, Description = "Desc", Difficulty = "Easy", Hint = "HInt" }).ToList();

        _mapperMock.Setup(m => m.Map<List<Question>>(dtos)).Returns(entities);
        _mapperMock.Setup(m => m.Map<List<QuestionDTO>>(entities)).Returns(mappedDtos);

        // Act
        var result = await _questionRepository.CreateBulkAsync(dtos);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(2, _dbContext.Questions.Count());
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task CreateBulkAsync_Throws_WhenDuplicateTitlesInRequest()
    {
        // Arrange
        var dtos = new List<CreateQuestionDTO>
            {
                new() { Title = "Same" },
                new() { Title = "Same" }
            };

        // Act
        await _questionRepository.CreateBulkAsync(dtos);
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task CreateBulkAsync_Throws_WhenTitleAlreadyExists()
    {
        // Arrange
        await _dbContext.Questions.AddAsync(new Question { Title = "Same", Description = "Desc", Difficulty = Difficulty.Easy, Hint = "HInt" });
        await _dbContext.SaveChangesAsync();

        var dto = new List<CreateQuestionDTO> 
            {
                new() {Title = "Same", Description = "Desc", Difficulty = "Easy", Hint = "HInt" }
            };

        // Act
        await _questionRepository.CreateBulkAsync(dto);
    }
    [TestMethod]
    public async Task MarkAsSolvedAsync_AddsUserQuestionStatus_WhenNew()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var question = new Question { Id = questionId, Title = "Q1", Description = "Desc", Difficulty = Difficulty.Easy, Hint = "HInt" };
        await _dbContext.Questions.AddAsync(question);
        await _dbContext.SaveChangesAsync();
        // Act
        await _questionRepository.MarkAsSolvedAsync(questionId);

        // Assert
        var uqs = await _dbContext.UserQuestionStatuses.FirstOrDefaultAsync(uqs => uqs.QuestionId == questionId && uqs.UserId == _testUserId);
        var expectedTime = DateTime.UtcNow;
        Assert.IsNotNull(uqs);
        Assert.IsTrue(uqs.IsSolved);
        Assert.IsNotNull(uqs.SolvedAt);
        Assert.IsTrue(uqs.SolvedAt?.ToUniversalTime().Subtract(expectedTime).Duration().TotalSeconds < 1);
    }
    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException))]
    public async Task MarkAsSolvedAsync_Throws_WhenUserIdNull()
    {
        // Arrange
        string? userId = null;
        _userContextServiceMock.Setup(u => u.GetUserId()).Returns(userId);
        var questionId = Guid.NewGuid();
        var question = new Question { Id = questionId, Title = "Q1", Description = "Desc", Difficulty = Difficulty.Easy, Hint = "HInt" };
        await _dbContext.Questions.AddAsync(question);
        await _dbContext.SaveChangesAsync();

        // Act
        await _questionRepository.MarkAsSolvedAsync(questionId);
    }
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException), "Question doesn't exist")]
    public async Task MarkAsSolvedAsync_Throws_WhenQuestionIdInvalid()
    {
        // Arrange
        _userContextServiceMock.Setup(u => u.GetUserId()).Returns(_testUserId);
        var questionId = Guid.NewGuid();
        var question = new Question { Id = questionId, Title = "Q1", Description = "Desc", Difficulty = Difficulty.Easy, Hint = "HInt" };

        // Act
        await _questionRepository.MarkAsSolvedAsync(questionId);
    }
    [TestMethod]
    public async Task GetUserSolvedQuestionsAsync_ReturnsMappedDTOs()
    {
        // Arrange
        var question = new Question { Id = Guid.NewGuid(), Title = "Q1", Description = "Desc", Difficulty = Difficulty.Easy, Hint = "HInt" };
        await _dbContext.Questions.AddAsync(question);
        // Adding another question
        await _dbContext.Questions.AddAsync(new Question { Id = Guid.NewGuid(), Title = "Q2", Description = "Desc", Difficulty = Difficulty.Easy, Hint = "HInt" });
        var isSolved = true;
        var solvedAt = DateTime.UtcNow;
        await _dbContext.UserQuestionStatuses.AddAsync(new UserQuestionStatus
        {
            UserId = _testUserId,
            QuestionId = question.Id,
            IsSolved = isSolved,
            SolvedAt = solvedAt
        });
        await _dbContext.SaveChangesAsync();
        var dto = new QuestionDTO { Id = question.Id, Title = "Q1", Description = "Desc", Difficulty = "Easy", Hint = "HInt", Solved = isSolved, SolvedAt = solvedAt };
        _mapperMock.Setup(m => m.Map<QuestionDTO>(It.IsAny<Question>())).Returns(dto);

        // Act
        var questionDTOList = await _questionRepository.GetUserSolvedQuestionsAsync();

        // Assert
        Assert.AreEqual(questionDTOList.Count(), 1);
        var expectedTime = DateTime.UtcNow;
        var solvedQuestion = questionDTOList.First();
        Assert.IsTrue(solvedQuestion.Solved);
        Assert.IsNotNull(solvedQuestion.SolvedAt);
        Assert.IsTrue(solvedQuestion.SolvedAt?.ToUniversalTime().Subtract(expectedTime).Duration().TotalSeconds < 1);
    }
    [TestMethod]
    public async Task GetProgressForUserAsync_ReturnsProgressPerTopic()
    {
        // Arrange
        var q1 = new Question { Id = Guid.NewGuid(), Title = "Q1", Topic = Topic.Array, Description = "Desc", Hint = "Hint" };
        var q2 = new Question { Id = Guid.NewGuid(), Title = "Q2", Topic = Topic.String, Description = "Desc", Hint = "Hint" };
        _dbContext.Questions.AddRange(q1, q2);
        _dbContext.UserQuestionStatuses.Add(new UserQuestionStatus
        {
            UserId = _testUserId,
            QuestionId = q1.Id,
            Question = q1,
            IsSolved = true,
            SolvedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var progress = await _questionRepository.GetProgressForUserAsync();

        // Assert
        Assert.AreEqual(2, progress.Count);
        Assert.AreEqual(1, progress["Array"].Solved);
        Assert.AreEqual(1, progress["Array"].Total);
        Assert.AreEqual(0, progress["String"].Solved);
        Assert.AreEqual(1, progress["String"].Total);
    }
}