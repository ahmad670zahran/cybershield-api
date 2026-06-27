using CyberShield.API.DTOs;

namespace CyberShield.API.Services
{
    public interface IAdminAnalyticsService
    {
        Task<List<TopFeatureDto>> GetTopFeaturesAsync(int topN = 10);
        Task<List<FeatureDailyDto>> GetFeatureUsageByDateAsync(string featureKey, DateTime from, DateTime to);
        Task<List<PackageUsageDto>> GetUsageByPackageAsync();
        Task<List<SubDistributionDto>> GetSubscriptionDistributionAsync();
    }
}
