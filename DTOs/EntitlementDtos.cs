using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.DTOs
{
    public class EntitlementCheckRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string FeatureKey { get; set; } = string.Empty;
    }

    public class EntitlementResult
    {
        public bool Allowed { get; set; }
        public string? Reason { get; set; }
        public string? Package { get; set; }
        public int Limit { get; set; }    // -1 = unlimited
        public int Used { get; set; }
        public int Remaining { get; set; } // -1 = unlimited
        public int? PackageId { get; set; }
        public int? FeatureId { get; set; }
    }
}
