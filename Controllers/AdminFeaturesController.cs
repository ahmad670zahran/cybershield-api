using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CyberShield.API.Controllers
{
    [Route("api/admin/features")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminFeaturesController : ControllerBase
    {
        private readonly IAdminFeatureService _featureService;

        public AdminFeaturesController(IAdminFeatureService featureService)
        {
            _featureService = featureService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFeatures()
        {
            var features = await _featureService.GetFeaturesAsync();
            return Ok(features);
        }

        [HttpGet("{featureId:int}")]
        public async Task<IActionResult> GetFeatureDetail(int featureId)
        {
            var detail = await _featureService.GetFeatureDetailAsync(featureId);
            if (detail is null) return NotFound(new { message = "Feature not found." });
            return Ok(detail);
        }

        [HttpPut("{featureId:int}/disable")]
        public async Task<IActionResult> DisableFeature(int featureId)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminId is null) return Unauthorized();

            try
            {
                await _featureService.DisableFeatureAsync(adminId, featureId);
                return Ok(new { message = "Feature disabled. All future entitlement checks will be denied." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{featureId:int}/enable")]
        public async Task<IActionResult> EnableFeature(int featureId)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminId is null) return Unauthorized();

            try
            {
                await _featureService.EnableFeatureAsync(adminId, featureId);
                return Ok(new { message = "Feature enabled." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
