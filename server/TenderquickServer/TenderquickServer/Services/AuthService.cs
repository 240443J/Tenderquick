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
            var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password ?? string.Empty, user.PasswordHash))
                return null;

            // Claims aren't populated during login itself, so pass the actor explicitly.
            await _audit.LogAsAsync(user.Id, user.Name, "Auth.Login", "User", user.Id, new { user.Email });
            return new AuthResponse(GenerateToken(user), ToDto(user));
        }

        public async Task<CreateUserResult> CreateUserAsync(CreateUserRequest req)
        {
            if (!Roles.IsValid(req.Role))
                return new CreateUserResult(CreateUserOutcome.InvalidRole, null);

            var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
            if (await _db.Users.AnyAsync(u => u.Email == email))
                return new CreateUserResult(CreateUserOutcome.DuplicateEmail, null);

            var user = new User
            {
                Name = (req.Name ?? string.Empty).Trim(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = req.Role,
                CreatedAt = DateTime.UtcNow,
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("User.Created", "User", user.Id, new { user.Email, user.Role });
            return new CreateUserResult(CreateUserOutcome.Created, ToDto(user));
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync() =>
            await _db.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Select(u => new UserDto(u.Id, u.Name, u.Email, u.Role))
                .ToListAsync();

        public UserDto? GetById(int id)
        {
            var user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
            return user is null ? null : ToDto(user);
        }

        private string GenerateToken(User user)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
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
