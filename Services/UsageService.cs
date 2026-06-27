using CyberShield.API.Data;
using CyberShield.API.DTOs;
using CyberShield.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.API.Services
{
    public class UsageService : IUsageService
    {
        private readonly ApplicationDbContext _db;

        public UsageService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task RegisterAsync(RegisterUsageDto dto)
        {
            var now = DateTime.UtcNow;

            // 1. Insert history record
            _db.FeatureUsageHistories.Add(new FeatureUsageHistory
            {
                UserId = dto.UserId,
                FeatureId = dto.FeatureId,
                PackageId = dto.PackageId,
                RequestId = dto.RequestId,
                Status = dto.Status,
                UsedAt = now,
                Metadata = dto.Metadata
            });

            // 2. Upsert monthly counter (only increment on success)
            if (dto.Status == "Success")
            {
                var counter = await _db.FeatureUsageCounters
                    .FirstOrDefaultAsync(c =>
                        c.UserId == dto.UserId &&
                        c.FeatureId == dto.FeatureId &&
                        c.Year == now.Year &&
                        c.Month == now.Month);

                if (counter is null)
                {
                    _db.FeatureUsageCounters.Add(new FeatureUsageCounter
                    {
                        UserId = dto.UserId,
                        FeatureId = dto.FeatureId,
                        Year = now.Year,
                        Month = now.Month,
                        UsageCount = 1
                    });
                }
                else
                {
                    counter.UsageCount++;
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task<UsageSummaryDto> GetSummaryAsync(string userId)
        {
            var subscription = await _db.UserSubscriptions
                .Include(s => s.Package)
                    .ThenInclude(p => p.PackageFeatures)
                        .ThenInclude(pf => pf.Feature)
                .FirstOrDefaultAsync(s =>
                    s.UserId == userId &&
                    s.Status == SubscriptionStatus.Active &&
                    s.EndDate > DateTime.UtcNow);

            if (subscription is null)
                return new UsageSummaryDto { Package = "No active subscription" };

            var now = DateTime.UtcNow;
            var counters = await _db.FeatureUsageCounters
                .Where(c => c.UserId == userId && c.Year == now.Year && c.Month == now.Month)
                .ToDictionaryAsync(c => c.FeatureId, c => c.UsageCount);

            var features = subscription.Package.PackageFeatures
                .OrderBy(pf => pf.DisplayOrder)
                .Select(pf =>
                {
                    int used = counters.TryGetValue(pf.FeatureId, out var count) ? count : 0;
                    return new UsageFeatureItemDto
                    {
                        FeatureKey = pf.Feature.FeatureKey,
                        FeatureName = pf.Feature.Name,
                        Limit = pf.LimitValue,
                        Used = pf.LimitValue == -1 ? used : used,
                        Remaining = pf.LimitValue == -1 ? -1 : pf.LimitValue - used
                    };
                }).ToList();

            return new UsageSummaryDto
            {
                Package = subscription.Package.Name,
                Features = features
            };
        }

        public async Task<List<UsageHistoryItemDto>> GetHistoryAsync(string userId)
        {
            return await _db.FeatureUsageHistories
                .Include(h => h.Feature)
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.UsedAt)
                .Select(h => new UsageHistoryItemDto
                {
                    FeatureKey = h.Feature.FeatureKey,
                    FeatureName = h.Feature.Name,
                    Status = h.Status,
                    Metadata = h.Metadata,
                    UsedAt = h.UsedAt
                })
                .ToListAsync();
        }
    }
}
