using TenderquickServer.Models;
using TenderquickServer.Models.Tenders;

namespace TenderquickServer.Services
{
    public enum CreateOutcome { Created, DuplicateReference }
    public enum UpdateOutcome { Updated, NotFound, InvalidStatus }

    public record CreateTenderResult(CreateOutcome Outcome, Tender? Tender);
    public record UpdateTenderResult(UpdateOutcome Outcome, Tender? Tender);

    public interface ITenderService
    {
        Task<IEnumerable<TenderListItem>> GetAllAsync(string? status, string? search);
        Task<Tender?> GetByIdAsync(int id);
        Task<CreateTenderResult> CreateAsync(CreateTenderRequest req);
        Task<UpdateTenderResult> UpdateAsync(int id, UpdateTenderRequest req);
        Task<bool> DeleteAsync(int id);
    }
}
