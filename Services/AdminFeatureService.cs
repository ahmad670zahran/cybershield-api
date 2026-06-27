using CyberShield.API.Data;
using CyberShield.API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CyberShield.API.Services
{
    public class AdminFeatureService : IAdminFeatureService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAdminAuditService _audit;

        public AdminFeatureService(ApplicationDbContext db, IAdminAuditService audit)
        {
            _db = db;
            _audit = audit;
        }

        public async Task<List<AdminFeatureItemDto>> GetFeaturesAsync()
        {
            var now = DateTime.UtcNow;
            var features = await _db.Features.Where(f => f.IsActive).ToListAsync();

            // Usage totals from counter table (fast aggregation)
            var totalUsageMap = await _db.FeatureUsageCounters
                .GroupBy(c => c.FeatureId)
                .Select(g => new { FeatureId = g.Key, Total = g.Sum(c => (long)c.UsageCount) })
                .ToDictionaryAsync(g => g.FeatureId, g => g.Total);

            var monthUsageMap = await _db.FeatureUsageCounters
                .Where(c => c.Year == now.Year && c.Month == now.Month)
                .GroupBy(c => c.FeatureId)
                .Select(g => new { FeatureId = g.Key, Total = g.Sum(c => (long)c.UsageCount) })
                .ToDictionaryAsync(g => g.FeatureId, g => g.Total);

            // Today — must use history (day granularity)
            var todayUsageMap = await _db.FeatureUsageHistories
                .Where(h => h.Status == "Success" && h.UsedAt >= now.Date)
                .GroupBy(h => h.FeatureId)
                .Select(g => new { FeatureId = g.Key, Count = (long)g.Count() })
                .ToDictionaryAsync(g => g.FeatureId, g => g.Count);

            return features.Select(f => new AdminFeatureItemDto
            {
                Id = f.Id,
                FeatureKey = f.FeatureKey,
                Name = f.Name,
                Description = f.Description,
                IsEnabled = f.IsEnabled,
                TotalUsage = totalUsageMap.TryGetValue(f.Id, out var t) ? t : 0,
                TodayUsage = todayUsageMap.TryGetValue(f.Id, out var d) ? d : 0,
                ThisMonthUsage = monthUsageMap.TryGetValue(f.Id, out var m) ? m : 0
            }).ToList();
        }

        public async Task<AdminFeatureDetailDto?> GetFeatureDetailAsync(int featureId)
        {
            var feature = await _db.Features.FindAsync(featureId);
            if (feature is null) return null;

            var now = DateTime.UtcNow;

            var totalUsage = await _db.FeatureUsageCounters
                .Where(c => c.FeatureId == featureId)
                .SumAsync(c => (long)c.UsageCount);

            var todayUsage = await _db.FeatureUsageHistories
                .CountAsync(h => h.FeatureId == featureId && h.Status == "Success" && h.UsedAt >= now.Date);

            var monthUsage = await _db.FeatureUsageCounters
                .Where(c => c.FeatureId == featureId && c.Year == now.Year && c.Month == now.Month)
                .SumAsync(c => (long)c.UsageCount);

            var featureItem = new AdminFeatureItemDto
            {
                Id = feature.Id,
                FeatureKey = feature.FeatureKey,
                Name = feature.Name,
                Description = feature.Description,
                IsEnabled = feature.IsEnabled,
                TotalUsage = totalUsage,
                TodayUsage = todayUsage,
                ThisMonthUsage = monthUsage
            };

            // Packages that include this feature
            var assignedPackages = await _db.PackageFeatures
                .Where(pf => pf.FeatureId == featureId)
                .Include(pf => pf.Package).ThenInclude(p => p.PackageFeatures).ThenInclude(pf2 => pf2.Feature)
                .Select(pf => pf.Package)
                .ToListAsync();

            var recentActivity = await _db.FeatureUsageHistories
                .Include(h => h.Feature)
                .Where(h => h.FeatureId == featureId)
                .OrderByDescending(h => h.UsedAt)
                .Take(20)
                .Select(h => new UsageHistoryItemDto
                {
                    FeatureKey = h.Feature.FeatureKey,
                    FeatureName = h.Feature.Name,
                    Status = h.Status,
                    Metadata = h.Metadata,
                    UsedAt = h.UsedAt
                })
                .ToListAsync();

            return new AdminFeatureDetailDto
            {
                Feature = featureItem,
                AssignedPackages = assignedPackages.Select(PackageService.MapToDto).ToList(),
                RecentActivity = recentActivity
            };
        }

        public async Task DisableFeatureAsync(string adminId, int featureId)
        {
            var feature = await _db.Features.FindAsync(featureId)
                ?? throw new InvalidOperationException("Feature not found.");

            var old = JsonSerializer.Serialize(new { feature.IsEnabled });
            feature.IsEnabled = false;
            await _db.SaveChangesAsync();
            await _audit.LogAsync(adminId, "DISABLE_FEATURE", "Feature", featureId.ToString(), old,
                JsonSerializer.Serialize(new { IsEnabled = false }));
        }

        public async Task EnableFeatureAsync(string adminId, int featureId)
        {
            var feature = await _db.Features.FindAsync(featureId)
                ?? throw new InvalidOperationException("Feature not found.");

            var old = JsonSerializer.Serialize(new { feature.IsEnabled });
            feature.IsEnabled = true;
            await _db.SaveChangesAsync();
            await _audit.LogAsync(adminId, "ENABLE_FEATURE", "Feature", featureId.ToString(), old,
                JsonSerializer.Serialize(new { IsEnabled = true }));
        }
    }
}
