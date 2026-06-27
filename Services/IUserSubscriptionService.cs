using CyberShield.API.DTOs;

namespace CyberShield.API.Services
{
    public interface IUserSubscriptionService
    {
        Task<SubscriptionResponseDto> SubscribeAsync(string userId, int packageId);
        Task<SubscriptionResponseDto> UpgradeAsync(string userId, int newPackageId);
        Task<bool> CancelAsync(string userId);
        Task<SubscriptionResponseDto?> GetCurrentAsync(string userId);
        Task<List<SubscriptionResponseDto>> GetHistoryAsync(string userId);
    }
}
