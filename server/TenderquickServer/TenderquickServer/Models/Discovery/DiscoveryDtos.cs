using TenderquickServer.Models.Tenders;

namespace TenderquickServer.Models.Discovery
{
    public record ScrapeSourceDto(string Id, string Label, bool Enabled, string Status, string Note);

    public record ScrapeResultDto(
        int Id,
        string Reference,
        string Title,
        string Agency,
        string Source,
        DateTime? PublishedAt,
        DateTime? ClosingAt,
        string? EstValueRange,
        int Relevance,
        IReadOnlyList<string> Matched,
        string? Summary,
        string? DetailUrl,
        bool Imported);

    public record ScanRequest(string? Keyword, IReadOnlyList<string>? Sources, int? Limit);

    public record ImportDiscoveredResponse(bool Ok, string? Message, TenderDetail? Tender);

    public record KeywordWatchDto(
        int Id,
        string Keywords,
        string Sources,
        bool IsActive,
        DateTime? LastRunAt);

    public record CreateKeywordWatchRequest(string Keywords, string? Sources);
}
