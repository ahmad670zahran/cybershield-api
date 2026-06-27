using CyberShield.API.DTOs;
using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PasswordController : ControllerBase
    {
        private readonly IEntitlementService _entitlementService;
        private readonly IUsageService _usageService;

        public PasswordController(IEntitlementService entitlementService, IUsageService usageService)
        {
            _entitlementService = entitlementService;
            _usageService = usageService;
        }

        public class PasswordSettings
        {
            public int Length { get; set; } = 12;
            public bool IncludeUppercase { get; set; } = true;
            public bool IncludeLowercase { get; set; } = true;
            public bool IncludeNumbers { get; set; } = true;
            public bool IncludeSpecialChars { get; set; } = true;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePassword([FromBody] PasswordSettings settings)
        {
            if (settings.Length < 6 || settings.Length > 64)
                return BadRequest(new { message = "Password length must be between 6 and 64 characters." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            // Entitlement check
            var check = await _entitlementService.CheckAsync(userId, "PASSWORD_GENERATOR");
            if (!check.Allowed)
                return StatusCode(403, new { message = check.Reason });

            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var charPool = new StringBuilder();
            var guaranteedChars = new List<char>();

            if (settings.IncludeUppercase) { charPool.Append(uppercase); guaranteedChars.Add(uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)]); }
            if (settings.IncludeLowercase) { charPool.Append(lowercase); guaranteedChars.Add(lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)]); }
            if (settings.IncludeNumbers) { charPool.Append(numbers); guaranteedChars.Add(numbers[RandomNumberGenerator.GetInt32(numbers.Length)]); }
            if (settings.IncludeSpecialChars) { charPool.Append(specialChars); guaranteedChars.Add(specialChars[RandomNumberGenerator.GetInt32(specialChars.Length)]); }

            if (charPool.Length == 0)
                return BadRequest(new { message = "Please select at least one character type." });

            int remaining = settings.Length - guaranteedChars.Count;
            for (int i = 0; i < remaining; i++)
                guaranteedChars.Add(charPool[RandomNumberGenerator.GetInt32(charPool.Length)]);

            var finalPassword = guaranteedChars.OrderBy(_ => RandomNumberGenerator.GetInt32(100)).ToArray();

            string strength = settings.Length >= 12 && settings.IncludeSpecialChars && settings.IncludeNumbers
                ? "Very Strong"
                : settings.Length >= 8 ? "Medium" : "Weak";

            // Register usage
            await _usageService.RegisterAsync(new RegisterUsageDto
            {
                UserId = userId,
                FeatureKey = "PASSWORD_GENERATOR",
                PackageId = check.PackageId ?? 0,
                FeatureId = check.FeatureId ?? 0,
                Status = "Success",
                Metadata = JsonSerializer.Serialize(new { length = settings.Length, strength })
            });

            return Ok(new
            {
                password = new string(finalPassword),
                length = settings.Length,
                strength
            });
        }
    }
}
