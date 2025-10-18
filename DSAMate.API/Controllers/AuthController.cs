using DSAMate.API.Models.Dtos;
using DSAMate.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace DSAMate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthTokenService _authTokenService;
        public AuthController(UserManager<IdentityUser> userManager, IAuthTokenService authTokenService) {
            _userManager = userManager;
            _authTokenService = authTokenService;
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerRequestDto)
        {
            var identityUser = new IdentityUser
            {
                UserName = registerRequestDto.UserName,
                Email = registerRequestDto.UserName
            };

            var identityResult = await _userManager.CreateAsync(identityUser, registerRequestDto.Password);

            if (identityResult.Succeeded)
            {
                if (registerRequestDto.Roles.Any())
                {
                    identityResult = await _userManager.AddToRolesAsync(identityUser, registerRequestDto.Roles);
                }
                if (identityResult.Succeeded)
                {
                    return Ok("User registration successful! Please login");
                }
            }

            return BadRequest("Something went wrong");

        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> LoginRequest([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDTO.Username);
            if (user != null)
            {
                var passwordValidation = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
                if (passwordValidation)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // Token Generation
                    var jwtToken = _authTokenService.GetAuthToken(user, roles.ToList());
                    var response = new LoginResponseDTO { JwtToken = jwtToken };

                    return Ok(response);
                }
                return BadRequest("Incorrect password!");
            }
            return BadRequest("Incorrect username");
        }
    }
}
