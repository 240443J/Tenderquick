using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenderquickServer.Models;
using TenderquickServer.Models.Discovery;
using TenderquickServer.Services;

namespace TenderquickServer.Controllers
{
    [ApiController]
    [Route("api/scraper")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
    public class ScraperController : ControllerBase
    {
        private readonly IDiscoveryService _discovery;
        private readonly CurrentUser _user;

        public ScraperController(IDiscoveryService discovery, CurrentUser user)
        {
            _discovery = discovery;
            _user = user;
        }

        // GET api/scraper/sources
        [HttpGet("sources")]
        public ActionResult<IEnumerable<ScrapeSourceDto>> GetSources()
        {
            return Ok(_discovery.GetSources());
        }

        // POST api/scraper/scan
        [HttpPost("scan")]
        public async Task<ActionResult<IEnumerable<ScrapeResultDto>>> Scan(
            [FromBody] ScanRequest req, CancellationToken ct)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.Keyword))
                return BadRequest(new { message = "A keyword is required." });

            return Ok(await _discovery.ScanAsync(req, ct));
        }

        // POST api/scraper/import/{id}
        [HttpPost("import/{id:int}")]
        public async Task<ActionResult<ImportDiscoveredResponse>> Import(int id)
        {
            var result = await _discovery.ImportAsync(id);
            return result.Ok ? Ok(result) : BadRequest(result);
        }

        // GET api/scraper/watchlist
        [HttpGet("watchlist")]
        public async Task<ActionResult<IEnumerable<KeywordWatchDto>>> GetWatchlist()
        {
            var userId = _user.Id;
            if (userId is null) return Unauthorized();

            return Ok(await _discovery.GetWatchesAsync(userId.Value));
        }

        // POST api/scraper/watchlist
        [HttpPost("watchlist")]
        public async Task<ActionResult<KeywordWatchDto>> CreateWatch([FromBody] CreateKeywordWatchRequest req)
        {
            var userId = _user.Id;
            if (userId is null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(req.Keywords))
                return BadRequest(new { message = "Keywords are required." });

            var watch = await _discovery.CreateWatchAsync(userId.Value, req);
            return CreatedAtAction(nameof(GetWatchlist), null, watch);
        }

        // DELETE api/scraper/watchlist/{id}
        [HttpDelete("watchlist/{id:int}")]
        public async Task<IActionResult> DeleteWatch(int id)
        {
            var userId = _user.Id;
            if (userId is null) return Unauthorized();

            return await _discovery.DeleteWatchAsync(userId.Value, id) ? NoContent() : NotFound();
        }
    }
}
