using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.Models
{
    public class Feature
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FeatureKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsEnabled { get; set; } = true;

        public virtual ICollection<PackageFeature> PackageFeatures { get; set; } = new List<PackageFeature>();
    }
}
