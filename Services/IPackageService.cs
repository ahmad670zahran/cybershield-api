using CyberShield.API.DTOs;

namespace CyberShield.API.Services
{
    public interface IPackageService
    {
        Task<List<PackageResponseDto>> GetAllAsync(bool includeInactive = false);
        Task<PackageResponseDto?> GetByIdAsync(int id);
        Task<PackageResponseDto> CreateAsync(CreatePackageDto dto);
        Task<PackageResponseDto?> UpdateAsync(int id, UpdatePackageDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
