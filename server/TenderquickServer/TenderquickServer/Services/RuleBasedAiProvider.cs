using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TenderquickServer.Models;
using TenderquickServer.Models.Ai;

namespace TenderquickServer.Services
{
    // Default provider: deterministic, offline, no API key. It matches specification text
    // against the priced catalog and renders the stored section templates.
    //
    // It exists so the AI-shaped features are fully working end to end (and every call is
    // logged as an AiInteraction) before a hosted model is wired in. Replacing it with an
    // LLM-backed IAiProvider is a one-line change in Program.cs.
    public class RuleBasedAiProvider : IAiProvider
    {
        private static readonly Regex QuantityPattern =
            new(@"(\d[\d,]*)\s*(?:nos?\.?|units?|pcs?|sets?|points?|each|m\b|metres?|meters?)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "the", "and", "for", "with", "from", "each", "unit", "units", "type", "new",
            "supply", "install", "installation", "works", "work", "system", "systems",
            "general", "item", "items", "box", "lot", "per", "hour", "meter", "metre",
        };

        private readonly IConfiguration _config;

        public RuleBasedAiProvider(IConfiguration config)
        {
            _config = config;
        }

        public string ModelName => "rules-v1";

        public Task<AiQuotationResult> SuggestQuotationLinesAsync(
            AiTenderContext tender,
            IReadOnlyList<AiCatalogEntry> catalog,
            CancellationToken ct = default)
        {
            var specText = string.Join(" \n ", tender.Specs);
            var haystack = $"{tender.Title} {specText}".ToLowerInvariant();

            var lines = new List<AiSuggestedLine>();

            var equipment = catalog.Where(c => c.Kind == QuotationLineKind.Equipment);
            foreach (var entry in equipment)
            {
                var keywords = Keywords(entry.Name).Concat(Keywords(entry.Category)).Distinct().ToList();
                if (keywords.Count == 0) continue;

                var hits = keywords.Where(k => haystack.Contains(k, StringComparison.OrdinalIgnoreCase)).ToList();
                if (hits.Count == 0) continue;

                var qty = InferQuantity(tender.Specs, hits) ?? 1m;
                lines.Add(new AiSuggestedLine(
                    QuotationLineKind.Equipment, entry.Name, qty, entry.Unit, entry.UnitPrice, entry.ItemId, null));
            }

            // Labour is never inferred from the catalog alone — a job with materials always
            // needs a trade and supervision, so those are added explicitly.
            var labour = catalog.Where(c => c.Kind == QuotationLineKind.Labour).ToList();
            var tradeHours = lines.Count > 0 ? 8m * lines.Count * 4m : 40m;

            var trade = BestLabourMatch(labour, haystack);
            if (trade is not null)
                lines.Add(new AiSuggestedLine(
                    QuotationLineKind.Labour, trade.Name, tradeHours, trade.Unit, trade.UnitPrice, null, trade.ItemId));

            var supervisor = labour.FirstOrDefault(l => l.Name.Contains("Supervisor", StringComparison.OrdinalIgnoreCase));
            if (supervisor is not null && supervisor.ItemId != trade?.ItemId)
                lines.Add(new AiSuggestedLine(
                    QuotationLineKind.Labour, supervisor.Name, Math.Round(tradeHours / 4m, 0),
                    supervisor.Unit, supervisor.UnitPrice, null, supervisor.ItemId));

            var prompt = BuildQuotationPrompt(tender, catalog.Count);
            var response = string.Join("; ", lines.Select(l => $"{l.Description} x{l.Qty}"));

            return Task.FromResult(new AiQuotationResult(lines, Tokens(prompt), Tokens(response)));
        }

        public Task<AiSectionResult> GenerateDocumentSectionsAsync(
            AiTenderContext tender,
            IReadOnlyList<AiSectionTemplate> templates,
            IReadOnlyList<string> learnedPreferences,
            CancellationToken ct = default)
        {
            var company = _config.GetSection("Company");
            var specList = tender.Specs.Count == 0
                ? "   (No specification lines have been captured for this tender yet.)"
                : string.Join("\n", tender.Specs.Select((s, i) => $"   {i + 1}.{i + 1}  {s}"));

            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["agency"] = tender.Agency,
                ["reference"] = tender.Reference,
                ["title"] = tender.Title,
                ["specs"] = specList,
                ["company"] = company["Name"] ?? "our company",
                ["uen"] = company["Uen"] ?? string.Empty,
                ["email"] = company["Email"] ?? string.Empty,
                ["phone"] = company["Phone"] ?? string.Empty,
                ["preferences"] = learnedPreferences.Count == 0
                    ? string.Empty
                    : string.Join("\n", learnedPreferences.Select(p => $"• {p}")),
            };

            var sections = templates
                .Select(t => new AiSection(Render(t.Heading, tokens), Render(t.BodyTemplate, tokens)))
                .ToList();

            var prompt = BuildDraftPrompt(tender, learnedPreferences);
            var response = string.Join("\n\n", sections.Select(s => $"{s.Heading}\n{s.Body}"));

            return Task.FromResult(new AiSectionResult(sections, Tokens(prompt), Tokens(response)));
        }

        private static AiCatalogEntry? BestLabourMatch(IReadOnlyList<AiCatalogEntry> labour, string haystack)
        {
            if (labour.Count == 0) return null;

            AiCatalogEntry? best = null;
            var bestScore = 0;

            foreach (var entry in labour)
            {
                if (entry.Name.Contains("Supervisor", StringComparison.OrdinalIgnoreCase)) continue;

                var score = Keywords(entry.Name)
                    .Count(k => haystack.Contains(k, StringComparison.OrdinalIgnoreCase));

                if (score > bestScore)
                {
                    bestScore = score;
                    best = entry;
                }
            }

            return best ?? labour.FirstOrDefault(l =>
                       !l.Name.Contains("Supervisor", StringComparison.OrdinalIgnoreCase))
                   ?? labour[0];
        }

        private static decimal? InferQuantity(IReadOnlyList<string> specs, IReadOnlyList<string> hits)
        {
            foreach (var spec in specs)
            {
                if (!hits.Any(h => spec.Contains(h, StringComparison.OrdinalIgnoreCase))) continue;

                var match = QuantityPattern.Match(spec);
                if (!match.Success) continue;

                var raw = match.Groups[1].Value.Replace(",", string.Empty);
                if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var qty) && qty > 0)
                    return qty;
            }

            return null;
        }

        private static IEnumerable<string> Keywords(string source) =>
            Regex.Split(source ?? string.Empty, @"[^A-Za-z0-9]+")
                .Where(w => w.Length >= 3 && !StopWords.Contains(w))
                .Select(w => w.ToLowerInvariant())
                .Distinct();

        private static string Render(string template, IDictionary<string, string> tokens) =>
            Regex.Replace(template ?? string.Empty, @"\{\{(\w+)\}\}", m =>
                tokens.TryGetValue(m.Groups[1].Value, out var value) ? value : m.Value);

        private static string BuildQuotationPrompt(AiTenderContext tender, int catalogSize)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Price tender {tender.Reference} — {tender.Title} ({tender.Agency}).");
            sb.AppendLine($"Catalog entries available: {catalogSize}.");
            foreach (var spec in tender.Specs) sb.AppendLine($"- {spec}");
            return sb.ToString();
        }

        private static string BuildDraftPrompt(AiTenderContext tender, IReadOnlyList<string> preferences)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Draft a tender response for {tender.Reference} — {tender.Title} ({tender.Agency}).");
            foreach (var spec in tender.Specs) sb.AppendLine($"- {spec}");
            foreach (var pref in preferences) sb.AppendLine($"house style: {pref}");
            return sb.ToString();
        }

        // Rough token estimate so cost reporting has a consistent unit across providers.
        private static int Tokens(string? text) =>
            string.IsNullOrEmpty(text) ? 0 : (int)Math.Ceiling(text.Length / 4.0);
    }
}
