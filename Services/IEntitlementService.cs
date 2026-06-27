using CyberShield.API.DTOs;

namespace CyberShield.API.Services
{
    public interface IEntitlementService
    {
        Task<EntitlementResult> CheckAsync(string userId, string featureKey);
    }
}
