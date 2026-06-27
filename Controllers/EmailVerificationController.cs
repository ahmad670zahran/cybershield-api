using CyberShield.API.DTOs;
using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailVerificationController : ControllerBase
    {
        private readonly IEntitlementService _entitlementService;
        private readonly IUsageService _usageService;

        public EmailVerificationController(IEntitlementService entitlementService, IUsageService usageService)
        {
            _entitlementService = entitlementService;
            _usageService = usageService;
        }

        public class EmailRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmail([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Please provide a valid email address." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            // Entitlement check
            var check = await _entitlementService.CheckAsync(userId, "EMAIL_VERIFICATION");
            if (!check.Allowed)
                return StatusCode(403, new { message = check.Reason });

            string email = request.Email.Trim().ToLower();
            bool isValidFormat = Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            if (!isValidFormat)
            {
                return Ok(new
                {
                    email,
                    isValid = false,
                    isDisposable = false,
                    status = "Invalid format",
                    riskLevel = "High",
                    recommendation = "The email does not match the standard format."
                });
            }

            var disposableDomains = new HashSet<string>
            {
                "mailinator.com", "yopmail.com", "tempmail.com",
                "10minutemail.com", "sharklasers.com", "guerrillamail.com"
            };

            string domain = email.Split('@')[1];
            bool isDisposable = disposableDomains.Contains(domain);

            string status = isDisposable ? "Disposable / Temporary" : "Valid";
            string riskLevel = isDisposable ? "Medium" : "Safe";
            string recommendation = isDisposable
                ? "This is a temporary email. Use with caution."
                : "This email appears trustworthy.";

            // Register usage
            await _usageService.RegisterAsync(new RegisterUsageDto
            {
                UserId = userId,
                FeatureKey = "EMAIL_VERIFICATION",
                PackageId = check.PackageId ?? 0,
                FeatureId = check.FeatureId ?? 0,
                Status = "Success",
                Metadata = JsonSerializer.Serialize(new { email, domain, isDisposable, isValidFormat })
            });

            return Ok(new { email, domain, isValid = true, isDisposable, status, riskLevel, recommendation });
        }
    }
}
