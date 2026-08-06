using TenderquickServer.Models.Ai;

namespace TenderquickServer.Services
{
    // The whole AI surface of the product. Swapping vendors means writing one new class and
    // changing one registration line in Program.cs — no controller or service above this
    // interface knows which provider is in use.
    public interface IAiProvider
    {
        string ModelName { get; }

        Task<AiQuotationResult> SuggestQuotationLinesAsync(
            AiTenderContext tender,
            IReadOnlyList<AiCatalogEntry> catalog,
            CancellationToken ct = default);

        Task<AiSectionResult> GenerateDocumentSectionsAsync(
            AiTenderContext tender,
            IReadOnlyList<AiSectionTemplate> templates,
            IReadOnlyList<string> learnedPreferences,
            CancellationToken ct = default);
    }
}
