using Microsoft.AspNetCore.Identity;

namespace DSAMate.API.Services
{
    public interface IAuthTokenService
    {
        string GetAuthToken(IdentityUser identityUser, List<string> roles);
    }
}
