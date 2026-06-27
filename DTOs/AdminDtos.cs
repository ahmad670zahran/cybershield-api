using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.DTOs
{
    // ── Pagination ────────────────────────────────────────────────────────
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int DisabledUsers { get; set; }

        public int TotalSubscriptions { get; set; }
        public int BasicSubscriptions { get; set; }
        public int PremiumSubscriptions { get; set; }
        public int EnterpriseSubscriptions { get; set; }

        public long TotalFeatureUsage { get; set; }
        public long TodayUsage { get; set; }
        public long ThisWeekUsage { get; set; }
        public long ThisMonthUsage { get; set; }

        public int ActiveFeatures { get; set; }
        public int DisabledFeatures { get; set; }
    }

    // ── Users ─────────────────────────────────────────────────────────────
    public class AdminUserItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Package { get; set; }
        public string Status { get; set; } = string.Empty; // "Active" | "Disabled"
        public DateTime? SubscriptionStart { get; set; }
        public DateTime? SubscriptionEnd { get; set; }
        public long TotalUsage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminUserDetailDto
    {
        public AdminUserItemDto User { get; set; } = new();
        public SubscriptionResponseDto? Subscription { get; set; }
        public UsageSummaryDto? UsageSummary { get; set; }
        public List<UsageHistoryItemDto> RecentActivity { get; set; } = new();
    }

    public class DisableUserDto
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    // ── Features ──────────────────────────────────────────────────────────
    public class AdminFeatureItemDto
    {
        public int Id { get; set; }
        public string FeatureKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public long TotalUsage { get; set; }
        public long TodayUsage { get; set; }
        public long ThisMonthUsage { get; set; }
    }

    public class AdminFeatureDetailDto
    {
        public AdminFeatureItemDto Feature { get; set; } = new();
        public List<PackageResponseDto> AssignedPackages { get; set; } = new();
        public List<UsageHistoryItemDto> RecentActivity { get; set; } = new();
    }

    // ── Audit Logs ────────────────────────────────────────────────────────
    public class AdminAuditLogItemDto
    {
        public long Id { get; set; }
        public string AdminId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
