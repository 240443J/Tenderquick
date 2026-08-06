namespace TenderquickServer.Models.Ai
{
    public record AiTenderContext(
        int Id,
        string Reference,
        string Title,
        string Agency,
        IReadOnlyList<string> Specs);

    // A priceable thing the provider is allowed to match against. Kind is
    // QuotationLineKind.Equipment or .Labour; ItemId points back at the source row.
    public record AiCatalogEntry(
        int ItemId,
        string Kind,
        string Name,
        string Category,
        string Unit,
        decimal UnitPrice);

    public record AiSuggestedLine(
        string Kind,
        string Description,
        decimal Qty,
        string Unit,
        decimal UnitPrice,
        int? InventoryItemId,
        int? LabourRateId);

    public record AiSectionTemplate(string Heading, string BodyTemplate);

    public record AiSection(string Heading, string Body);

    public record AiSectionResult(IReadOnlyList<AiSection> Sections, int TokensIn, int TokensOut);

    public record AiQuotationResult(IReadOnlyList<AiSuggestedLine> Lines, int TokensIn, int TokensOut);
}
