namespace TenderquickServer.Models.Search
{
    public record ExternalTenderResult(
        string Source,
        string Reference,
        string Title,
        string Agency,
        string? Status,
        decimal? Value,
        DateTime? Date,
        string? DetailUrl);

    public record SourceStatus(string Source, int Count, bool Ok, string? Message = null);

    public record TenderSearchResponse(
        string Keyword,
        IReadOnlyList<ExternalTenderResult> Results,
        IReadOnlyList<SourceStatus> Sources);

    public record ImportResultsRequest(IReadOnlyList<ExternalTenderResult> Items);

    public record ImportResultsResponse(int Imported, int Skipped);
}
