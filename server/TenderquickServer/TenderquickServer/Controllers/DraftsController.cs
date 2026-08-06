using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenderquickServer.Models;
using TenderquickServer.Models.Documents;
using TenderquickServer.Services;

namespace TenderquickServer.Controllers
{
    [ApiController]
    [Route("api/drafts")]
    [Authorize]
    public class DraftsController : ControllerBase
    {
        private const string Editors = $"{Roles.Admin},{Roles.Estimator}";

        private readonly IDocumentService _documents;

        public DraftsController(IDocumentService documents)
        {
            _documents = documents;
        }

        // GET api/drafts?tenderId=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DraftDto>>> GetAll([FromQuery] int? tenderId)
        {
            return Ok(await _documents.GetAllAsync(tenderId));
        }

        // GET api/drafts/memory
        // Declared before "{id}" so the literal segment isn't captured as an id.
        [HttpGet("memory")]
        public async Task<ActionResult<MemoryDto>> GetMemory()
        {
            return Ok(await _documents.GetMemoryAsync());
        }

        // POST api/drafts/memory/learn
        [HttpPost("memory/learn")]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<MemoryDto>> Learn([FromBody] LearnFromEditRequest? req)
        {
            return Ok(await _documents.LearnFromEditAsync(req ?? new LearnFromEditRequest(null, null)));
        }

        // GET api/drafts/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DraftDto>> GetById(int id)
        {
            var draft = await _documents.GetByIdAsync(id);
            return draft is null ? NotFound() : Ok(draft);
        }

        // POST api/drafts
        [HttpPost]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<DraftDto>> Create([FromBody] CreateDraftRequest req)
        {
            var result = await _documents.CreateAsync(req);
            return result.Outcome switch
            {
                DocumentOutcome.Ok => CreatedAtAction(nameof(GetById), new { id = result.Draft!.Id }, result.Draft),
                DocumentOutcome.TenderNotFound => NotFound(new { message = "Tender not found." }),
                _ => BadRequest(),
            };
        }

        // PUT api/drafts/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<DraftDto>> Update(int id, [FromBody] UpdateDraftRequest req)
        {
            var result = await _documents.UpdateAsync(id, req);
            return result.Outcome switch
            {
                DocumentOutcome.Ok => Ok(result.Draft),
                DocumentOutcome.NotFound => NotFound(),
                DocumentOutcome.InvalidStatus => BadRequest(new { message = "Invalid draft status." }),
                _ => BadRequest(),
            };
        }

        // DELETE api/drafts/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Editors)]
        public async Task<IActionResult> Delete(int id)
        {
            return await _documents.DeleteAsync(id) ? NoContent() : NotFound();
        }

        // POST api/drafts/generate/{tenderId}
        // Returns sections without persisting them — the client streams them in, then saves.
        [HttpPost("generate/{tenderId:int}")]
        [Authorize(Roles = Editors)]
        public async Task<ActionResult<GenerateSectionsResponse>> Generate(int tenderId, CancellationToken ct)
        {
            var result = await _documents.GenerateSectionsAsync(tenderId, ct);
            return result is null ? NotFound(new { message = "Tender not found." }) : Ok(result);
        }
    }
}
