using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenderquickServer.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        [MaxLength(120)]
        public string UserName { get; set; } = "system";

        [Required, MaxLength(80)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(60)]
        public string? EntityType { get; set; }

        public int? EntityId { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime At { get; set; } = DateTime.UtcNow;

        [MaxLength(2000)]
        public string? MetaJson { get; set; }
    }
}
