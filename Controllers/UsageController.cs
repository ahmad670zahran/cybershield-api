using CyberShield.API.DTOs;
using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsageController : ControllerBase
    {
        private readonly IUsageService _usageService;

        public UsageController(IUsageService usageService)
        {
            _usageService = usageService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUsageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            dto.UserId = userId;
            await _usageService.RegisterAsync(dto);
            return Ok(new { message = "Usage registered successfully." });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var summary = await _usageService.GetSummaryAsync(userId);
            return Ok(summary);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var history = await _usageService.GetHistoryAsync(userId);
            return Ok(history);
        }
    }
}
