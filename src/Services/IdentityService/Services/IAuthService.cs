using IdentityService.Models;

namespace IdentityService.Services
{
    public interface IAuthService
    {
         Task<LoginResponse?> LoginAsync(LoginRequest request);

         User? GetUserById(string userId);

         IEnumerable<User> GetAllUsers();
    }
}