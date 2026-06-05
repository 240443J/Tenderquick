using System.ComponentModel.DataAnnotations;

namespace TenderquickServer.Models.Auth
{
    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class CreateUserRequest
    {
        [Required, MinLength(2), MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = Roles.Viewer;
    }

    public record UserDto(int Id, string Name, string Email, string Role);

    public record AuthResponse(string Token, UserDto User);
}
