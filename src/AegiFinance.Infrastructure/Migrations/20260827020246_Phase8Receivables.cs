using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase8Receivables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BillingItems_SubscriptionId_BillingCycleId",
                table: "BillingItems");

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "BillingItems",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodEnd",
                table: "BillingItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodStart",
                table: "BillingItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "BillingItems",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE bi
                SET bi.IdempotencyKey = CONCAT('scheduled:', CONVERT(nvarchar(36), bi.SubscriptionId), ':', CONVERT(nvarchar(36), bi.BillingCycleId)),
                    bi.PeriodStart = bc.StartDate,
                    bi.PeriodEnd = bc.EndDate,
                    bi.Type = 'SubscriptionCharge'
                FROM BillingItems bi
                INNER JOIN BillingCycles bc ON bc.Id = bi.BillingCycleId;
                """);

            migrationBuilder.CreateTable(
                name: "BillingAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ReversedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReversedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReversalReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_BillingAdjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingAdjustments_BillingItems_BillingItemId",
                        column: x => x.BillingItemId,
                        principalTable: "BillingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentPromises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromisedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PromiseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PaymentPromises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentPromises_BillingItems_BillingItemId",
                        column: x => x.BillingItemId,
                        principalTable: "BillingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingItems_IdempotencyKey",
                table: "BillingItems",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingItems_SubscriptionId_BillingCycleId",
                table: "BillingItems",
                columns: new[] { "SubscriptionId", "BillingCycleId" });

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_BillingItemId",
                table: "BillingAdjustments",
                column: "BillingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAdjustments_IdempotencyKey",
                table: "BillingAdjustments",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPromises_BillingItemId_Status_PromiseDate",
                table: "PaymentPromises",
                columns: new[] { "BillingItemId", "Status", "PromiseDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingAdjustments");

            migrationBuilder.DropTable(
                name: "PaymentPromises");

            migrationBuilder.DropIndex(
                name: "IX_BillingItems_IdempotencyKey",
                table: "BillingItems");

            migrationBuilder.DropIndex(
                name: "IX_BillingItems_SubscriptionId_BillingCycleId",
                table: "BillingItems");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "BillingItems");

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                table: "BillingItems");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                table: "BillingItems");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "BillingItems");

            migrationBuilder.CreateIndex(
                name: "IX_BillingItems_SubscriptionId_BillingCycleId",
                table: "BillingItems",
                columns: new[] { "SubscriptionId", "BillingCycleId" },
                unique: true);
        }
    }
}
