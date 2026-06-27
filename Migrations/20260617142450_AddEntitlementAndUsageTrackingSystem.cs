using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberShield.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEntitlementAndUsageTrackingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentMonthFilesScanned",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "FeatureKey",
                table: "PackageFeatures");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "PackageFeatures");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "PackageFeatures");

            migrationBuilder.AddColumn<int>(
                name: "FeatureId",
                table: "PackageFeatures",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LimitValue",
                table: "PackageFeatures",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeatureKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureUsageCounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    FeatureId = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: false),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureUsageCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureUsageCounters_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeatureUsageHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    FeatureId = table.Column<int>(type: "INTEGER", nullable: false),
                    PackageId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UsedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureUsageHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureUsageHistories_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageFeatures_FeatureId",
                table: "PackageFeatures",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureUsageCounters_FeatureId",
                table: "FeatureUsageCounters",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureUsageCounters_UserId_FeatureId_Year_Month",
                table: "FeatureUsageCounters",
                columns: new[] { "UserId", "FeatureId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureUsageHistories_FeatureId",
                table: "FeatureUsageHistories",
                column: "FeatureId");

            migrationBuilder.AddForeignKey(
                name: "FK_PackageFeatures_Features_FeatureId",
                table: "PackageFeatures",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackageFeatures_Features_FeatureId",
                table: "PackageFeatures");

            migrationBuilder.DropTable(
                name: "FeatureUsageCounters");

            migrationBuilder.DropTable(
                name: "FeatureUsageHistories");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropIndex(
                name: "IX_PackageFeatures_FeatureId",
                table: "PackageFeatures");

            migrationBuilder.DropColumn(
                name: "FeatureId",
                table: "PackageFeatures");

            migrationBuilder.DropColumn(
                name: "LimitValue",
                table: "PackageFeatures");

            migrationBuilder.AddColumn<int>(
                name: "CurrentMonthFilesScanned",
                table: "UserSubscriptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FeatureKey",
                table: "PackageFeatures",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "PackageFeatures",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "PackageFeatures",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
