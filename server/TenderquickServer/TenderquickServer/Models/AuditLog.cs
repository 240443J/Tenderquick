namespace TenderquickServer.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; } = "System";
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public DateTime At { get; set; } = DateTime.UtcNow;
        public string? MetaJson { get; set; }
    }
}
