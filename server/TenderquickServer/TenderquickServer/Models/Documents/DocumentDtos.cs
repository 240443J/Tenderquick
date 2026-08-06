namespace TenderquickServer.Models.Documents
{
    public record DraftSectionDto(string Heading, string Body);

    public record DraftDto(
        int Id,
        int TenderId,
        string TenderRef,
        string Title,
        string Type,
        string Status,
        int Version,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        IReadOnlyList<DraftSectionDto> Sections);

    public record CreateDraftRequest(
        int TenderId,
        string? Title,
        string? Type,
        IReadOnlyList<DraftSectionDto>? Sections);

    public record UpdateDraftRequest(
        string? Title,
        string? Status,
        bool? BumpVersion,
        IReadOnlyList<DraftSectionDto>? Sections);

    public record DraftTenderSummary(int Id, string Reference, string Title, string Agency);

    public record GenerateSectionsResponse(
        IReadOnlyList<DraftSectionDto> Sections,
        DraftTenderSummary Tender);

    public record PreferenceDto(int Id, string Text, decimal Confidence, string Source);

    public record MemoryDto(
        int SamplesLearned,
        DateTime LastUpdated,
        IReadOnlyList<PreferenceDto> Preferences);

    public record LearnFromEditRequest(string? Text, int? DraftId);
}
