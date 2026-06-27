using CyberShield.API.Data;
using CyberShield.API.DTOs;
using CyberShield.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.API.Services
{
    public class EntitlementService : IEntitlementService
    {
        private readonly ApplicationDbContext _db;

        public EntitlementService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<EntitlementResult> CheckAsync(string userId, string featureKey)
        {
            // 0. Check user is active
            var user = await _db.Users.FindAsync(userId);
            if (user is null || !user.IsActive)
                return Denied("Account is disabled. Please contact support.");

            // 1. Find active subscription
            var subscription = await _db.UserSubscriptions
                .Include(s => s.Package)
                .FirstOrDefaultAsync(s =>
                    s.UserId == userId &&
                    s.Status == SubscriptionStatus.Active &&
                    s.EndDate > DateTime.UtcNow);

            if (subscription is null)
                return Denied("No active subscription found. Please subscribe to a package first.");

            // 2. Find feature in catalog
            var feature = await _db.Features
                .FirstOrDefaultAsync(f => f.FeatureKey == featureKey && f.IsActive);

            if (feature is null)
                return Denied($"Feature '{featureKey}' does not exist.");

            // 2b. Check feature is enabled (admin toggle)
            if (!feature.IsEnabled)
                return Denied("Feature temporarily unavailable.");

            // 3. Check if package includes this feature
            var packageFeature = await _db.PackageFeatures
                .FirstOrDefaultAsync(pf =>
                    pf.PackageId == subscription.PackageId &&
                    pf.FeatureId == feature.Id);

            if (packageFeature is null)
                return Denied($"Feature '{featureKey}' is not included in your {subscription.Package.Name} package.");

            // 4. Unlimited feature
            if (packageFeature.LimitValue == -1)
                return new EntitlementResult
                {
                    Allowed = true,
                    Package = subscription.Package.Name,
                    Limit = -1,
                    Used = 0,
                    Remaining = -1,
                    PackageId = subscription.PackageId,
                    FeatureId = feature.Id
                };

            // 5. Check monthly usage counter
            var now = DateTime.UtcNow;
            var counter = await _db.FeatureUsageCounters
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.FeatureId == feature.Id &&
                    c.Year == now.Year &&
                    c.Month == now.Month);

            int used = counter?.UsageCount ?? 0;

            if (used >= packageFeature.LimitValue)
                return Denied($"Monthly limit of {packageFeature.LimitValue} for '{featureKey}' has been reached.");

            return new EntitlementResult
            {
                Allowed = true,
                Package = subscription.Package.Name,
                Limit = packageFeature.LimitValue,
                Used = used,
                Remaining = packageFeature.LimitValue - used,
                PackageId = subscription.PackageId,
                FeatureId = feature.Id
            };
        }

        private static EntitlementResult Denied(string reason) =>
            new() { Allowed = false, Reason = reason };
    }
}
