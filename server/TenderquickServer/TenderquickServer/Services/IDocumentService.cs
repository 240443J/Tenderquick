using TenderquickServer.Models.Documents;

namespace TenderquickServer.Services
{
    public enum DocumentOutcome { Ok, NotFound, TenderNotFound, InvalidStatus }

    public record DocumentResult(DocumentOutcome Outcome, DraftDto? Draft);

    public interface IDocumentService
    {
        Task<IEnumerable<DraftDto>> GetAllAsync(int? tenderId);
        Task<DraftDto?> GetByIdAsync(int id);
        Task<DocumentResult> CreateAsync(CreateDraftRequest req);
        Task<DocumentResult> UpdateAsync(int id, UpdateDraftRequest req);
        Task<bool> DeleteAsync(int id);
        Task<GenerateSectionsResponse?> GenerateSectionsAsync(int tenderId, CancellationToken ct = default);
        Task<MemoryDto> GetMemoryAsync();
        Task<MemoryDto> LearnFromEditAsync(LearnFromEditRequest req);
    }
}
