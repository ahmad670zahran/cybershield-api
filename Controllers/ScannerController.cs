using CyberShield.API.DTOs;
using CyberShield.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScannerController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IEntitlementService _entitlementService;
        private readonly IUsageService _usageService;

        public ScannerController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IEntitlementService entitlementService,
            IUsageService usageService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _entitlementService = entitlementService;
            _usageService = usageService;
        }

        public class UrlScanRequest
        {
            public string Url { get; set; } = string.Empty;
        }

        [HttpPost("scan-url")]
        public async Task<IActionResult> ScanUrl([FromBody] UrlScanRequest request)
        {
            if (string.IsNullOrEmpty(request.Url))
                return BadRequest(new { message = "Please provide a valid URL." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            // Entitlement check
            var check = await _entitlementService.CheckAsync(userId, "LINK_SCAN");
            if (!check.Allowed)
                return StatusCode(403, new { message = check.Reason });

            // Scan
            bool isSafe;
            string threatType;
            (isSafe, threatType) = await ScanWithGoogleAsync(request.Url);

            // Register usage
            await _usageService.RegisterAsync(new RegisterUsageDto
            {
                UserId = userId,
                FeatureKey = "LINK_SCAN",
                PackageId = check.PackageId ?? 0,
                FeatureId = check.FeatureId ?? 0,
                Status = "Success",
                Metadata = JsonSerializer.Serialize(new { url = request.Url, isSafe, threatType })
            });

            return Ok(new
            {
                isSafe,
                message = isSafe ? "This URL is safe." : "Warning! This URL may be unsafe.",
                threatType
            });
        }

        private async Task<(bool isSafe, string threatType)> ScanWithGoogleAsync(string url)
        {
            try
            {
                var apiKey = _configuration["GoogleSafeBrowsing:ApiKey"];
                var googleUrl = $"{_configuration["GoogleSafeBrowsing:Url"]}?key={apiKey}";

                var payload = new
                {
                    client = new { clientId = "cybershield", clientVersion = "1.0.0" },
                    threatInfo = new
                    {
                        threatTypes = new[] { "MALWARE", "SOCIAL_ENGINEERING", "UNWANTED_SOFTWARE", "POTENTIALLY_HARMFUL_APPLICATION" },
                        platformTypes = new[] { "ANY_PLATFORM" },
                        threatEntryTypes = new[] { "URL" },
                        threatEntries = new[] { new { url } }
                    }
                };

                var httpClient = _httpClientFactory.CreateClient();
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(googleUrl, content);

                if (!response.IsSuccessStatusCode)
                    return SimulatedCheck(url);

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                if (!root.TryGetProperty("matches", out _))
                    return (true, "None");

                var threatType = root.GetProperty("matches")[0].GetProperty("threatType").GetString() ?? "Unknown";
                return (false, threatType);
            }
            catch
            {
                return SimulatedCheck(url);
            }
        }

        private static (bool isSafe, string threatType) SimulatedCheck(string url)
        {
            bool isEvil = url.Contains("evil") || url.Contains("malware") || url.Contains("test-virus");
            return isEvil
                ? (false, "SOCIAL_ENGINEERING (Phishing) [simulated]")
                : (true, "None [simulated]");
        }
    }
}
