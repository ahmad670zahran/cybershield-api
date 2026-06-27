using CyberShield.API.Data;
using CyberShield.API.DTOs;
using CyberShield.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.API.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _db;

        public AdminDashboardService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = todayStart.AddDays(-(int)todayStart.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // User counts
            var totalUsers = await _db.Users.CountAsync();
            var activeUsers = await _db.Users.CountAsync(u => u.IsActive);

            // Subscription counts — active subscriptions grouped by package name
            var activeSubs = await _db.UserSubscriptions
                .Include(s => s.Package)
                .Where(s => s.Status == SubscriptionStatus.Active && s.EndDate > now)
                .GroupBy(s => s.Package.Name)
                .Select(g => new { PackageName = g.Key, Count = g.Count() })
                .ToListAsync();

            int totalSubs = activeSubs.Sum(g => g.Count);
            int basicSubs = activeSubs.FirstOrDefault(g => g.PackageName == "Basic")?.Count ?? 0;
            int premiumSubs = activeSubs.FirstOrDefault(g => g.PackageName == "Premium")?.Count ?? 0;
            int enterpriseSubs = activeSubs.FirstOrDefault(g => g.PackageName == "Enterprise")?.Count ?? 0;

            // Usage — aggregate from FeatureUsageCounters (fast)
            var totalUsage = await _db.FeatureUsageCounters.SumAsync(c => (long)c.UsageCount);
            var thisMonthUsage = await _db.FeatureUsageCounters
                .Where(c => c.Year == now.Year && c.Month == now.Month)
                .SumAsync(c => (long)c.UsageCount);

            // Today and this week — must use FeatureUsageHistory (day-level granularity)
            var todayUsage = await _db.FeatureUsageHistories
                .CountAsync(h => h.Status == "Success" && h.UsedAt >= todayStart);
            var thisWeekUsage = await _db.FeatureUsageHistories
                .CountAsync(h => h.Status == "Success" && h.UsedAt >= weekStart);

            // Feature counts
            var activeFeatures = await _db.Features.CountAsync(f => f.IsActive && f.IsEnabled);
            var disabledFeatures = await _db.Features.CountAsync(f => f.IsActive && !f.IsEnabled);

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                DisabledUsers = totalUsers - activeUsers,
                TotalSubscriptions = totalSubs,
                BasicSubscriptions = basicSubs,
                PremiumSubscriptions = premiumSubs,
                EnterpriseSubscriptions = enterpriseSubs,
                TotalFeatureUsage = totalUsage,
                TodayUsage = todayUsage,
                ThisWeekUsage = thisWeekUsage,
                ThisMonthUsage = thisMonthUsage,
                ActiveFeatures = activeFeatures,
                DisabledFeatures = disabledFeatures
            };
        }
    }
}
