using CyberShield.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public class RegisterModel
        {
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
        }

        public class LoginModel
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest(new { message = "Email is required." });

            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Password is required." });

            // Auto-generate username from email if not provided
            if (string.IsNullOrWhiteSpace(model.Username))
                model.Username = model.Email.Split('@')[0];

            // Auto-generate FullName from username if not provided
            if (string.IsNullOrWhiteSpace(model.FullName))
                model.FullName = model.Username;

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return BadRequest(new { message = "This email is already registered." });

            // Check if username is taken
            var usernameExists = await _userManager.FindByNameAsync(model.Username);
            if (usernameExists != null)
                model.Username = model.Username + "_" + new Random().Next(100, 999);

            ApplicationUser user = new()
            {
                Email = model.Email,
                UserName = model.Username,
                FullName = model.FullName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "Registration failed.", errors });
            }

            // Ensure User role exists
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new
            {
                message = "Account created successfully!",
                userId = user.Id,
                username = user.UserName,
                email = user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Email and password are required." });

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { message = "Invalid email or password." });

            if (!user.IsActive)
                return Unauthorized(new { message = "Account is disabled. Please contact support." });

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddHours(8),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                userId = user.Id,
                username = user.UserName,
                email = user.Email,
                fullName = user.FullName,
                roles = userRoles,
                message = "Login successful!"
            });
        }
    }
}
