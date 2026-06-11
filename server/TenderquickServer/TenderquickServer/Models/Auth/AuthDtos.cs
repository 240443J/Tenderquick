namespace TenderquickServer.Models.Auth
{
    public record LoginRequest(string Email, string Password);
    public record UserDto(int Id, string Name, string Email, string Role);
    public record AuthResponse(string Token, UserDto User);
    public record CreateUserRequest(string Name, string Email, string Password, string Role);
}
