namespace CyberShield.API.DTOs
{
    public class TopFeatureDto
    {
        public string FeatureKey { get; set; } = string.Empty;
        public string FeatureName { get; set; } = string.Empty;
        public long Usage { get; set; }
    }

    public class FeatureDailyDto
    {
        public string Date { get; set; } = string.Empty; // "yyyy-MM-dd"
        public long Usage { get; set; }
    }

    public class PackageUsageDto
    {
        public string PackageName { get; set; } = string.Empty;
        public long Usage { get; set; }
    }

    public class SubDistributionDto
    {
        public string PackageName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
