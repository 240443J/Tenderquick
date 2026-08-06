using TenderquickServer.Models.Discovery;

namespace TenderquickServer.Services
{
    public interface IDiscoveryService
    {
        IEnumerable<ScrapeSourceDto> GetSources();
        Task<IEnumerable<ScrapeResultDto>> ScanAsync(ScanRequest req, CancellationToken ct = default);
        Task<ImportDiscoveredResponse> ImportAsync(int discoveredId);

        Task<IEnumerable<KeywordWatchDto>> GetWatchesAsync(int userId);
        Task<KeywordWatchDto> CreateWatchAsync(int userId, CreateKeywordWatchRequest req);
        Task<bool> DeleteWatchAsync(int userId, int id);
    }
}
