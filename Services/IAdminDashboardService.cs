using CyberShield.API.DTOs;

namespace CyberShield.API.Services
{
    public interface IAdminDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync();
    }
}
