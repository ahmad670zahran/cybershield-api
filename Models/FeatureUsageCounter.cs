using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.Models
{
    public class FeatureUsageCounter
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int FeatureId { get; set; }
        public virtual Feature Feature { get; set; } = null!;

        public int Year { get; set; }
        public int Month { get; set; }
        public int UsageCount { get; set; } = 0;
    }
}
