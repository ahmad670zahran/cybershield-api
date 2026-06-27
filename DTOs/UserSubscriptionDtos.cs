using CyberShield.API.Models;
using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.DTOs
{
    public class SubscribeDto
    {
        [Required]
        public int PackageId { get; set; }
    }

    public class UpgradeSubscriptionDto
    {
        [Required]
        public int NewPackageId { get; set; }
    }

    public class SubscriptionResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public decimal PackagePrice { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PackageFeatureResponseDto> Features { get; set; } = new();
    }
}
