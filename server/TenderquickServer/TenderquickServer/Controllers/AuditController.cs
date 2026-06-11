using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenderquickServer.Models;
using TenderquickServer.Services;

namespace TenderquickServer.Controllers
{
    [ApiController]
    [Route("api/audit")]
    [Authorize(Roles = Roles.Admin)]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _audit;

        public AuditController(IAuditService audit)
        {
            _audit = audit;
        }

        // GET api/audit/recent?limit=50
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<AuditLog>>> Recent([FromQuery] int limit = 50)
        {
            limit = Math.Clamp(limit, 1, 200);
            return Ok(await _audit.GetRecentAsync(limit));
        }
    }
}
