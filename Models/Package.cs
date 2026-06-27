using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyberShield.API.Models
{
    public enum BillingCycle { Monthly, Yearly }

    public class Package
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "EGP";

        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

        public bool IsPopular { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PackageFeature> PackageFeatures { get; set; } = new List<PackageFeature>();
        public virtual ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
    }
}
