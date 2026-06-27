using CyberShield.API.Data;
using CyberShield.API.DTOs;
using CyberShield.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CyberShield.API.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAdminAuditService _audit;
        private readonly IUserSubscriptionService _subscriptionService;
        private readonly IUsageService _usageService;

        public AdminUserService(
            ApplicationDbContext db,
            IAdminAuditService audit,
            IUserSubscriptionService subscriptionService,
            IUsageService usageService)
        {
            _db = db;
            _audit = audit;
            _subscriptionService = subscriptionService;
            _usageService = usageService;
        }

        public async Task<PagedResult<AdminUserItemDto>> GetUsersAsync(
            int page, int pageSize, string? search, int? packageId, string? status)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.FullName.Contains(search) || (u.Email != null && u.Email.Contains(search)));

            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status.Equals("Active", StringComparison.OrdinalIgnoreCase);
                query = query.Where(u => u.IsActive == isActive);
            }

            if (packageId.HasValue)
                query = query.Where(u => u.CurrentPackageId == packageId.Value);

            var total = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new { u.Id, u.FullName, u.Email, u.IsActive, u.CreatedAt, u.CurrentPackageId })
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();

            // Load active subscriptions for these users
            var now = DateTime.UtcNow;
            var subscriptions = await _db.UserSubscriptions
                .Include(s => s.Package)
                .Where(s => userIds.Contains(s.UserId) && s.Status == SubscriptionStatus.Active && s.EndDate > now)
                .ToListAsync();

            // Load total usage counts
            var usageCounts = await _db.FeatureUsageCounters
                .Where(c => userIds.Contains(c.UserId))
                .GroupBy(c => c.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(c => (long)c.UsageCount) })
                .ToListAsync();
            var usageMap = usageCounts.ToDictionary(u => u.UserId, u => u.Total);
            var subMap = subscriptions.ToDictionary(s => s.UserId);

            var items = users.Select(u =>
            {
                subMap.TryGetValue(u.Id, out var sub);
                usageMap.TryGetValue(u.Id, out var usage);
                return new AdminUserItemDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    Package = sub?.Package?.Name,
                    Status = u.IsActive ? "Active" : "Disabled",
                    SubscriptionStart = sub?.StartDate,
                    SubscriptionEnd = sub?.EndDate,
                    TotalUsage = usage,
                    CreatedAt = u.CreatedAt
                };
            }).ToList();

            return new PagedResult<AdminUserItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AdminUserDetailDto?> GetUserDetailAsync(string userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return null;

            var now = DateTime.UtcNow;
            var sub = await _db.UserSubscriptions
                .Include(s => s.Package).ThenInclude(p => p.PackageFeatures).ThenInclude(pf => pf.Feature)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.EndDate > now);

            var totalUsage = await _db.FeatureUsageCounters
                .Where(c => c.UserId == userId)
                .SumAsync(c => (long)c.UsageCount);

            var userItem = new AdminUserItemDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Package = user.CurrentPackage?.Name,
                Status = user.IsActive ? "Active" : "Disabled",
                SubscriptionStart = sub?.StartDate,
                SubscriptionEnd = sub?.EndDate,
                TotalUsage = totalUsage,
                CreatedAt = user.CreatedAt
            };

            var subscriptionDto = sub is not null ? UserSubscriptionService.MapToDtoStatic(sub) : null;
            var usageSummary = await _usageService.GetSummaryAsync(userId);
            var recentActivity = await _usageService.GetHistoryAsync(userId);

            return new AdminUserDetailDto
            {
                User = userItem,
                Subscription = subscriptionDto,
                UsageSummary = usageSummary,
                RecentActivity = recentActivity.Take(20).ToList()
            };
        }

        public async Task DisableUserAsync(string adminId, string userId, string reason)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            var old = JsonSerializer.Serialize(new { user.IsActive, user.DisabledReason });

            user.IsActive = false;
            user.DisabledAt = DateTime.UtcNow;
            user.DisabledReason = reason;

            await _db.SaveChangesAsync();
            await _audit.LogAsync(adminId, "DISABLE_USER", "User", userId, old,
                JsonSerializer.Serialize(new { IsActive = false, DisabledReason = reason }));
        }

        public async Task EnableUserAsync(string adminId, string userId)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            var old = JsonSerializer.Serialize(new { user.IsActive, user.DisabledReason });

            user.IsActive = true;
            user.DisabledAt = null;
            user.DisabledReason = null;

            await _db.SaveChangesAsync();
            await _audit.LogAsync(adminId, "ENABLE_USER", "User", userId, old,
                JsonSerializer.Serialize(new { IsActive = true }));
        }
    }
}
