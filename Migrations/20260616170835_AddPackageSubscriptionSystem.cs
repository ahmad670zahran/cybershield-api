using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberShield.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageSubscriptionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ProtectionPlans_CurrentPlanId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "ProtectionPlans");

            migrationBuilder.RenameColumn(
                name: "CurrentPlanId",
                table: "AspNetUsers",
                newName: "CurrentPackageId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_CurrentPlanId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_CurrentPackageId");

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    BillingCycle = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPopular = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PackageId = table.Column<int>(type: "INTEGER", nullable: false),
                    FeatureKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageFeatures_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    PackageId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentMonthFilesScanned = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserSubscriptions_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageFeatures_PackageId",
                table: "PackageFeatures",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_PackageId",
                table: "UserSubscriptions",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Packages_CurrentPackageId",
                table: "AspNetUsers",
                column: "CurrentPackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Packages_CurrentPackageId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "PackageFeatures");

            migrationBuilder.DropTable(
                name: "UserSubscriptions");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.RenameColumn(
                name: "CurrentPackageId",
                table: "AspNetUsers",
                newName: "CurrentPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_CurrentPackageId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_CurrentPlanId");

            migrationBuilder.CreateTable(
                name: "ProtectionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DiscountPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    Features = table.Column<string>(type: "TEXT", nullable: false),
                    HasAdvancedEmailVerification = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasDedicatedAccountManager = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxDevicesAllowed = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxFilesPerMonth = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    OldPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlanName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Rating = table.Column<double>(type: "REAL", nullable: false),
                    ReviewsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UnlimitedLinkScanning = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtectionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProtectionPlanId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentMonthFilesScanned = table.Column<int>(type: "INTEGER", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_ProtectionPlans_ProtectionPlanId",
                        column: x => x.ProtectionPlanId,
                        principalTable: "ProtectionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ProtectionPlanId",
                table: "Subscriptions",
                column: "ProtectionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ProtectionPlans_CurrentPlanId",
                table: "AspNetUsers",
                column: "CurrentPlanId",
                principalTable: "ProtectionPlans",
                principalColumn: "Id");
        }
    }
}
