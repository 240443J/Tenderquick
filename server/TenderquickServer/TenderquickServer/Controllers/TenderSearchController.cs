using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenderquickServer.Models;
using TenderquickServer.Models.Search;
using TenderquickServer.Services;

namespace TenderquickServer.Controllers
{
    [ApiController]
    [Route("api/tender-search")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Estimator}")]
    public class TenderSearchController : ControllerBase
    {
        private readonly ITenderSearchService _search;

        public TenderSearchController(ITenderSearchService search)
        {
            _search = search;
        }

        // GET api/tender-search?keyword=cleaning&sources=gebiz,sesami,tenderboard&limit=50
        [HttpGet]
        public async Task<ActionResult<TenderSearchResponse>> Search(
            [FromQuery] string? keyword,
            [FromQuery] string? sources,
            [FromQuery] int limit = 50,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return BadRequest(new { message = "A keyword is required." });

            var sourceKeys = string.IsNullOrWhiteSpace(sources)
                ? null
                : sources.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            limit = Math.Clamp(limit, 1, 100);

            var result = await _search.SearchAsync(keyword.Trim(), sourceKeys, limit, ct);
            return Ok(result);
        }

        // POST api/tender-search/import
        [HttpPost("import")]
        public async Task<ActionResult<ImportResultsResponse>> Import([FromBody] ImportResultsRequest req)
        {
            if (req?.Items is null || req.Items.Count == 0)
                return BadRequest(new { message = "No results to import." });

            var result = await _search.ImportAsync(req.Items);
            return Ok(result);
        }
    }
}
