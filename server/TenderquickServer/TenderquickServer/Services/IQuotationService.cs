using TenderquickServer.Models.Quotations;

namespace TenderquickServer.Services
{
    public enum QuotationOutcome { Ok, NotFound, TenderNotFound, EmptyCatalog, AlreadyVerified }

    public record QuotationResult(QuotationOutcome Outcome, QuotationDto? Quotation);

    public interface IQuotationService
    {
        Task<IEnumerable<QuotationDto>> GetAllAsync(int? tenderId);
        Task<QuotationDto?> GetByIdAsync(int id);
        Task<QuotationResult> GenerateFromTenderAsync(int tenderId, CancellationToken ct = default);
        Task<QuotationResult> UpdateAsync(int id, UpdateQuotationRequest req);
        Task<QuotationResult> VerifyAsync(int id);
        Task<IEnumerable<SignoffDto>?> GetSignoffsAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
