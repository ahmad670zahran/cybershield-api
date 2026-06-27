using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.DTOs
{
    public class CreatePackageFeatureDto
    {
        [Required]
        public int FeatureId { get; set; }

        public int LimitValue { get; set; } = -1; // -1 = unlimited

        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdatePackageFeatureDto
    {
        public int? LimitValue { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
