using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberShield.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "Features",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisabledAt",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisabledReason",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AdminAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdminId = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TargetType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TargetId = table.Column<string>(type: "TEXT", nullable: false),
                    OldValue = table.Column<string>(type: "TEXT", nullable: true),
                    NewValue = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditLogs_Action",
                table: "AdminAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditLogs_CreatedAt",
                table: "AdminAuditLogs",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAuditLogs");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "DisabledAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DisabledReason",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");
        }
    }
}
