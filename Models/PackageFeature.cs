namespace CyberShield.API.Models
{
    public class PackageFeature
    {
        public int Id { get; set; }

        public int PackageId { get; set; }
        public virtual Package Package { get; set; } = null!;

        public int FeatureId { get; set; }
        public virtual Feature Feature { get; set; } = null!;

        // -1 = unlimited, N = N uses per billing cycle
        public int LimitValue { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }
}
