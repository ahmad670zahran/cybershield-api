using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CyberShield.API.Controllers
{
    [Route("api/admin/analytics")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminAnalyticsController : ControllerBase
    {
        private readonly IAdminAnalyticsService _analyticsService;

        public AdminAnalyticsController(IAdminAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("top-features")]
        public async Task<IActionResult> GetTopFeatures([FromQuery] int topN = 10)
        {
            var result = await _analyticsService.GetTopFeaturesAsync(topN);
            return Ok(result);
        }

        [HttpGet("feature-usage")]
        public async Task<IActionResult> GetFeatureUsageByDate(
            [FromQuery] string featureKey,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (string.IsNullOrEmpty(featureKey))
                return BadRequest(new { message = "featureKey is required." });

            if (from > to)
                return BadRequest(new { message = "from must be before to." });

            var result = await _analyticsService.GetFeatureUsageByDateAsync(featureKey, from, to);
            return Ok(result);
        }

        [HttpGet("packages")]
        public async Task<IActionResult> GetUsageByPackage()
        {
            var result = await _analyticsService.GetUsageByPackageAsync();
            return Ok(result);
        }

        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptionDistribution()
        {
            var result = await _analyticsService.GetSubscriptionDistributionAsync();
            return Ok(result);
        }
    }
}
