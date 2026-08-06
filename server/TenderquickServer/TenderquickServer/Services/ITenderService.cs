using TenderquickServer.Models.Tenders;

namespace TenderquickServer.Services
{
    public enum CreateOutcome { Created, DuplicateReference }
    public enum UpdateOutcome { Updated, NotFound, InvalidStatus }
    public enum DeleteOutcome { Deleted, NotFound, HasQuotations }

    public record CreateTenderResult(CreateOutcome Outcome, TenderDetail? Tender);
    public record UpdateTenderResult(UpdateOutcome Outcome, TenderDetail? Tender);

    public interface ITenderService
    {
        Task<IEnumerable<TenderListItem>> GetAllAsync(string? status, string? search);
        Task<TenderDetail?> GetByIdAsync(int id);
        Task<CreateTenderResult> CreateAsync(CreateTenderRequest req);
        Task<UpdateTenderResult> UpdateAsync(int id, UpdateTenderRequest req);
        Task<DeleteOutcome> DeleteAsync(int id);
    }
}
