using CyberShield.API.DTOs;

namespace CyberShield.API.Services
{
    public interface IAdminFeatureService
    {
        Task<List<AdminFeatureItemDto>> GetFeaturesAsync();
        Task<AdminFeatureDetailDto?> GetFeatureDetailAsync(int featureId);
        Task DisableFeatureAsync(string adminId, int featureId);
        Task EnableFeatureAsync(string adminId, int featureId);
    }
}
