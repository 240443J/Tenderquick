using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using TenderquickServer.Models.Search;

namespace TenderquickServer.Services.Sources
{
    // Real adapter: scrapes Sesami's public Business Opportunities page (server-rendered JSP
    // table, no auth required). The page has no server-side keyword search, so the full live
    // list is fetched and filtered here.
    public class SesamiSource : ITenderSource
    {
        public string Key => "sesami";
        public string Name => "Sesami";
        public bool IsImplemented => true;

        private readonly HttpClient _http;
        private readonly string _url;

        public SesamiSource(HttpClient http, IConfiguration config)
        {
            _http = http;
            _url = config["TenderSources:Sesami:Url"]
                   ?? "https://sesami.online/bizopps/businessOpportunities.jsp";
            if (!_http.DefaultRequestHeaders.UserAgent.Any())
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (TenderquickBot)");
        }

        // Each data row is 8 <td> cells: Buyer, Reference, Doc Type, Description,
        // Starting Date, Closing Date, Submission, Action.
        private static readonly Regex RowRegex = new(
            @"<tr>\s*(?:<td[^>]*>(?<c>.*?)</td>\s*){8}</tr>",
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public async Task<IReadOnlyList<ExternalTenderResult>> SearchAsync(string keyword, int limit, CancellationToken ct)
        {
            var html = await _http.GetStringAsync(_url, ct);

            var terms = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var results = new List<ExternalTenderResult>();

            foreach (Match m in RowRegex.Matches(html))
            {
                var cells = m.Groups["c"].Captures
                    .Select(c => WebUtility.HtmlDecode(Regex.Replace(c.Value, "<[^>]+>", " ")).Trim())
                    .ToArray();
                if (cells.Length < 6) continue;

                var (buyer, reference, docType, description, closing) =
                    (cells[0], cells[1], cells[2], cells[3], cells[5]);

                var haystack = $"{buyer} {reference} {description}";
                if (!terms.All(t => haystack.Contains(t, StringComparison.OrdinalIgnoreCase))) continue;

                results.Add(new ExternalTenderResult(
                    Source: Name,
                    Reference: reference,
                    Title: description,
                    Agency: buyer,
                    Status: docType,
                    Value: null,
                    Date: ParseDate(closing),
                    DetailUrl: _url));

                if (results.Count >= limit) break;
            }
            return results;
        }

        private static DateTime? ParseDate(string s) =>
            DateTime.TryParseExact(s, "d MMM yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d
                : (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2) ? d2 : null);
    }
}
