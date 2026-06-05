using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TenderquickServer.Data;
using TenderquickServer.Models;
using TenderquickServer.Models.Auth;

namespace TenderquickServer.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IAuditService _audit;

        public AuthService(AppDbContext db, IConfiguration config, IAuditService audit)
        {
            _db = db;
            _config = config;
            _audit = audit;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest req)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return null;

            var token = GenerateToken(user);
            await _audit.LogAsync("Auth.Login", "User", user.Id);
            return new AuthResponse(token, ToDto(user));
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest req)
        {
            var user = new User
            {
                Name = req.Name.Trim(),
                Email = req.Email.Trim().ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = req.Role,
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("User.Created", "User", user.Id, new { user.Email, user.Role });
            return ToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            return await _db.Users
                .OrderBy(u => u.Name)
                .Select(u => new UserDto(u.Id, u.Name, u.Email, u.Role))
                .ToListAsync();
        }

        public string GenerateToken(User user)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireMinutes"] ?? "1440")),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UserDto ToDto(User u) => new(u.Id, u.Name, u.Email, u.Role);
    }
}
