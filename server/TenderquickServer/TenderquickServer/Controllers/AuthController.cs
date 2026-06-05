using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenderquickServer.Models;
using TenderquickServer.Models.Auth;
using TenderquickServer.Services;

namespace TenderquickServer.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        // POST api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
        {
            var result = await _auth.LoginAsync(req);
            if (result is null)
                return Unauthorized(new { message = "Invalid email or password." });
            return Ok(result);
        }

        // GET api/auth/me
        [HttpGet("me")]
        [Authorize]
        public ActionResult<UserDto> Me()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var id))
                return Unauthorized();

            return Ok(new UserDto(
                id,
                User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                User.FindFirstValue(ClaimTypes.Role) ?? Roles.Viewer));
        }

        // POST api/auth/users
        [HttpPost("users")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest req)
        {
            if (!Roles.IsValid(req.Role))
                return BadRequest(new { message = $"Invalid role '{req.Role}'." });

            var user = await _auth.CreateUserAsync(req);
            return CreatedAtAction(nameof(GetUsers), new { }, user);
        }

        // GET api/auth/users
        [HttpGet("users")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            return Ok(await _auth.GetUsersAsync());
        }
    }
}
