using CyberShield.API.Data;
using CyberShield.API.DTOs;
using CyberShield.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.API.Services
{
    public class AdminAnalyticsService : IAdminAnalyticsService
    {
        private readonly ApplicationDbContext _db;

        public AdminAnalyticsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<TopFeatureDto>> GetTopFeaturesAsync(int topN = 10)
        {
            return await _db.FeatureUsageCounters
                .GroupBy(c => new { c.FeatureId })
                .Select(g => new
                {
                    g.Key.FeatureId,
                    Total = g.Sum(c => (long)c.UsageCount)
                })
                .OrderByDescending(x => x.Total)
                .Take(topN)
                .Join(_db.Features,
                    x => x.FeatureId,
                    f => f.Id,
                    (x, f) => new TopFeatureDto
                    {
                        FeatureKey = f.FeatureKey,
                        FeatureName = f.Name,
                        Usage = x.Total
                    })
                .ToListAsync();
        }

        public async Task<List<FeatureDailyDto>> GetFeatureUsageByDateAsync(
            string featureKey, DateTime from, DateTime to)
        {
            var feature = await _db.Features
                .FirstOrDefaultAsync(f => f.FeatureKey == featureKey);

            if (feature is null) return new List<FeatureDailyDto>();

            var toEnd = to.Date.AddDays(1); // include full last day

            var raw = await _db.FeatureUsageHistories
                .Where(h =>
                    h.FeatureId == feature.Id &&
                    h.Status == "Success" &&
                    h.UsedAt >= from.Date &&
                    h.UsedAt < toEnd)
                .GroupBy(h => new { h.UsedAt.Year, h.UsedAt.Month, h.UsedAt.Day })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Day,
                    Count = (long)g.Count()
                })
                .ToListAsync();

            return raw
                .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
                .Select(x => new FeatureDailyDto
                {
                    Date = $"{x.Year:D4}-{x.Month:D2}-{x.Day:D2}",
                    Usage = x.Count
                })
                .ToList();
        }

        public async Task<List<PackageUsageDto>> GetUsageByPackageAsync()
        {
            var now = DateTime.UtcNow;

            // Get all active subscriptions
            var activeSubs = await _db.UserSubscriptions
                .Where(s => s.Status == SubscriptionStatus.Active && s.EndDate > now)
                .Select(s => new { s.UserId, s.PackageId, s.Package.Name })
                .ToListAsync();

            var userPackageMap = activeSubs.ToDictionary(s => s.UserId, s => s.Name);
            var userIds = userPackageMap.Keys.ToList();

            var counters = await _db.FeatureUsageCounters
                .Where(c => userIds.Contains(c.UserId))
                .Select(c => new { c.UserId, c.UsageCount })
                .ToListAsync();

            return counters
                .GroupBy(c => userPackageMap.TryGetValue(c.UserId, out var p) ? p : "Unknown")
                .Select(g => new PackageUsageDto
                {
                    PackageName = g.Key,
                    Usage = g.Sum(c => (long)c.UsageCount)
                })
                .OrderByDescending(x => x.Usage)
                .ToList();
        }

        public async Task<List<SubDistributionDto>> GetSubscriptionDistributionAsync()
        {
            var now = DateTime.UtcNow;

            return await _db.UserSubscriptions
                .Where(s => s.Status == SubscriptionStatus.Active && s.EndDate > now)
                .GroupBy(s => s.Package.Name)
                .Select(g => new SubDistributionDto
                {
                    PackageName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }
    }
}
