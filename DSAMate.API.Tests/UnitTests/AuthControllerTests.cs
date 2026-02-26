using DSAMate.API.Controllers;
using DSAMate.API.Data;
using DSAMate.API.Models.Dtos;
using DSAMate.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json.Linq;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace DSAMate.API.Tests.UnitTests
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<IAuthTokenService> _mockAuthTokenService;
        private AuthController _controller;

        // Sample data
        private readonly IdentityUser _sampleUser = new IdentityUser { Id = "test-id-123", UserName = "test@user.com", Email = "test@user.com" };
        private readonly List<string> _sampleRoles = new List<string> { "User" };

        [TestInitialize]
        public void TestInitialize()
        {
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            _mockAuthTokenService = new Mock<IAuthTokenService>();

            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
        Mock.Of<IRoleStore<IdentityRole>>(), // Required: RoleStore
        null, // Optional: IRoleValidators<TRole>[]
        null, // Optional: ILookupNormalizer
        null, // Optional: IdentityErrorDescriber
        null // Optional: ILogger<RoleManager<TRole>>
    );

            _controller = new AuthController(_mockRoleManager.Object, _mockUserManager.Object, _mockAuthTokenService.Object);

        }

        // --- REGISTER (POST /Register) Tests ---

        [TestMethod]
        public async Task Register_ReturnsOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var registerDto = new RegisterRequestDTO
            {
                EmailAddress = "new@user.com",
                Password = "Password1",
                Roles = ["User"]
            };

            // Setup 1: User creation succeeds
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Setup 2: Adding user to roles succeeds
            _mockUserManager.Setup(um => um.AddToRolesAsync(It.IsAny<IdentityUser>(), registerDto.Roles))
                .ReturnsAsync(IdentityResult.Success);

            // Setup 3: Mock RoleManager to return a valid object for the valid role (if needed)
            _mockRoleManager
                .Setup(rm => rm.FindByNameAsync("User"))
                .ReturnsAsync(new IdentityRole { Name = "User" });

            // Act
            var result = await _controller.Register(registerDto);

            // Assert (HTTP 200 OK)
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual("User registration successful! Please login", ((result as OkObjectResult)?.Value as DefaultResponseDTO).Response);

            // Verify creation and role assignment calls
            _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<IdentityUser>(), registerDto.Password), Times.Once());
            _mockUserManager.Verify(um => um.AddToRolesAsync(It.IsAny<IdentityUser>(), registerDto.Roles), Times.Once());
        }

        [TestMethod]
        public async Task Register_ReturnsBadRequest_WhenUserCreationFailed()
        {
            // Arrange
            var registerDto = new RegisterRequestDTO { EmailAddress = "fail@user.com", Password = "P", Roles = ["User"] };
            var errors = new IdentityError { Description = "Password too weak" };

            // Setup: User creation fails
            _mockUserManager
            .Setup(um => um.FindByEmailAsync(registerDto.EmailAddress))
            .ReturnsAsync((IdentityUser?)null);

            _mockRoleManager
                        .Setup(um => um.FindByNameAsync("User"))
                        .ReturnsAsync(new IdentityRole { Name = "User"});

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed(errors));


            // Act
            var result = await _controller.Register(registerDto);

            // Assert (HTTP 400 Bad Request)
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Password too weak", ((result as BadRequestObjectResult)?.Value as IdentityError).Description);

            // Verify no role assignment was attempted
            _mockUserManager.Verify(um => um.AddToRolesAsync(It.IsAny<IdentityUser>(), It.IsAny<string[]>()), Times.Never());
        }

        [TestMethod]
        public async Task Register_ReturnsBadRequest_WhenRoleAssignmentFailed()
        {
            // Arrange
            var registerDto = new RegisterRequestDTO { EmailAddress = "rolefail@user.com", Password = "P", Roles = ["InvalidRole"] };
            var errors = new IdentityError { Description = "Role not found" };

            // Setup 1: User creation succeeds
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);

            // Setup 2: Role assignment fails
            _mockUserManager.Setup(um => um.AddToRolesAsync(It.IsAny<IdentityUser>(), registerDto.Roles))
                .ReturnsAsync(IdentityResult.Failed(errors));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert (HTTP 400 Bad Request)
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual("Role 'InvalidRole' does not exist", ((result as BadRequestObjectResult)?.Value as DefaultResponseDTO).Response);
        }

        // --- LOGIN (POST /Login) Tests ---

        [TestMethod]
        public async Task Login_ReturnsOk_WithJwtToken_WhenCredentialsAreValid()
        {
            // Arrange
            var loginDto = new LoginRequestDTO { EmailAddress = _sampleUser.Email!, Password = "ValidPassword" };
            const string expectedToken = "MockJwtTokenString";

            // Setup 1: Find user by email succeeds
            _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.EmailAddress))
                .ReturnsAsync(_sampleUser);

            // Setup 2: Password check succeeds
            _mockUserManager.Setup(um => um.CheckPasswordAsync(_sampleUser, loginDto.Password))
                .ReturnsAsync(true);

            // Setup 3: Get roles succeeds
            _mockUserManager.Setup(um => um.GetRolesAsync(_sampleUser))
                .ReturnsAsync(_sampleRoles);

            // Setup 4: Token generation succeeds
            _mockAuthTokenService.Setup(ats => ats.GetAuthToken(_sampleUser, _sampleRoles))
                .Returns(expectedToken);

            // Act
            var result = await _controller.LoginRequest(loginDto);

            // Assert (HTTP 200 OK with token)
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var response = (result as OkObjectResult)?.Value as LoginResponseDTO;

            Assert.IsNotNull(response);
            Assert.AreEqual(expectedToken, response.JwtToken);

            // Verify token service was called
            _mockAuthTokenService.Verify(ats => ats.GetAuthToken(_sampleUser, _sampleRoles), Times.Once());
        }

        [TestMethod]
        public async Task Login_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            // Arrange
            var loginDto = new LoginRequestDTO { EmailAddress = "nonexistent@user.com", Password = "P" };

            // Setup: Find user returns null
            _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.EmailAddress))
                .ReturnsAsync((IdentityUser?)null);

             // Act
            var result = await _controller.LoginRequest(loginDto);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestObject = result as BadRequestObjectResult;

            // Assert (HTTP 400 Bad Request)
            Assert.IsNotNull((result as BadRequestObjectResult)?.Value);

            var responseContent = badRequestObject.Value as DefaultResponseDTO;

            Assert.AreEqual("Incorrect email", responseContent.Response);

            // Verify no password check was performed
            _mockUserManager.Verify(um => um.CheckPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public async Task Login_ReturnsBadRequest_WhenPasswordIsIncorrect()
        {
            // Arrange
            var loginDto = new LoginRequestDTO { EmailAddress = _sampleUser.Email!, Password = "WrongPassword" };

            // Setup 1: Find user succeeds
            _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.EmailAddress))
                .ReturnsAsync(_sampleUser);

            // Setup 2: Password check fails
            _mockUserManager.Setup(um => um.CheckPasswordAsync(_sampleUser, loginDto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.LoginRequest(loginDto);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestObject = result as BadRequestObjectResult;

            // Assert (HTTP 400 Bad Request)
            Assert.IsNotNull((result as BadRequestObjectResult)?.Value);

            var responseContent = badRequestObject.Value as DefaultResponseDTO;

            Assert.AreEqual("Incorrect password!", responseContent.Response);

            // Verify token service was NOT called
            _mockAuthTokenService.Verify(ats => ats.GetAuthToken(It.IsAny<IdentityUser>(), It.IsAny<List<string>>()), Times.Never());
        }
    }
}
