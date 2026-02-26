using DSAMate.API.Data;
using DSAMate.API.Models.Dtos;
using DSAMate.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DSAMate.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly RoleManager<IdentityRole> _rolemanager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthTokenService _authTokenService;
        public AuthController(RoleManager<IdentityRole> rolemanager, UserManager<IdentityUser> userManager, IAuthTokenService authTokenService) {
            _rolemanager = rolemanager;
            _userManager = userManager;
            _authTokenService = authTokenService;
        }
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO registerRequestDto)
        {
            var userEmail = registerRequestDto.EmailAddress;
            var existingUser = await _userManager.FindByEmailAsync(userEmail);

            if (existingUser != null)
            {
                return BadRequest(new DefaultResponseDTO { Response = $"An account with {userEmail} already exists" });
            }

            foreach (var role in registerRequestDto.Roles)
            {
                var isRoleValid = await _rolemanager.FindByNameAsync(role);
                if (isRoleValid == null)
                {
                    return BadRequest(new DefaultResponseDTO { Response = $"Role '{role}' does not exist" });
                }
            }

            var identityUser = new IdentityUser
            {
                UserName = userEmail,
                Email = userEmail
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
                    return Ok(new DefaultResponseDTO { Response = "User registration successful! Please login" });
                }
            }

            return BadRequest(identityResult.Errors.ToList()[0]);

        }

        [HttpPost]
        [Route("Unregister")]
        public async Task<IActionResult> Unregister([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDTO.EmailAddress);
            if (user == null)
            {
                return BadRequest(new DefaultResponseDTO { Response = "Incorrect email" });
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
            if (!passwordValid)
            {
                return BadRequest(new DefaultResponseDTO { Response = "Incorrect password" });
            }

            var result = await _userManager.DeleteAsync(user);
            if(result.Succeeded)
            {
                return Ok(new DefaultResponseDTO { Response = "User unregistered successfully" });
            }
            return BadRequest(new DefaultResponseDTO { Response = "Failed to unregister the user" });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> LoginRequest([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDTO.EmailAddress);
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
                return BadRequest(new DefaultResponseDTO { Response = "Incorrect password!" });
            }
            return BadRequest(new DefaultResponseDTO { Response = "Incorrect email" });
        }
    }
}
