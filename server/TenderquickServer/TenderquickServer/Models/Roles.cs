namespace TenderquickServer.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Estimator = "Estimator";
        public const string Viewer = "Viewer";

        public static readonly string[] All = { Admin, Estimator, Viewer };

        public static bool IsValid(string role) => Array.Exists(All, r => r == role);
    }
}
