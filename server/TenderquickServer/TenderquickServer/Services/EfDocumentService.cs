using Microsoft.EntityFrameworkCore;
using TenderquickServer.Data;
using TenderquickServer.Models;
using TenderquickServer.Models.Ai;
using TenderquickServer.Models.Documents;

namespace TenderquickServer.Services
{
    public class EfDocumentService : IDocumentService
    {
        private readonly AppDbContext _db;
        private readonly IAiProvider _ai;
        private readonly IAuditService _audit;
        private readonly CurrentUser _user;

        public EfDocumentService(AppDbContext db, IAiProvider ai, IAuditService audit, CurrentUser user)
        {
            _db = db;
            _ai = ai;
            _audit = audit;
            _user = user;
        }

        public async Task<IEnumerable<DraftDto>> GetAllAsync(int? tenderId)
        {
            var query = _db.TenderDocuments
                .AsNoTracking()
                .Include(d => d.Tender)
                .Include(d => d.Sections)
                .AsQueryable();

            if (tenderId is not null)
                query = query.Where(d => d.TenderId == tenderId);

            var rows = await query.OrderByDescending(d => d.UpdatedAt).ThenByDescending(d => d.Id).ToListAsync();
            return rows.Select(ToDto).ToList();
        }

        public async Task<DraftDto?> GetByIdAsync(int id)
        {
            var doc = await _db.TenderDocuments
                .AsNoTracking()
                .Include(d => d.Tender)
                .Include(d => d.Sections)
                .FirstOrDefaultAsync(d => d.Id == id);

            return doc is null ? null : ToDto(doc);
        }

        public async Task<DocumentResult> CreateAsync(CreateDraftRequest req)
        {
            var tender = await _db.Tenders.FirstOrDefaultAsync(t => t.Id == req.TenderId);
            if (tender is null)
                return new DocumentResult(DocumentOutcome.TenderNotFound, null);

            var now = DateTime.UtcNow;
            var doc = new TenderDocument
            {
                TenderId = tender.Id,
                Title = string.IsNullOrWhiteSpace(req.Title)
                    ? $"Technical Proposal — {tender.Title}"
                    : req.Title.Trim(),
                Type = string.IsNullOrWhiteSpace(req.Type) ? "Technical Proposal" : req.Type.Trim(),
                Status = DocumentStatus.Draft,
                Version = 1,
                CreatedByUserId = _user.Id,
                CreatedAt = now,
                UpdatedAt = now,
            };

            ApplySections(doc, req.Sections, aiGenerated: false);

            _db.TenderDocuments.Add(doc);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Draft.Created", "TenderDocument", doc.Id, new { doc.Title, tender.Reference });

            doc.Tender = tender;
            return new DocumentResult(DocumentOutcome.Ok, ToDto(doc));
        }

        public async Task<DocumentResult> UpdateAsync(int id, UpdateDraftRequest req)
        {
            var doc = await _db.TenderDocuments
                .Include(d => d.Tender)
                .Include(d => d.Sections)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc is null)
                return new DocumentResult(DocumentOutcome.NotFound, null);

            if (req.Status is not null && !DocumentStatus.IsValid(req.Status))
                return new DocumentResult(DocumentOutcome.InvalidStatus, null);

            if (!string.IsNullOrWhiteSpace(req.Title)) doc.Title = req.Title.Trim();
            if (req.Status is not null) doc.Status = req.Status;
            if (req.BumpVersion == true) doc.Version += 1;

            if (req.Sections is not null)
            {
                _db.TenderDocumentSections.RemoveRange(doc.Sections);
                doc.Sections.Clear();
                ApplySections(doc, req.Sections, aiGenerated: false);
            }

            doc.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Draft.Updated", "TenderDocument", doc.Id, new { doc.Title, doc.Status, doc.Version });
            return new DocumentResult(DocumentOutcome.Ok, ToDto(doc));
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var doc = await _db.TenderDocuments.FirstOrDefaultAsync(d => d.Id == id);
            if (doc is null) return false;

            _db.TenderDocuments.Remove(doc);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Draft.Deleted", "TenderDocument", id, new { doc.Title });
            return true;
        }

        public async Task<GenerateSectionsResponse?> GenerateSectionsAsync(int tenderId, CancellationToken ct = default)
        {
            var tender = await _db.Tenders
                .AsNoTracking()
                .Include(t => t.Specs)
                .FirstOrDefaultAsync(t => t.Id == tenderId, ct);

            if (tender is null) return null;

            var templates = await _db.DocumentTemplates
                .AsNoTracking()
                .Where(t => t.IsActive)
                .OrderBy(t => t.Ordinal)
                .Select(t => new AiSectionTemplate(t.Section, t.BodyTemplate))
                .ToListAsync(ct);

            var memory = await GetOrCreateMemoryAsync();
            var preferences = memory.Preferences
                .OrderByDescending(p => p.Confidence)
                .Select(p => p.Text)
                .ToList();

            var context = new AiTenderContext(
                tender.Id, tender.Reference, tender.Title, tender.Agency,
                tender.Specs.OrderBy(s => s.Ordinal).Select(s => s.Text).ToList());

            var result = await _ai.GenerateDocumentSectionsAsync(context, templates, preferences, ct);

            _db.AiInteractions.Add(new AiInteraction
            {
                Feature = AiFeature.DocumentDraft,
                Model = _ai.ModelName,
                TenderId = tender.Id,
                EntityType = "Tender",
                EntityId = tender.Id,
                Prompt = $"{tender.Reference} — {tender.Title}",
                Response = string.Join("\n\n", result.Sections.Select(s => $"{s.Heading}\n{s.Body}")),
                TokensIn = result.TokensIn,
                TokensOut = result.TokensOut,
                UserId = _user.Id,
                CreatedAt = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync(ct);

            await _audit.LogAsync("Draft.Generated", "Tender", tender.Id,
                new { tender.Reference, Sections = result.Sections.Count });

            return new GenerateSectionsResponse(
                result.Sections.Select(s => new DraftSectionDto(s.Heading, s.Body)).ToList(),
                new DraftTenderSummary(tender.Id, tender.Reference, tender.Title, tender.Agency));
        }

        public async Task<MemoryDto> GetMemoryAsync()
        {
            var memory = await GetOrCreateMemoryAsync();
            return ToDto(memory);
        }

        public async Task<MemoryDto> LearnFromEditAsync(LearnFromEditRequest req)
        {
            var memory = await GetOrCreateMemoryAsync();

            memory.SamplesLearned += 1;
            memory.LastUpdatedAt = DateTime.UtcNow;

            var text = req.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (text.Length > 500) text = text[..500];

                var existing = memory.Preferences.FirstOrDefault(p =>
                    string.Equals(p.Text, text, StringComparison.OrdinalIgnoreCase));

                if (existing is null)
                {
                    memory.Preferences.Add(new AiPreference
                    {
                        Text = text,
                        // Starts low: a single edit is weak evidence. Repeats raise it.
                        Confidence = 0.55m,
                        Source = "Learned just now from your edit",
                        TimesApplied = 1,
                        CreatedAt = DateTime.UtcNow,
                    });
                }
                else
                {
                    existing.TimesApplied += 1;
                    existing.Confidence = Math.Min(0.99m, existing.Confidence + 0.05m);
                    existing.Source = $"Reinforced across {existing.TimesApplied} of your edits";
                }
            }

            _db.AiInteractions.Add(new AiInteraction
            {
                Feature = AiFeature.DocumentEdit,
                Model = _ai.ModelName,
                EntityType = "TenderDocument",
                EntityId = req.DraftId,
                HumanEditDelta = text?.Length ?? 0,
                Outcome = "HumanEdited",
                UserId = _user.Id,
                CreatedAt = DateTime.UtcNow,
            });

            await _db.SaveChangesAsync();
            return ToDto(memory);
        }

        private async Task<AiMemory> GetOrCreateMemoryAsync()
        {
            // One shared team memory (UserId null) — the house style belongs to the company,
            // not to whoever happened to edit last.
            var memory = await _db.AiMemories
                .Include(m => m.Preferences)
                .FirstOrDefaultAsync(m => m.UserId == null);

            if (memory is null)
            {
                memory = new AiMemory { UserId = null, SamplesLearned = 0, LastUpdatedAt = DateTime.UtcNow };
                _db.AiMemories.Add(memory);
                await _db.SaveChangesAsync();
            }

            return memory;
        }

        private static void ApplySections(TenderDocument doc, IReadOnlyList<DraftSectionDto>? sections, bool aiGenerated)
        {
            if (sections is null) return;

            var ordinal = 0;
            foreach (var section in sections)
            {
                if (string.IsNullOrWhiteSpace(section.Heading) && string.IsNullOrWhiteSpace(section.Body))
                    continue;

                doc.Sections.Add(new TenderDocumentSection
                {
                    Ordinal = ordinal++,
                    Heading = (section.Heading ?? string.Empty).Trim(),
                    Body = section.Body ?? string.Empty,
                    IsAiGenerated = aiGenerated,
                });
            }
        }

        private static DraftDto ToDto(TenderDocument d) => new(
            d.Id,
            d.TenderId,
            d.Tender?.Reference ?? string.Empty,
            d.Title,
            d.Type,
            d.Status,
            d.Version,
            d.CreatedAt,
            d.UpdatedAt,
            d.Sections
                .OrderBy(s => s.Ordinal)
                .Select(s => new DraftSectionDto(s.Heading, s.Body))
                .ToList());

        private static MemoryDto ToDto(AiMemory m) => new(
            m.SamplesLearned,
            m.LastUpdatedAt,
            m.Preferences
                .OrderByDescending(p => p.Confidence)
                .ThenByDescending(p => p.Id)
                .Select(p => new PreferenceDto(p.Id, p.Text, p.Confidence, p.Source))
                .ToList());
    }
}
