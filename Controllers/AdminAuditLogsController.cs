using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CyberShield.API.Controllers
{
    [Route("api/admin/audit-logs")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminAuditLogsController : ControllerBase
    {
        private readonly IAdminAuditService _auditService;

        public AdminAuditLogsController(IAdminAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? action = null)
        {
            var result = await _auditService.GetLogsAsync(page, pageSize, from, to, action);
            return Ok(result);
        }
    }
}
