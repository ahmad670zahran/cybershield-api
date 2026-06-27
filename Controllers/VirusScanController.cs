using CyberShield.API.DTOs;
using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VirusScanController : ControllerBase
    {
        private readonly IEntitlementService _entitlementService;
        private readonly IUsageService _usageService;

        public VirusScanController(IEntitlementService entitlementService, IUsageService usageService)
        {
            _entitlementService = entitlementService;
            _usageService = usageService;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Please select a valid file to scan." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            // Entitlement check
            var check = await _entitlementService.CheckAsync(userId, "FILE_SCAN");
            if (!check.Allowed)
                return StatusCode(403, new { message = check.Reason });

            // Compute SHA-256 hash
            string fileHash;
            using (var stream = file.OpenReadStream())
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = await sha256.ComputeHashAsync(stream);
                fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            // Scan logic
            bool isSafe = true;
            string threatName = "No threats detected";
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (ext is ".exe" or ".bat" or ".vbs" or ".cmd")
            {
                isSafe = false;
                threatName = "Trojan.Win32.Generic (untrusted executable)";
            }
            else if (file.FileName.Contains("malware") || file.FileName.Contains("virus"))
            {
                isSafe = false;
                threatName = "Worm.Generic.MaliciousPayload";
            }

            // Register usage
            var metadata = JsonSerializer.Serialize(new
            {
                fileName = file.FileName,
                fileSizeKb = Math.Round((double)file.Length / 1024, 2),
                sha256Hash = fileHash,
                isSafe,
                threatName
            });

            await _usageService.RegisterAsync(new RegisterUsageDto
            {
                UserId = userId,
                FeatureKey = "FILE_SCAN",
                PackageId = check.PackageId ?? 0,
                FeatureId = check.FeatureId ?? 0,
                Status = "Success",
                Metadata = metadata
            });

            string remaining = check.Remaining == -1 ? "Unlimited" : (check.Remaining - 1).ToString();

            return Ok(new
            {
                fileName = file.FileName,
                fileSizeKb = Math.Round((double)file.Length / 1024, 2),
                sha256Hash = fileHash,
                scanDate = DateTime.UtcNow,
                isSafe,
                status = isSafe ? "Safe" : "Threat Detected",
                detectedThreat = threatName,
                packageName = check.Package,
                scansUsedThisMonth = (check.Used + 1),
                scansRemainingThisMonth = remaining
            });
        }
    }
}
