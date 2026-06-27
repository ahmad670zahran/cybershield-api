using CyberShield.API.Data;
using CyberShield.API.DTOs;
using CyberShield.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.API.Services
{
    public class AdminAuditService : IAdminAuditService
    {
        private readonly ApplicationDbContext _db;

        public AdminAuditService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string adminId, string action, string targetType, string targetId,
                                   string? oldValue = null, string? newValue = null)
        {
            _db.AdminAuditLogs.Add(new AdminAuditLog
            {
                AdminId = adminId,
                Action = action,
                TargetType = targetType,
                TargetId = targetId,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        public async Task<PagedResult<AdminAuditLogItemDto>> GetLogsAsync(
            int page, int pageSize, DateTime? from, DateTime? to, string? action)
        {
            var query = _db.AdminAuditLogs.AsQueryable();

            if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(l => l.CreatedAt <= to.Value);
            if (!string.IsNullOrEmpty(action)) query = query.Where(l => l.Action == action);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new AdminAuditLogItemDto
                {
                    Id = l.Id,
                    AdminId = l.AdminId,
                    Action = l.Action,
                    TargetType = l.TargetType,
                    TargetId = l.TargetId,
                    OldValue = l.OldValue,
                    NewValue = l.NewValue,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<AdminAuditLogItemDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
