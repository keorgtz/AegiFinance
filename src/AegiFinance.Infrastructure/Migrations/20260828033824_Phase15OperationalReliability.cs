using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase15OperationalReliability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockOwner = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TraceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutboxMessages_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_OrganizationId_IdempotencyKey",
                table: "OutboxMessages",
                columns: new[] { "OrganizationId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_AvailableAt_LockedUntil",
                table: "OutboxMessages",
                columns: new[] { "Status", "AvailableAt", "LockedUntil" });

            migrationBuilder.Sql("""
                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[OutboxMessages],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[OutboxMessages] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[OutboxMessages] AFTER UPDATE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                DROP FILTER PREDICATE ON [dbo].[OutboxMessages],
                DROP BLOCK PREDICATE ON [dbo].[OutboxMessages];
                """);

            migrationBuilder.DropTable(
                name: "OutboxMessages");
        }
    }
}
