using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.Models
{
    public class FeatureUsageHistory
    {
        public long Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int FeatureId { get; set; }
        public virtual Feature Feature { get; set; } = null!;

        public int PackageId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequestId { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty; // "Success" | "Failed" | "Denied"

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        public string? Metadata { get; set; } // JSON text
    }
}
