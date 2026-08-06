namespace TenderquickServer.Models.Quotations
{
    public record QuotationLineDto(
        int Id,
        string Kind,
        string Desc,
        decimal Qty,
        string Unit,
        decimal UnitPrice,
        bool IsAiSuggested);

    public record QuotationDto(
        int Id,
        string QuoteNo,
        int TenderId,
        string TenderRef,
        string Title,
        string Client,
        string Status,
        int Version,
        bool Verified,
        string? VerifiedBy,
        DateTime? VerifiedAt,
        decimal MarkupPct,
        decimal GstPct,
        decimal Subtotal,
        decimal Total,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        IReadOnlyList<QuotationLineDto> LineItems);

    public record QuotationLineInput(
        int? Id,
        string? Kind,
        string? Desc,
        decimal Qty,
        string? Unit,
        decimal UnitPrice,
        bool? IsAiSuggested);

    public record UpdateQuotationRequest(
        string? Title,
        decimal? MarkupPct,
        decimal? GstPct,
        IReadOnlyList<QuotationLineInput>? LineItems);

    public record SignoffDto(int Id, string UserName, int QuoteVersion, DateTime SignedAt);
}
