using CyberShield.API.DTOs;

namespace CyberShield.API.Services
{
    public interface IAdminUserService
    {
        Task<PagedResult<AdminUserItemDto>> GetUsersAsync(
            int page, int pageSize, string? search, int? packageId, string? status);

        Task<AdminUserDetailDto?> GetUserDetailAsync(string userId);
        Task DisableUserAsync(string adminId, string userId, string reason);
        Task EnableUserAsync(string adminId, string userId);
    }
}
