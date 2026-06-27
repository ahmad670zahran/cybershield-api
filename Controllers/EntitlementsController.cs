using CyberShield.API.DTOs;
using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntitlementsController : ControllerBase
    {
        private readonly IEntitlementService _entitlementService;

        public EntitlementsController(IEntitlementService entitlementService)
        {
            _entitlementService = entitlementService;
        }

        [HttpPost("check")]
        [Authorize]
        public async Task<IActionResult> Check([FromBody] EntitlementCheckRequest request)
        {
            // Allow checking for the authenticated user only (ignore request.UserId if different)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var result = await _entitlementService.CheckAsync(userId, request.FeatureKey);
            return Ok(result);
        }
    }
}
