using TenderquickServer.Models;
using TenderquickServer.Models.Auth;

namespace TenderquickServer.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest req);
        Task<UserDto> CreateUserAsync(CreateUserRequest req);
        Task<IEnumerable<UserDto>> GetUsersAsync();
        string GenerateToken(User user);
    }
}
