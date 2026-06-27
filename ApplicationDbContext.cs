using CyberShield.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.API
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Package> Packages { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<PackageFeature> PackageFeatures { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<FeatureUsageHistory> FeatureUsageHistories { get; set; }
        public DbSet<FeatureUsageCounter> FeatureUsageCounters { get; set; }
        public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
        public DbSet<SecurityNews> SecurityNews { get; set; }
        public DbSet<SecurityTip> SecurityTips { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserSubscription>(entity =>
            {
                entity.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(s => s.Package).WithMany(p => p.Subscriptions).HasForeignKey(s => s.PackageId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PackageFeature>(entity =>
            {
                entity.HasOne(pf => pf.Package).WithMany(p => p.PackageFeatures).HasForeignKey(pf => pf.PackageId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(pf => pf.Feature).WithMany(f => f.PackageFeatures).HasForeignKey(pf => pf.FeatureId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(u => u.CurrentPackage).WithMany().HasForeignKey(u => u.CurrentPackageId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<FeatureUsageHistory>(entity =>
            {
                entity.HasOne(h => h.Feature).WithMany().HasForeignKey(h => h.FeatureId).OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<FeatureUsageCounter>(entity =>
            {
                entity.HasOne(c => c.Feature).WithMany().HasForeignKey(c => c.FeatureId).OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(c => new { c.UserId, c.FeatureId, c.Year, c.Month }).IsUnique();
            });

            builder.Entity<AdminAuditLog>(entity =>
            {
                entity.HasIndex(a => a.CreatedAt);
                entity.HasIndex(a => a.Action);
            });
        }
    }
}