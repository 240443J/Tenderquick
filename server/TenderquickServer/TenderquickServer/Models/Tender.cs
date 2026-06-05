using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenderquickServer.Models
{
    public class Tender
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Reference { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(160)]
        public string Agency { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Source { get; set; } = TenderSource.Manual;

        [Required, MaxLength(20)]
        public string Status { get; set; } = TenderStatus.Interested;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstValue { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ClosingAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
