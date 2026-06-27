using CyberShield.API.DTOs;
using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CyberShield.API.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _userService;

        public AdminUsersController(IAdminUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] int? packageId = null,
            [FromQuery] string? status = null)
        {
            var result = await _userService.GetUsersAsync(page, pageSize, search, packageId, status);
            return Ok(result);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserDetail(string userId)
        {
            var detail = await _userService.GetUserDetailAsync(userId);
            if (detail is null) return NotFound(new { message = "User not found." });
            return Ok(detail);
        }

        [HttpPut("{userId}/disable")]
        public async Task<IActionResult> DisableUser(string userId, [FromBody] DisableUserDto dto)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminId is null) return Unauthorized();

            try
            {
                await _userService.DisableUserAsync(adminId, userId, dto.Reason);
                return Ok(new { message = "User disabled successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{userId}/enable")]
        public async Task<IActionResult> EnableUser(string userId)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminId is null) return Unauthorized();

            try
            {
                await _userService.EnableUserAsync(adminId, userId);
                return Ok(new { message = "User enabled successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
