using CyberShield.API.Models;
using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.DTOs
{
    public class CreatePackageDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CurrentPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OriginalPrice { get; set; }

        public string Currency { get; set; } = "EGP";
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
        public bool IsPopular { get; set; } = false;
    }

    public class UpdatePackageDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? CurrentPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? OriginalPrice { get; set; }

        public string? Currency { get; set; }
        public BillingCycle? BillingCycle { get; set; }
        public bool? IsPopular { get; set; }
        public bool? IsActive { get; set; }
    }

    public class PackageFeatureResponseDto
    {
        public int Id { get; set; }
        public int FeatureId { get; set; }
        public string FeatureKey { get; set; } = string.Empty;
        public string FeatureName { get; set; } = string.Empty;
        public int LimitValue { get; set; }
        public string LimitDisplay { get; set; } = string.Empty; // "Unlimited" or "100"
        public int DisplayOrder { get; set; }
    }

    public class PackageResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string BillingCycle { get; set; } = string.Empty;
        public bool IsPopular { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PackageFeatureResponseDto> Features { get; set; } = new();
    }
}
