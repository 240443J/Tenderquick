using TenderquickServer.Models.Search;

namespace TenderquickServer.Services.Sources
{
    // One adapter per external tender portal. Registered as IEnumerable<ITenderSource> so the
    // search service can fan out across all of them without knowing which are live vs stubbed.
    public interface ITenderSource
    {
        // Stable key used in the ?sources= query param (e.g. "gebiz").
        string Key { get; }
        string Name { get; }
        // False for stub adapters so the UI can say "not yet supported" instead of "no matches".
        bool IsImplemented { get; }
        Task<IReadOnlyList<ExternalTenderResult>> SearchAsync(string keyword, int limit, CancellationToken ct);
    }
}
