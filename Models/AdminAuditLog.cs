using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.Models
{
    public class AdminAuditLog
    {
        public long Id { get; set; }

        [Required]
        public string AdminId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // e.g. "DISABLE_USER", "ENABLE_FEATURE"

        [Required]
        [MaxLength(50)]
        public string TargetType { get; set; } = string.Empty; // "User" | "Feature" | "Package"

        [Required]
        public string TargetId { get; set; } = string.Empty;

        public string? OldValue { get; set; } // JSON snapshot
        public string? NewValue { get; set; } // JSON snapshot

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
