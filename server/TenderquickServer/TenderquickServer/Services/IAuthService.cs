using TenderquickServer.Models.Auth;

namespace TenderquickServer.Services
{
    public enum CreateUserOutcome { Created, DuplicateEmail, InvalidRole }

    public record CreateUserResult(CreateUserOutcome Outcome, UserDto? User);

    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest req);
        Task<CreateUserResult> CreateUserAsync(CreateUserRequest req);
        Task<IEnumerable<UserDto>> GetUsersAsync();
        UserDto? GetById(int id);
    }
}
