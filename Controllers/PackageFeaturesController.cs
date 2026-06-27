using CyberShield.API.Data;
using CyberShield.API.DTOs;
using CyberShield.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.API.Controllers
{
    [Route("api/packages/{packageId}/features")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PackageFeaturesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PackageFeaturesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int packageId, [FromBody] CreatePackageFeatureDto dto)
        {
            if (!await _db.Packages.AnyAsync(p => p.Id == packageId))
                return NotFound(new { message = "Package not found." });

            if (!await _db.Features.AnyAsync(f => f.Id == dto.FeatureId))
                return BadRequest(new { message = "Feature not found." });

            var feature = new PackageFeature
            {
                PackageId = packageId,
                FeatureId = dto.FeatureId,
                LimitValue = dto.LimitValue,
                DisplayOrder = dto.DisplayOrder
            };

            _db.PackageFeatures.Add(feature);
            await _db.SaveChangesAsync();

            await _db.Entry(feature).Reference(f => f.Feature).LoadAsync();

            return CreatedAtAction(null, new PackageFeatureResponseDto
            {
                Id = feature.Id,
                FeatureId = feature.FeatureId,
                FeatureKey = feature.Feature?.FeatureKey ?? string.Empty,
                FeatureName = feature.Feature?.Name ?? string.Empty,
                LimitValue = feature.LimitValue,
                LimitDisplay = feature.LimitValue == -1 ? "Unlimited" : feature.LimitValue.ToString(),
                DisplayOrder = feature.DisplayOrder
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int packageId, int id, [FromBody] UpdatePackageFeatureDto dto)
        {
            var feature = await _db.PackageFeatures
                .Include(f => f.Feature)
                .FirstOrDefaultAsync(f => f.Id == id && f.PackageId == packageId);

            if (feature is null)
                return NotFound(new { message = "Feature not found." });

            if (dto.LimitValue.HasValue) feature.LimitValue = dto.LimitValue.Value;
            if (dto.DisplayOrder.HasValue) feature.DisplayOrder = dto.DisplayOrder.Value;

            await _db.SaveChangesAsync();

            return Ok(new PackageFeatureResponseDto
            {
                Id = feature.Id,
                FeatureId = feature.FeatureId,
                FeatureKey = feature.Feature?.FeatureKey ?? string.Empty,
                FeatureName = feature.Feature?.Name ?? string.Empty,
                LimitValue = feature.LimitValue,
                LimitDisplay = feature.LimitValue == -1 ? "Unlimited" : feature.LimitValue.ToString(),
                DisplayOrder = feature.DisplayOrder
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int packageId, int id)
        {
            var feature = await _db.PackageFeatures
                .FirstOrDefaultAsync(f => f.Id == id && f.PackageId == packageId);

            if (feature is null)
                return NotFound(new { message = "Feature not found." });

            _db.PackageFeatures.Remove(feature);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Feature removed from package." });
        }
    }
}
