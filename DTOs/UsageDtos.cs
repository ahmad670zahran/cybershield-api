using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.DTOs
{
    public class RegisterUsageDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string FeatureKey { get; set; } = string.Empty;

        public int PackageId { get; set; }
        public int FeatureId { get; set; }

        [Required]
        public string Status { get; set; } = "Success"; // "Success" | "Failed" | "Denied"

        public string? Metadata { get; set; } // JSON string
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }

    public class UsageFeatureItemDto
    {
        public string FeatureKey { get; set; } = string.Empty;
        public string FeatureName { get; set; } = string.Empty;
        public int Limit { get; set; }     // -1 = unlimited
        public int Used { get; set; }
        public int Remaining { get; set; } // -1 = unlimited
    }

    public class UsageSummaryDto
    {
        public string Package { get; set; } = string.Empty;
        public List<UsageFeatureItemDto> Features { get; set; } = new();
    }

    public class UsageHistoryItemDto
    {
        public string FeatureKey { get; set; } = string.Empty;
        public string FeatureName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Metadata { get; set; }
        public DateTime UsedAt { get; set; }
    }
}
