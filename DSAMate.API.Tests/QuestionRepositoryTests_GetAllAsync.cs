using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DSAMate.API.Data;
using DSAMate.API.Data.Domains;
using DSAMate.API.Models.Dtos;
using DSAMate.API.Models;
using DSAMate.API.Models.Domains;
using DSAMate.API.Repositories;
using DSAMate.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DSAMate.API.Tests
{
    [TestClass]
    public class QuestionRepository_GetAllAsync_Tests
    {
        private AppDbContext _context = null!;
        private QuestionRepository _repository = null!;
        private Mock<IUserContextService> _userContextMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private string _userId;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            _userContextMock = new Mock<IUserContextService>();
            _mapperMock = new Mock<IMapper>();

            // default user id for solved-question mapping tests
            _userId = "TestUser123";
            _userContextMock.Setup(x => x.GetUserId()).Returns(_userId);

            // mapper: map Question -> QuestionDTO using the question values
            _mapperMock
                .Setup(m => m.Map<QuestionDTO>(It.IsAny<Question>()))
                .Returns<Question>(q => new QuestionDTO
                {
                    Id = q.Id,
                    Title = q.Title,
                    Hint = q.Hint,
                    Difficulty = q.Difficulty.ToString(),
                    Topic = q.Topic.ToString(),
                    Solved = false,
                    SolvedAt = null
                });

            _repository = new QuestionRepository(_context, _mapperMock.Object, _userContextMock.Object);

            SeedData().GetAwaiter().GetResult();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private async Task SeedData()
        {
            // create 6 questions to test pagination/sort/filter
            var questions = new List<Question>
            {
                new Question { Id = Guid.NewGuid(), Title = "Alpha Problem", Hint = "hint A", Difficulty = Difficulty.Easy, Topic = Topic.Array, Description = "Desc" },
                new Question { Id = Guid.NewGuid(), Title = "Beta Problem",  Hint = "hint B", Difficulty = Difficulty.Medium, Topic = Topic.String, Description = "Desc" },
                new Question { Id = Guid.NewGuid(), Title = "Gamma Problem", Hint = "hint C", Difficulty = Difficulty.Hard, Topic = Topic.Graph, Description = "Desc" },
                new Question { Id = Guid.NewGuid(), Title = "Delta Problem", Hint = "hint D", Difficulty = Difficulty.Easy, Topic = Topic.Array, Description = "Desc" },
                new Question { Id = Guid.NewGuid(), Title = "Epsilon Problem", Hint = "hint E", Difficulty = Difficulty.Medium, Topic = Topic.Math, Description = "Desc" },
                new Question { Id = Guid.NewGuid(), Title = "Zeta Problem", Hint = "hint Z", Difficulty = Difficulty.Hard, Topic = Topic.Tree, Description = "Desc" }
            };

            await _context.Questions.AddRangeAsync(questions);

            // mark one as solved by current user
            var solved = new UserQuestionStatus
            {
                UserId = _userId,
                QuestionId = questions[1].Id, // mark "Beta Problem" as solved
                IsSolved = true,
                SolvedAt = DateTime.UtcNow,
                Question = questions[1]
            };
            await _context.UserQuestionStatuses.AddAsync(solved);

            await _context.SaveChangesAsync();
        }

        [TestMethod]
        public async Task GetAllAsync_ReturnsPagedResults_WithDefaultOrdering()
        {
            // pageSize = 2, pageNumber = 2 -> should return 2 items: 3rd and 4th in default order
            var result = await _repository.GetAllAsync(column: null, query: null, sortBy: null, isAscending: true, pageNumber: 2, pageSize: 2);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            // because repository doesn't define a default order, the in-memory DB preserves insertion order;
            // based on seed, the 3rd item is "Gamma Problem"
            Assert.IsTrue(result.Any(r => r.Title == "Gamma Problem"));
            Assert.IsTrue(result.Any(r => r.Title == "Delta Problem"));
            // mandatory fields mapped by mocked mapper
            var sample = result.First();
            Assert.IsFalse(string.IsNullOrWhiteSpace(sample.Hint));
            Assert.IsTrue(Enum.IsDefined(typeof(Difficulty), sample.Difficulty));
        }

        [TestMethod]
        public async Task GetAllAsync_Filters_ByTitle_Contains_IgnoringCase()
        {
            // search for "epsilon" should match "Epsilon Problem"
            var result = await _repository.GetAllAsync(column: "title", query: "epsilon", sortBy: null, isAscending: true, pageNumber: 1, pageSize: 10);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Epsilon Problem", result[0].Title);
            Assert.AreEqual("hint E", result[0].Hint);
            Assert.AreEqual("Medium", result[0].Difficulty);
        }

        [TestMethod]
        public async Task GetAllAsync_Sorts_ByTitle_Descending()
        {
            // sortBy title, descending => first should be "Zeta Problem"
            var result = await _repository.GetAllAsync(column: null, query: null, sortBy: "title", isAscending: false, pageNumber: 1, pageSize: 10);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count >= 1);
            Assert.AreEqual("Zeta Problem", result.First().Title);
        }

        [TestMethod]
        public async Task GetAllAsync_MarksSolvedQuestions_ForCurrentUser()
        {
            var result = await _repository.GetAllAsync(column: null, query: null, sortBy: null, isAscending: true, pageNumber: 1, pageSize: 20);

            // "Beta Problem" was marked solved in SeedData
            var solvedDto = result.FirstOrDefault(r => r.Title == "Beta Problem");
            Assert.IsNotNull(solvedDto);
            Assert.IsTrue(solvedDto.Solved);
            Assert.IsNotNull(solvedDto.SolvedAt);
        }
    }
}
