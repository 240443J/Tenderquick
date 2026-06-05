using System.ComponentModel.DataAnnotations;

namespace TenderquickServer.Models.Tenders
{
    public class CreateTenderRequest
    {
        [Required, MinLength(2), MaxLength(80)]
        public string Reference { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MinLength(2), MaxLength(160)]
        public string Agency { get; set; } = string.Empty;

        [Range(0, 1_000_000_000)]
        public decimal? EstValue { get; set; }

        public DateTime? ClosingAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class UpdateTenderRequest
    {
        [Required, MinLength(3), MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MinLength(2), MaxLength(160)]
        public string Agency { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = TenderStatus.Interested;

        [Range(0, 1_000_000_000)]
        public decimal? EstValue { get; set; }

        public DateTime? ClosingAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public record TenderListItem(
        int Id, string Reference, string Title, string Agency,
        string Status, decimal? EstValue, DateTime? ClosingAt);
}
