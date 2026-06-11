namespace TenderquickServer.Models.Tenders
{
    public record TenderListItem(
        int Id,
        string Reference,
        string Title,
        string Agency,
        string Source,
        string Status,
        decimal? EstValue,
        DateTime? ClosingAt);

    public record CreateTenderRequest(
        string Reference,
        string Title,
        string Agency,
        string? Source,
        decimal? EstValue,
        DateTime? ClosingAt,
        string? Notes);

    public record UpdateTenderRequest(
        string Title,
        string Agency,
        string Status,
        decimal? EstValue,
        DateTime? ClosingAt,
        string? Notes);

    public static class TenderStatus
    {
        public const string Interested = "Interested";
        public const string Drafting = "Drafting";
        public const string Submitted = "Submitted";
        public const string Won = "Won";
        public const string Lost = "Lost";

        public static readonly string[] All = { Interested, Drafting, Submitted, Won, Lost };

        public static bool IsValid(string? status) =>
            status is not null && All.Contains(status);
    }
}
