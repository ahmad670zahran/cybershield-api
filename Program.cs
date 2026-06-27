using CyberShield.API.Data;
using CyberShield.API.Filters;
using CyberShield.API.Models;
using CyberShield.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Railway: bind to $PORT automatically ──────────────────────
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Override config with environment variables (Railway injects them)
builder.Configuration.AddEnvironmentVariables();

// 1. DbContext — uses /app/data/ in production (Railway volume)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=/app/data/cybershield.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// 2. Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddHttpClient();

// 3. Services
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();
builder.Services.AddScoped<IEntitlementService, EntitlementService>();
builder.Services.AddScoped<IUsageService, UsageService>();

// 3b. Admin Services
builder.Services.AddScoped<IAdminAuditService, AdminAuditService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminFeatureService, AdminFeatureService>();
builder.Services.AddScoped<IAdminAnalyticsService, AdminAnalyticsService>();

// 4. JWT Authentication
var jwtSecret = builder.Configuration["JWT:Secret"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? "CyberShieldSuperSecretKey2026ByZakriaMustBeLongEnough!!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// 5. CORS — يسمح لـ Netlify وأي localhost
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddTransient<ActiveUserFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ActiveUserFilter>();
});
builder.Services.AddEndpointsApiExplorer();

// 6. Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CyberShield API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 7. Health check (Railway uses this)
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Auto-migrate + Seed on startup ────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var db = services.GetRequiredService<ApplicationDbContext>();

    // Run migrations (creates DB if not exists)
    await db.Database.MigrateAsync();

    // Roles
    foreach (var role in new[] { "Admin", "User" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    // Admin user
    const string adminEmail = "superadmin@technocrypt.com";
    if (await userManager.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new ApplicationUser
        {
            UserName = "superadmin",
            Email = adminEmail,
            FullName = "Super Admin",
            EmailConfirmed = true,
            IsActive = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };
        var result = await userManager.CreateAsync(admin, "AdminSecure123!@#");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            await userManager.AddToRoleAsync(admin, "User");
        }
    }

    // Feature catalog
    if (!db.Features.Any())
    {
        var features = new List<Feature>
        {
            new() { FeatureKey = "FILE_SCAN",               Name = "File Scan",                  Description = "Scan files for malware and threats." },
            new() { FeatureKey = "LINK_SCAN",               Name = "Link Scan",                  Description = "Scan URLs for phishing and malware." },
            new() { FeatureKey = "EMAIL_VERIFICATION",      Name = "Email Verification",         Description = "Verify email addresses and detect disposable emails." },
            new() { FeatureKey = "PHISHING_PROTECTION",     Name = "Phishing Protection",        Description = "Advanced phishing detection and protection." },
            new() { FeatureKey = "PASSWORD_GENERATOR",      Name = "Password Generator",         Description = "Generate strong, cryptographically secure passwords." },
            new() { FeatureKey = "DATA_BACKUP",             Name = "Data Backup",                Description = "Automated data backup service." },
            new() { FeatureKey = "REPORTS",                 Name = "Security Reports",           Description = "Generate security analysis reports." },
            new() { FeatureKey = "SECURITY_NEWS",           Name = "Security News",              Description = "Access to latest cybersecurity news." },
            new() { FeatureKey = "SECURITY_TIPS",           Name = "Security Tips",              Description = "Daily cybersecurity tips and best practices." },
            new() { FeatureKey = "TEAM_SCANNING",           Name = "Team Scanning",              Description = "Unlimited scanning for all team members." },
            new() { FeatureKey = "CUSTOM_DASHBOARD",        Name = "Custom Dashboard",           Description = "Fully customizable security dashboard." },
            new() { FeatureKey = "ACCOUNT_MANAGER",         Name = "Dedicated Account Manager", Description = "A dedicated security consultant for your account." },
            new() { FeatureKey = "TEAM_TRAINING",           Name = "Team Training",              Description = "Cybersecurity training sessions for your team." },
            new() { FeatureKey = "INTEGRATIONS",            Name = "System Integrations",        Description = "Integration with existing enterprise systems." },
            new() { FeatureKey = "SLA",                     Name = "Custom SLA",                 Description = "Custom Service Level Agreement." },
            new() { FeatureKey = "SECURITY_CONSULTATIONS",  Name = "Security Consultations",     Description = "On-demand security consultation sessions." }
        };
        db.Features.AddRange(features);
        await db.SaveChangesAsync();
    }

    // Packages
    if (!db.Packages.Any())
    {
        var featureMap = await db.Features.ToDictionaryAsync(f => f.FeatureKey, f => f.Id);
        int F(string key) => featureMap[key];

        var packages = new List<Package>
        {
            new Package
            {
                Name = "Basic",
                Description = "Essential cybersecurity tools for individuals.",
                CurrentPrice = 149,
                OriginalPrice = 149,
                Currency = "EGP",
                BillingCycle = BillingCycle.Monthly,
                IsPopular = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PackageFeatures = new List<PackageFeature>
                {
                    new() { FeatureId = F("FILE_SCAN"),           LimitValue = 100, DisplayOrder = 1 },
                    new() { FeatureId = F("LINK_SCAN"),           LimitValue = -1,  DisplayOrder = 2 },
                    new() { FeatureId = F("PASSWORD_GENERATOR"),  LimitValue = -1,  DisplayOrder = 3 },
                    new() { FeatureId = F("SECURITY_TIPS"),       LimitValue = -1,  DisplayOrder = 4 },
                    new() { FeatureId = F("REPORTS"),             LimitValue = -1,  DisplayOrder = 5 }
                }
            },
            new Package
            {
                Name = "Premium",
                Description = "Advanced protection for power users.",
                CurrentPrice = 299,
                OriginalPrice = 399,
                Currency = "EGP",
                BillingCycle = BillingCycle.Monthly,
                IsPopular = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PackageFeatures = new List<PackageFeature>
                {
                    new() { FeatureId = F("FILE_SCAN"),            LimitValue = -1, DisplayOrder = 1 },
                    new() { FeatureId = F("LINK_SCAN"),            LimitValue = -1, DisplayOrder = 2 },
                    new() { FeatureId = F("EMAIL_VERIFICATION"),   LimitValue = -1, DisplayOrder = 3 },
                    new() { FeatureId = F("PHISHING_PROTECTION"),  LimitValue = -1, DisplayOrder = 4 },
                    new() { FeatureId = F("SECURITY_NEWS"),        LimitValue = -1, DisplayOrder = 5 },
                    new() { FeatureId = F("PASSWORD_GENERATOR"),   LimitValue = -1, DisplayOrder = 6 },
                    new() { FeatureId = F("DATA_BACKUP"),          LimitValue = -1, DisplayOrder = 7 },
                    new() { FeatureId = F("REPORTS"),              LimitValue = -1, DisplayOrder = 8 },
                    new() { FeatureId = F("SECURITY_TIPS"),        LimitValue = -1, DisplayOrder = 9 }
                }
            },
            new Package
            {
                Name = "Enterprise",
                Description = "Complete cybersecurity suite for teams and businesses.",
                CurrentPrice = 1999,
                OriginalPrice = 2499,
                Currency = "EGP",
                BillingCycle = BillingCycle.Monthly,
                IsPopular = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PackageFeatures = new List<PackageFeature>
                {
                    new() { FeatureId = F("FILE_SCAN"),               LimitValue = -1, DisplayOrder = 1 },
                    new() { FeatureId = F("LINK_SCAN"),               LimitValue = -1, DisplayOrder = 2 },
                    new() { FeatureId = F("EMAIL_VERIFICATION"),      LimitValue = -1, DisplayOrder = 3 },
                    new() { FeatureId = F("PHISHING_PROTECTION"),     LimitValue = -1, DisplayOrder = 4 },
                    new() { FeatureId = F("SECURITY_NEWS"),           LimitValue = -1, DisplayOrder = 5 },
                    new() { FeatureId = F("PASSWORD_GENERATOR"),      LimitValue = -1, DisplayOrder = 6 },
                    new() { FeatureId = F("DATA_BACKUP"),             LimitValue = -1, DisplayOrder = 7 },
                    new() { FeatureId = F("REPORTS"),                 LimitValue = -1, DisplayOrder = 8 },
                    new() { FeatureId = F("SECURITY_TIPS"),           LimitValue = -1, DisplayOrder = 9 },
                    new() { FeatureId = F("TEAM_SCANNING"),           LimitValue = -1, DisplayOrder = 10 },
                    new() { FeatureId = F("CUSTOM_DASHBOARD"),        LimitValue = -1, DisplayOrder = 11 },
                    new() { FeatureId = F("ACCOUNT_MANAGER"),         LimitValue = -1, DisplayOrder = 12 },
                    new() { FeatureId = F("TEAM_TRAINING"),           LimitValue = -1, DisplayOrder = 13 },
                    new() { FeatureId = F("INTEGRATIONS"),            LimitValue = -1, DisplayOrder = 14 },
                    new() { FeatureId = F("SLA"),                     LimitValue = -1, DisplayOrder = 15 },
                    new() { FeatureId = F("SECURITY_CONSULTATIONS"),  LimitValue = -1, DisplayOrder = 16 }
                }
            }
        };
        db.Packages.AddRange(packages);
        await db.SaveChangesAsync();
    }
}

// ── Middleware Pipeline ───────────────────────────────────────
// Swagger في كل البيئات (مش development بس) عشان تقدر تتيست على Railway
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
