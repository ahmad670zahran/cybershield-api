using CyberShield.API.DTOs;
using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CyberShield.API.Controllers
{
    [Route("api/subscriptions")]
    [ApiController]
    [Authorize]
    public class UserSubscriptionsController : ControllerBase
    {
        private readonly IUserSubscriptionService _subscriptionService;

        public UserSubscriptionsController(IUserSubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            try
            {
                var result = await _subscriptionService.SubscribeAsync(userId, dto.PackageId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("upgrade")]
        public async Task<IActionResult> Upgrade([FromBody] UpgradeSubscriptionDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            try
            {
                var result = await _subscriptionService.UpgradeAsync(userId, dto.NewPackageId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var cancelled = await _subscriptionService.CancelAsync(userId);
            if (!cancelled)
                return NotFound(new { message = "No active subscription found." });

            return Ok(new { message = "Subscription cancelled successfully." });
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var subscription = await _subscriptionService.GetCurrentAsync(userId);
            if (subscription is null)
                return NotFound(new { message = "No active subscription found." });

            return Ok(subscription);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var history = await _subscriptionService.GetHistoryAsync(userId);
            return Ok(history);
        }
    }
}
