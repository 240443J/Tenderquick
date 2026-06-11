using System.Globalization;
using System.Text.Json;
using TenderquickServer.Models.Search;

namespace TenderquickServer.Services.Sources
{
    // Real adapter: queries Singapore's published "Government Procurement via GeBIZ" open dataset
    // on data.gov.sg (CKAN datastore_search). The `q` param does server-side keyword search.
    public class GebizSource : ITenderSource
    {
        public string Key => "gebiz";
        public string Name => "GeBIZ";
        public bool IsImplemented => true;

        private readonly HttpClient _http;
        private readonly ILogger<GebizSource> _logger;
        private readonly string _baseUrl;
        private readonly string _resourceId;

        public GebizSource(HttpClient http, IConfiguration config, ILogger<GebizSource> logger)
        {
            _http = http;
            _logger = logger;
            _baseUrl = config["TenderSources:Gebiz:BaseUrl"] ?? "https://data.gov.sg";
            _resourceId = config["TenderSources:Gebiz:ResourceId"] ?? "d_acde1106003906a75c3fa052592f2fcb";
        }

        public async Task<IReadOnlyList<ExternalTenderResult>> SearchAsync(string keyword, int limit, CancellationToken ct)
        {
            var url = $"{_baseUrl}/api/action/datastore_search" +
                      $"?resource_id={Uri.EscapeDataString(_resourceId)}" +
                      $"&q={Uri.EscapeDataString(keyword)}" +
                      $"&limit={limit}";

            var resp = await _http.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            var payload = await JsonSerializer.DeserializeAsync<DatastoreResponse>(stream, JsonOpts, ct);

            var records = payload?.Result?.Records ?? new List<GebizRecord>();
            return records.Select(Map).ToList();
        }

        private ExternalTenderResult Map(GebizRecord r) => new(
            Source: Name,
            Reference: r.TenderNo ?? string.Empty,
            Title: r.TenderDescription ?? string.Empty,
            Agency: r.Agency ?? string.Empty,
            Status: r.TenderDetailStatus,
            Value: r.AwardedAmt,
            Date: ParseDate(r.AwardDate),
            DetailUrl: $"{_baseUrl}/datasets/{_resourceId}/view");

        private static readonly string[] DateFormats = { "M/d/yyyy", "d/M/yyyy", "yyyy-MM-dd" };

        private static DateTime? ParseDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d
                : (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d2) ? d2 : null);
        }

        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        private class DatastoreResponse
        {
            public bool Success { get; set; }
            public DatastoreResult? Result { get; set; }
        }

        private class DatastoreResult
        {
            public List<GebizRecord>? Records { get; set; }
        }

        private class GebizRecord
        {
            [System.Text.Json.Serialization.JsonPropertyName("tender_no")] public string? TenderNo { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("tender_description")] public string? TenderDescription { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("agency")] public string? Agency { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("award_date")] public string? AwardDate { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("tender_detail_status")] public string? TenderDetailStatus { get; set; }
            [System.Text.Json.Serialization.JsonPropertyName("supplier_name")] public string? SupplierName { get; set; }
            // Dataset schema types this as numeric but the API returns strings; tolerate both
            // (and empty/garbage values) so one odd record can't fail the whole response.
            [System.Text.Json.Serialization.JsonPropertyName("awarded_amt")]
            [System.Text.Json.Serialization.JsonConverter(typeof(LenientDecimalConverter))]
            public decimal? AwardedAmt { get; set; }
        }

        private class LenientDecimalConverter : System.Text.Json.Serialization.JsonConverter<decimal?>
        {
            public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Number && reader.TryGetDecimal(out var n)) return n;
                if (reader.TokenType == JsonTokenType.String &&
                    decimal.TryParse(reader.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var s)) return s;
                return null;
            }

            public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
            {
                if (value.HasValue) writer.WriteNumberValue(value.Value);
                else writer.WriteNullValue();
            }
        }
    }
}
