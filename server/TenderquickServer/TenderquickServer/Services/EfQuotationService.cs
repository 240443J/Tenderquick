using Microsoft.EntityFrameworkCore;
using TenderquickServer.Data;
using TenderquickServer.Models;
using TenderquickServer.Models.Ai;
using TenderquickServer.Models.Quotations;

namespace TenderquickServer.Services
{
    public class EfQuotationService : IQuotationService
    {
        private readonly AppDbContext _db;
        private readonly IAiProvider _ai;
        private readonly IAuditService _audit;
        private readonly CurrentUser _user;

        public EfQuotationService(AppDbContext db, IAiProvider ai, IAuditService audit, CurrentUser user)
        {
            _db = db;
            _ai = ai;
            _audit = audit;
            _user = user;
        }

        public async Task<IEnumerable<QuotationDto>> GetAllAsync(int? tenderId)
        {
            var query = _db.Quotations
                .AsNoTracking()
                .Include(q => q.Tender)
                .Include(q => q.Lines)
                .AsQueryable();

            if (tenderId is not null)
                query = query.Where(q => q.TenderId == tenderId);

            var rows = await query.OrderByDescending(q => q.Id).ToListAsync();
            return rows.Select(ToDto).ToList();
        }

        public async Task<QuotationDto?> GetByIdAsync(int id)
        {
            var quote = await _db.Quotations
                .AsNoTracking()
                .Include(q => q.Tender)
                .Include(q => q.Lines)
                .FirstOrDefaultAsync(q => q.Id == id);

            return quote is null ? null : ToDto(quote);
        }

        public async Task<QuotationResult> GenerateFromTenderAsync(int tenderId, CancellationToken ct = default)
        {
            var tender = await _db.Tenders
                .Include(t => t.Specs)
                .FirstOrDefaultAsync(t => t.Id == tenderId, ct);

            if (tender is null)
                return new QuotationResult(QuotationOutcome.TenderNotFound, null);

            var catalog = await BuildCatalogAsync(ct);
            if (catalog.Count == 0)
                return new QuotationResult(QuotationOutcome.EmptyCatalog, null);

            var context = new AiTenderContext(
                tender.Id, tender.Reference, tender.Title, tender.Agency,
                tender.Specs.OrderBy(s => s.Ordinal).Select(s => s.Text).ToList());

            var suggestion = await _ai.SuggestQuotationLinesAsync(context, catalog, ct);

            var now = DateTime.UtcNow;
            var quote = new Quotation
            {
                QuoteNo = await NextQuoteNoAsync(ct),
                TenderId = tender.Id,
                Title = tender.Title,
                Client = tender.Agency,
                Status = QuotationStatus.Draft,
                Version = 1,
                MarkupPct = 15m,
                GstPct = 9m,
                CreatedByUserId = _user.Id,
                CreatedAt = now,
                UpdatedAt = now,
            };

            var ordinal = 0;
            foreach (var line in suggestion.Lines)
            {
                quote.Lines.Add(new QuotationLine
                {
                    Ordinal = ordinal++,
                    Kind = line.Kind,
                    Description = line.Description,
                    Qty = line.Qty,
                    Unit = line.Unit,
                    UnitPrice = line.UnitPrice,
                    InventoryItemId = line.InventoryItemId,
                    LabourRateId = line.LabourRateId,
                    // Flagged so the estimator can see exactly what a machine proposed.
                    IsAiSuggested = true,
                });
            }

            Recalculate(quote);

            _db.Quotations.Add(quote);
            await _db.SaveChangesAsync(ct);

            _db.AiInteractions.Add(new AiInteraction
            {
                Feature = AiFeature.QuotationDraft,
                Model = _ai.ModelName,
                TenderId = tender.Id,
                EntityType = "Quotation",
                EntityId = quote.Id,
                Prompt = $"{tender.Reference} — {tender.Title}",
                Response = string.Join("; ", quote.Lines.Select(l => $"{l.Description} x{l.Qty} @ {l.UnitPrice}")),
                TokensIn = suggestion.TokensIn,
                TokensOut = suggestion.TokensOut,
                UserId = _user.Id,
                CreatedAt = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync(ct);

            await _audit.LogAsync("Quotation.Drafted", "Quotation", quote.Id,
                new { quote.QuoteNo, tender.Reference, Lines = quote.Lines.Count });

            quote.Tender = tender;
            return new QuotationResult(QuotationOutcome.Ok, ToDto(quote));
        }

        public async Task<QuotationResult> UpdateAsync(int id, UpdateQuotationRequest req)
        {
            var quote = await _db.Quotations
                .Include(q => q.Tender)
                .Include(q => q.Lines)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote is null)
                return new QuotationResult(QuotationOutcome.NotFound, null);

            if (!string.IsNullOrWhiteSpace(req.Title)) quote.Title = req.Title.Trim();
            if (req.MarkupPct is not null) quote.MarkupPct = req.MarkupPct.Value;
            if (req.GstPct is not null) quote.GstPct = req.GstPct.Value;

            if (req.LineItems is not null)
            {
                _db.QuotationLines.RemoveRange(quote.Lines);
                quote.Lines.Clear();

                var ordinal = 0;
                foreach (var input in req.LineItems)
                {
                    if (string.IsNullOrWhiteSpace(input.Desc)) continue;

                    quote.Lines.Add(new QuotationLine
                    {
                        Ordinal = ordinal++,
                        Kind = QuotationLineKind.IsValid(input.Kind) ? input.Kind! : QuotationLineKind.Equipment,
                        Description = input.Desc.Trim(),
                        Qty = input.Qty,
                        Unit = string.IsNullOrWhiteSpace(input.Unit) ? "each" : input.Unit.Trim(),
                        UnitPrice = input.UnitPrice,
                        IsAiSuggested = input.IsAiSuggested ?? false,
                    });
                }
            }

            // Editing signed-off work invalidates the sign-off: the new numbers have not been
            // checked by a human, so the quote goes back to Draft on a fresh version.
            if (quote.Verified)
            {
                quote.Version += 1;
                quote.Verified = false;
                quote.VerifiedBy = null;
                quote.VerifiedAt = null;
                quote.Status = QuotationStatus.Draft;

                await _audit.LogAsync("Quotation.SignoffInvalidated", "Quotation", quote.Id,
                    new { quote.QuoteNo, quote.Version });
            }

            Recalculate(quote);
            quote.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Quotation.Updated", "Quotation", quote.Id,
                new { quote.QuoteNo, quote.Total });

            return new QuotationResult(QuotationOutcome.Ok, ToDto(quote));
        }

        public async Task<QuotationResult> VerifyAsync(int id)
        {
            var quote = await _db.Quotations
                .Include(q => q.Tender)
                .Include(q => q.Lines)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote is null)
                return new QuotationResult(QuotationOutcome.NotFound, null);

            if (quote.Verified)
                return new QuotationResult(QuotationOutcome.AlreadyVerified, ToDto(quote));

            var now = DateTime.UtcNow;
            var actor = _user.Name;

            quote.Verified = true;
            quote.VerifiedBy = actor;
            quote.VerifiedAt = now;
            quote.Status = QuotationStatus.Verified;
            quote.UpdatedAt = now;

            quote.Signoffs.Add(new QuotationSignoff
            {
                UserId = _user.Id,
                UserName = actor,
                QuoteVersion = quote.Version,
                SignedAt = now,
            });

            await _db.SaveChangesAsync();

            await _audit.LogAsync("Quotation.Verified", "Quotation", quote.Id,
                new { quote.QuoteNo, quote.Version, VerifiedBy = actor, quote.Total });

            return new QuotationResult(QuotationOutcome.Ok, ToDto(quote));
        }

        public async Task<IEnumerable<SignoffDto>?> GetSignoffsAsync(int id)
        {
            if (!await _db.Quotations.AnyAsync(q => q.Id == id)) return null;

            return await _db.QuotationSignoffs
                .AsNoTracking()
                .Where(s => s.QuotationId == id)
                .OrderByDescending(s => s.SignedAt)
                .Select(s => new SignoffDto(s.Id, s.UserName, s.QuoteVersion, s.SignedAt))
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var quote = await _db.Quotations.FirstOrDefaultAsync(q => q.Id == id);
            if (quote is null) return false;

            _db.Quotations.Remove(quote);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("Quotation.Deleted", "Quotation", id, new { quote.QuoteNo });
            return true;
        }

        private async Task<List<AiCatalogEntry>> BuildCatalogAsync(CancellationToken ct)
        {
            var equipment = await _db.InventoryItems
                .AsNoTracking()
                .Where(i => i.IsActive)
                .Select(i => new AiCatalogEntry(
                    i.Id,
                    QuotationLineKind.Equipment,
                    i.Name,
                    i.Category,
                    i.Unit,
                    i.Prices.OrderByDescending(p => p.EffectiveFrom).ThenByDescending(p => p.Id)
                        .Select(p => p.UnitCost).FirstOrDefault()))
                .ToListAsync(ct);

            var labour = await _db.LabourRates
                .AsNoTracking()
                .Where(l => l.IsActive)
                .Select(l => new AiCatalogEntry(
                    l.Id,
                    QuotationLineKind.Labour,
                    l.Role,
                    "Labour",
                    l.Unit,
                    l.Rates.OrderByDescending(r => r.EffectiveFrom).ThenByDescending(r => r.Id)
                        .Select(r => r.HourlyRate).FirstOrDefault()))
                .ToListAsync(ct);

            equipment.AddRange(labour);
            return equipment;
        }

        private async Task<string> NextQuoteNoAsync(CancellationToken ct)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"TQ-Q{year}-";

            var used = await _db.Quotations
                .Where(q => q.QuoteNo.StartsWith(prefix))
                .Select(q => q.QuoteNo)
                .ToListAsync(ct);

            var next = used
                .Select(no => int.TryParse(no[prefix.Length..], out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max() + 1;

            return $"{prefix}{next:000}";
        }

        private static void Recalculate(Quotation quote)
        {
            var subtotal = quote.Lines.Sum(l => l.Qty * l.UnitPrice);
            var preGst = subtotal + (subtotal * quote.MarkupPct / 100m);
            var total = preGst + (preGst * quote.GstPct / 100m);

            quote.Subtotal = decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero);
            quote.Total = decimal.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        private static QuotationDto ToDto(Quotation q) => new(
            q.Id,
            q.QuoteNo,
            q.TenderId,
            q.Tender?.Reference ?? string.Empty,
            q.Title,
            q.Client,
            q.Status,
            q.Version,
            q.Verified,
            q.VerifiedBy,
            q.VerifiedAt,
            q.MarkupPct,
            q.GstPct,
            q.Subtotal,
            q.Total,
            q.CreatedAt,
            q.UpdatedAt,
            q.Lines
                .OrderBy(l => l.Ordinal)
                .Select(l => new QuotationLineDto(
                    l.Id, l.Kind, l.Description, l.Qty, l.Unit, l.UnitPrice, l.IsAiSuggested))
                .ToList());
    }
}
