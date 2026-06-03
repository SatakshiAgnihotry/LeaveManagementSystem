using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context=context;
            _configuration=configuration;
        }
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user= await _context.Users.FirstOrDefaultAsync(u => u.Email== request.Email.ToLower());

            if(user == null )return null;

            if(!BCrypt.Net.BCrypt.Verify(request.Password,user.PasswordHash))return null;

            var token= GenerateToken(user);
            var expiesAt= DateTime.UtcNow.AddHours(8);

            return new LoginResponse
            {
                Token=token,
                UserId=user.UserId,
                Name=user.Name,
                Email=user.Email,
                Role=user.Role,
                ManagerId=user.ManagerId,
                ExpiresAt=expiesAt
            };

        }

        public User? GetUserById(string userId)
        {
            return _context.Users.Find(userId);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        private string GenerateToken(User user)
        {
            var secretKey= _configuration["Jwt:SecretKey"]!;
            var issuer= _configuration["Jwt:Issuer"]!;
            var audience= _configuration["Jwt:Audience"]!;

            var claims= new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.UserId),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name,user.Name),
                new Claim(ClaimTypes.Role,user.Role),
                new Claim("managerId", user.ManagerId ?? "")
            };

            var key= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var token=new JwtSecurityToken(
                issuer:issuer,
                audience:audience,
                claims: claims,
                expires:DateTime.UtcNow.AddHours(8),
                signingCredentials:signingCredentials

            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}