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
            // Generic message — no user enumeration.
            return result is null ? Unauthorized(new { message = "Invalid email or password." }) : Ok(result);
        }

        // GET api/auth/me
        [HttpGet("me")]
        [Authorize]
        public ActionResult<UserDto> Me()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(id, out var userId)) return Unauthorized();

            var dto = _auth.GetById(userId);
            return dto is null ? Unauthorized() : Ok(dto);
        }

        // POST api/auth/users  (Admin only)
        [HttpPost("users")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest req)
        {
            var result = await _auth.CreateUserAsync(req);
            return result.Outcome switch
            {
                CreateUserOutcome.Created => CreatedAtAction(nameof(GetUsers), result.User),
                CreateUserOutcome.DuplicateEmail => Conflict(new { message = "Email already in use." }),
                CreateUserOutcome.InvalidRole => BadRequest(new { message = "Invalid role." }),
                _ => BadRequest(),
            };
        }

        // GET api/auth/users  (Admin only)
        [HttpGet("users")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            return Ok(await _auth.GetUsersAsync());
        }
    }
}
