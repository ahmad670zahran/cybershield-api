using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CyberShield.API.Controllers
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminDashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _dashboardService.GetStatsAsync();
            return Ok(stats);
        }
    }
}
