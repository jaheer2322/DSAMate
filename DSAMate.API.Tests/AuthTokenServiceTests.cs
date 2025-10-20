using System.Security.Claims;
using DSAMate.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DSAMate.API.Tests
{
    [TestClass]
    public sealed class AuthTokenServiceTests
    {
        private readonly IdentityUser _user = new IdentityUser
        {
            Email = "test@example.com",
            UserName = "test@example.com",
            Id = Guid.NewGuid().ToString()
        };
        
        private readonly IConfiguration _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Jwt:Key", "thisisalongkeyforjwttokengenerationwhichisatleast256bit"},
                {"Jwt:Issuer","DSAMateTestIssuer"},
                {"Jwt:Audience","DSAMateTestAudience"}
            })
            .Build();

        private readonly List<string> _testRoles = ["Admin", "User"];
        [TestMethod]
        public void GetAuthToken_WithValidRoles_ReturnsTokenWithCorrectClaims()
        {
            // Arrange
            var _service = new AuthTokenService(_config);

            // Act
            var tokenString = _service.GetAuthToken(_user, _testRoles);
            var handler = new JsonWebTokenHandler();
            var token = handler.ReadJsonWebToken(tokenString);

            // Assert
            Assert.IsNotNull(token);

            Assert.AreEqual("DSAMateTestIssuer", token.Issuer);
            Assert.AreEqual("DSAMateTestAudience", token.Audiences.First());

            var expectedTime = DateTime.UtcNow.AddDays(1);
            Assert.IsTrue(token.ValidTo.ToUniversalTime().Subtract(expectedTime).Duration().TotalSeconds < 5);

            var emailClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            Assert.IsNotNull(emailClaim);
            Assert.AreEqual(_user.Email, emailClaim.Value);

            var roleClaims = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            Assert.IsNotNull(roleClaims);
            CollectionAssert.AreEquivalent(roleClaims, _testRoles);
        }
    }
}
