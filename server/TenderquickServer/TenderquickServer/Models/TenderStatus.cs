namespace TenderquickServer.Models
{
    public static class TenderStatus
    {
        public const string Interested = "Interested";
        public const string Drafting = "Drafting";
        public const string Submitted = "Submitted";
        public const string Won = "Won";
        public const string Lost = "Lost";

        public static readonly string[] All = { Interested, Drafting, Submitted, Won, Lost };

        public static bool IsValid(string status) => Array.Exists(All, s => s == status);
    }

    public static class TenderSource
    {
        public const string Manual = "Manual";
        public const string EmailIngest = "EmailIngest";
    }
}
