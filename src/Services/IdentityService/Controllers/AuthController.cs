using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authservice;

        public AuthController(IAuthService authService)
        {
            _authservice=authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result= await _authservice.LoginAsync(request);

            if(result == null) return Unauthorized(ApiResponse<object>.Fail("Invalid mail or password"));

            return Ok(ApiResponse<LoginResponse>.Ok(result));
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users=_authservice.GetAllUsers();
            return Ok(ApiResponse<IEnumerable<User>>.Ok(users));
        }
    }
}