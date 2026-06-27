using Microsoft.AspNetCore.Identity;

namespace CyberShield.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int FilesScannedCount { get; set; } = 0;
        public int LinksScannedCount { get; set; } = 0;

        public int? CurrentPackageId { get; set; }
        public virtual Package? CurrentPackage { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? DisabledAt { get; set; }
        public string? DisabledReason { get; set; }
    }
}
