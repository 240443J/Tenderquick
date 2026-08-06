namespace TenderquickServer.Models.Deadlines
{
    public record DeadlineDto(
        int Id,
        int TenderId,
        string TenderRef,
        string Title,
        string Type,
        DateTime DueAt,
        bool AddedToCalendar,
        string Priority);

    public record CreateDeadlineRequest(
        int TenderId,
        string Title,
        string Type,
        DateTime DueAt,
        string? Priority);

    public record UpdateDeadlineRequest(
        string? Title,
        string? Type,
        DateTime? DueAt,
        string? Priority);

    public record CalendarStatusDto(bool Connected, string? Account);
}
