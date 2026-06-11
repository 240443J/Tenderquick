using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using TenderquickServer.Models.Search;

namespace TenderquickServer.Services.Sources
{
    // Real adapter: queries Tenderboard's anonymous open-deals API (the JSON endpoint behind
    // their public /singaporetenders search page). The `key` is the portal's own filter string:
    // type~operator~keywords~buyers~industries~published~closing~sort, keywords pipe-separated.
    public class TenderboardSource : ITenderSource
    {
        public string Key => "tenderboard";
        public string Name => "Tenderboard";
        public bool IsImplemented => true;

        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public TenderboardSource(HttpClient http, IConfiguration config)
        {
            _http = http;
            _baseUrl = (config["TenderSources:Tenderboard:BaseUrl"] ?? "https://www.tenderboard.biz").TrimEnd('/');
            if (!_http.DefaultRequestHeaders.UserAgent.Any())
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (TenderquickBot)");
        }

        public async Task<IReadOnlyList<ExternalTenderResult>> SearchAsync(string keyword, int limit, CancellationToken ct)
        {
            var keywords = string.Join("|", keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            var body = JsonSerializer.Serialize(new { page = 0, key = $"open~or~{keywords}~~~~~-open" });

            var resp = await _http.PostAsync(
                $"{_baseUrl}/api/v1.0/deals/fetchEntities",
                new StringContent(body, Encoding.UTF8, "application/json"), ct);
            resp.EnsureSuccessStatusCode();

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            var payload = await JsonSerializer.DeserializeAsync<FetchResponse>(stream, JsonOpts, ct);

            var deals = payload?.Data?.Entities?.Deals ?? new List<Deal>();
            return deals.Take(limit).Select(Map).ToList();
        }

        private ExternalTenderResult Map(Deal d) => new(
            Source: Name,
            Reference: ReferenceFrom(d),
            Title: Regex.Replace(d.Description ?? string.Empty, @"\s+", " ").Trim(),
            Agency: d.Buyer ?? string.Empty,
            Status: "Open",
            Value: null,
            Date: FromUnix(d.Close),
            DetailUrl: string.IsNullOrEmpty(d.Path) ? null : _baseUrl + d.Path);

        private static string ReferenceFrom(Deal d)
        {
            var tail = d.Path?.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            return !string.IsNullOrWhiteSpace(tail) ? tail.ToUpperInvariant() : $"TB-{d.Id}";
        }

        private static DateTime? FromUnix(long? seconds) =>
            seconds is > 0 ? DateTimeOffset.FromUnixTimeSeconds(seconds.Value).UtcDateTime : null;

        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        private class FetchResponse { public DataNode? Data { get; set; } }
        private class DataNode { public EntitiesNode? Entities { get; set; } }
        private class EntitiesNode { public List<Deal>? Deals { get; set; } }

        private class Deal
        {
            [JsonPropertyName("id")] public string? Id { get; set; }
            [JsonPropertyName("description")] public string? Description { get; set; }
            [JsonPropertyName("buyer")] public string? Buyer { get; set; }
            [JsonPropertyName("close")] public long? Close { get; set; }
            [JsonPropertyName("path")] public string? Path { get; set; }
        }
    }
}
