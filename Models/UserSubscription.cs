using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyberShield.API.Models
{
    public enum SubscriptionStatus { Pending, Active, Expired, Cancelled }

    public class UserSubscription
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        public int PackageId { get; set; }
        public virtual Package Package { get; set; } = null!;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
