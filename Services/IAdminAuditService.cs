using CyberShield.API.DTOs;

namespace CyberShield.API.Services
{
    public interface IAdminAuditService
    {
        Task LogAsync(string adminId, string action, string targetType, string targetId,
                      string? oldValue = null, string? newValue = null);

        Task<PagedResult<AdminAuditLogItemDto>> GetLogsAsync(
            int page, int pageSize, DateTime? from, DateTime? to, string? action);
    }
}
