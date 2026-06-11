using TenderquickServer.Models.Search;

namespace TenderquickServer.Services
{
    public interface ITenderSearchService
    {
        Task<TenderSearchResponse> SearchAsync(string keyword, IReadOnlyList<string>? sourceKeys, int limit, CancellationToken ct);
        Task<ImportResultsResponse> ImportAsync(IReadOnlyList<ExternalTenderResult> items);
    }
}
