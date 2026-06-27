using CyberShield.API.DTOs;

namespace CyberShield.API.Services
{
    public interface IUsageService
    {
        Task RegisterAsync(RegisterUsageDto dto);
        Task<UsageSummaryDto> GetSummaryAsync(string userId);
        Task<List<UsageHistoryItemDto>> GetHistoryAsync(string userId);
    }
}
