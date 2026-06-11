using TenderquickServer.Models.Search;

namespace TenderquickServer.Services.Sources
{
    // Stub adapter. Tenderboard has no public open-data API — live tender data requires
    // authenticated portal access, so this yields nothing until a real integration is built.
    public class TenderboardSource : ITenderSource
    {
        public string Key => "tenderboard";
        public string Name => "Tenderboard";
        public bool IsImplemented => false;

        public Task<IReadOnlyList<ExternalTenderResult>> SearchAsync(string keyword, int limit, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ExternalTenderResult>>(Array.Empty<ExternalTenderResult>());
    }
}
