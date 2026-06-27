using CyberShield.API.Data;
using CyberShield.API.DTOs;
using CyberShield.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.API.Services
{
    public class UserSubscriptionService : IUserSubscriptionService
    {
        private readonly ApplicationDbContext _db;

        public UserSubscriptionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SubscriptionResponseDto> SubscribeAsync(string userId, int packageId)
        {
            var package = await _db.Packages
                .Include(p => p.PackageFeatures).ThenInclude(pf => pf.Feature)
                .FirstOrDefaultAsync(p => p.Id == packageId && p.IsActive)
                ?? throw new InvalidOperationException("Package not found or inactive.");

            var existing = await _db.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

            if (existing is not null)
                throw new InvalidOperationException("User already has an active subscription. Use upgrade instead.");

            var subscription = new UserSubscription
            {
                UserId = userId,
                PackageId = packageId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = SubscriptionStatus.Active,
                AmountPaid = package.CurrentPrice,
                CreatedAt = DateTime.UtcNow
            };

            _db.UserSubscriptions.Add(subscription);

            var user = await _db.Users.FindAsync(userId);
            if (user is not null) user.CurrentPackageId = packageId;

            await _db.SaveChangesAsync();

            subscription.Package = package;
            return MapToDto(subscription);
        }

        public async Task<SubscriptionResponseDto> UpgradeAsync(string userId, int newPackageId)
        {
            var newPackage = await _db.Packages
                .Include(p => p.PackageFeatures).ThenInclude(pf => pf.Feature)
                .FirstOrDefaultAsync(p => p.Id == newPackageId && p.IsActive)
                ?? throw new InvalidOperationException("Target package not found or inactive.");

            var current = await _db.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

            if (current is not null)
                current.Status = SubscriptionStatus.Cancelled;

            var newSubscription = new UserSubscription
            {
                UserId = userId,
                PackageId = newPackageId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                Status = SubscriptionStatus.Active,
                AmountPaid = newPackage.CurrentPrice,
                CreatedAt = DateTime.UtcNow
            };

            _db.UserSubscriptions.Add(newSubscription);

            var user = await _db.Users.FindAsync(userId);
            if (user is not null) user.CurrentPackageId = newPackageId;

            await _db.SaveChangesAsync();

            newSubscription.Package = newPackage;
            return MapToDto(newSubscription);
        }

        public async Task<bool> CancelAsync(string userId)
        {
            var subscription = await _db.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

            if (subscription is null) return false;

            subscription.Status = SubscriptionStatus.Cancelled;

            var user = await _db.Users.FindAsync(userId);
            if (user is not null) user.CurrentPackageId = null;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<SubscriptionResponseDto?> GetCurrentAsync(string userId)
        {
            var subscription = await _db.UserSubscriptions
                .Include(s => s.Package)
                    .ThenInclude(p => p.PackageFeatures.OrderBy(pf => pf.DisplayOrder))
                        .ThenInclude(pf => pf.Feature)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

            return subscription is null ? null : MapToDto(subscription);
        }

        public async Task<List<SubscriptionResponseDto>> GetHistoryAsync(string userId)
        {
            var subscriptions = await _db.UserSubscriptions
                .Include(s => s.Package)
                    .ThenInclude(p => p.PackageFeatures.OrderBy(pf => pf.DisplayOrder))
                        .ThenInclude(pf => pf.Feature)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return subscriptions.Select(MapToDto).ToList();
        }

        internal static SubscriptionResponseDto MapToDtoStatic(UserSubscription s) => MapToDto(s);

        private static SubscriptionResponseDto MapToDto(UserSubscription s) => new()
        {
            Id = s.Id,
            UserId = s.UserId,
            PackageId = s.PackageId,
            PackageName = s.Package?.Name ?? string.Empty,
            PackagePrice = s.Package?.CurrentPrice ?? 0,
            Currency = s.Package?.Currency ?? "EGP",
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Status = s.Status.ToString(),
            AmountPaid = s.AmountPaid,
            CreatedAt = s.CreatedAt,
            Features = s.Package?.PackageFeatures.Select(pf => new PackageFeatureResponseDto
            {
                Id = pf.Id,
                FeatureId = pf.FeatureId,
                FeatureKey = pf.Feature?.FeatureKey ?? string.Empty,
                FeatureName = pf.Feature?.Name ?? string.Empty,
                LimitValue = pf.LimitValue,
                LimitDisplay = pf.LimitValue == -1 ? "Unlimited" : pf.LimitValue.ToString(),
                DisplayOrder = pf.DisplayOrder
            }).ToList() ?? new()
        };
    }
}
