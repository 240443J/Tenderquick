using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TenderquickServer.Data;
using TenderquickServer.Models;
using TenderquickServer.Models.Auth;

namespace TenderquickServer.Services
{
    public class AuthService : IAuthService
    {
        private readonly InMemoryStore _store;
        private readonly IConfiguration _config;
        private readonly IAuditService _audit;

        public AuthService(InMemoryStore store, IConfiguration config, IAuditService audit)
        {
            _store = store;
            _config = config;
            _audit = audit;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest req)
        {
            var user = _store.Users.Values.FirstOrDefault(u =>
                string.Equals(u.Email, req.Email, StringComparison.OrdinalIgnoreCase));

            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return null;

            // Claims aren't populated during login itself, so pass the actor explicitly.
            await _audit.LogAsAsync(user.Id, user.Name, "Auth.Login", "User", user.Id, new { user.Email });
            return new AuthResponse(GenerateToken(user), ToDto(user));
        }

        public async Task<CreateUserResult> CreateUserAsync(CreateUserRequest req)
        {
            if (!Roles.IsValid(req.Role))
                return new CreateUserResult(CreateUserOutcome.InvalidRole, null);

            var exists = _store.Users.Values.Any(u =>
                string.Equals(u.Email, req.Email, StringComparison.OrdinalIgnoreCase));
            if (exists)
                return new CreateUserResult(CreateUserOutcome.DuplicateEmail, null);

            var user = new User
            {
                Id = _store.NextUserId(),
                Name = req.Name,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = req.Role,
                CreatedAt = DateTime.UtcNow,
            };
            _store.Users[user.Id] = user;

            await _audit.LogAsync("User.Created", "User", user.Id, new { user.Email, user.Role });
            return new CreateUserResult(CreateUserOutcome.Created, ToDto(user));
        }

        public Task<IEnumerable<UserDto>> GetUsersAsync() =>
            Task.FromResult(_store.Users.Values.OrderBy(u => u.Id).Select(ToDto));

        public UserDto? GetById(int id) =>
            _store.Users.TryGetValue(id, out var user) ? ToDto(user) : null;

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
