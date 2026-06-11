using TenderquickServer.Models.Search;

namespace TenderquickServer.Services.Sources
{
    // Stub adapter. Sesami has no public open-data API — live tender data sits behind authenticated
    // portal access, so this yields nothing until a real integration is built.
    public class SesamiSource : ITenderSource
    {
        public string Key => "sesami";
        public string Name => "Sesami";
        public bool IsImplemented => false;

        public Task<IReadOnlyList<ExternalTenderResult>> SearchAsync(string keyword, int limit, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ExternalTenderResult>>(Array.Empty<ExternalTenderResult>());
    }
}
