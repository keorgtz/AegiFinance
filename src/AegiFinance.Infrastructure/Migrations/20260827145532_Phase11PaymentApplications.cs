using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase11PaymentApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReversed",
                table: "SubscriptionAllocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentApplicationId",
                table: "SubscriptionAllocations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReversalReason",
                table: "SubscriptionAllocations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReversedAt",
                table: "SubscriptionAllocations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReversedBy",
                table: "SubscriptionAllocations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentAllocationVersion",
                table: "LedgerEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentAllocationVersion",
                table: "BillingItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PaymentApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PreferredServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalPaymentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnappliedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppliedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReversedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReversalReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReappliesPaymentApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PaymentApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentApplications_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentApplications_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentApplications_PaymentApplications_ReappliesPaymentApplicationId",
                        column: x => x.ReappliesPaymentApplicationId,
                        principalTable: "PaymentApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentApplications_Services_PreferredServiceId",
                        column: x => x.PreferredServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentApplicationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultPriority = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
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
                    table.PrimaryKey("PK_PaymentApplicationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentApplicationSettings_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentApplicationPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LedgerEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AvailableBefore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnappliedAfter = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_PaymentApplicationPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentApplicationPayments_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentApplicationPayments_LedgerEntries_LedgerEntryId",
                        column: x => x.LedgerEntryId,
                        principalTable: "LedgerEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentApplicationPayments_PaymentApplications_PaymentApplicationId",
                        column: x => x.PaymentApplicationId,
                        principalTable: "PaymentApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAllocations_PaymentApplicationId_IsReversed",
                table: "SubscriptionAllocations",
                columns: new[] { "PaymentApplicationId", "IsReversed" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplicationPayments_JournalEntryId",
                table: "PaymentApplicationPayments",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplicationPayments_LedgerEntryId",
                table: "PaymentApplicationPayments",
                column: "LedgerEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplicationPayments_PaymentApplicationId_LedgerEntryId",
                table: "PaymentApplicationPayments",
                columns: new[] { "PaymentApplicationId", "LedgerEntryId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplications_ClientId_AppliedAt",
                table: "PaymentApplications",
                columns: new[] { "ClientId", "AppliedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplications_OrganizationId_IdempotencyKey",
                table: "PaymentApplications",
                columns: new[] { "OrganizationId", "IdempotencyKey" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplications_OrganizationId_ReceiptNumber",
                table: "PaymentApplications",
                columns: new[] { "OrganizationId", "ReceiptNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplications_PreferredServiceId",
                table: "PaymentApplications",
                column: "PreferredServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplications_ReappliesPaymentApplicationId",
                table: "PaymentApplications",
                column: "ReappliesPaymentApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplicationSettings_OrganizationId",
                table: "PaymentApplicationSettings",
                column: "OrganizationId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionAllocations_PaymentApplications_PaymentApplicationId",
                table: "SubscriptionAllocations",
                column: "PaymentApplicationId",
                principalTable: "PaymentApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                CREATE FUNCTION [dbo].[fn_AegiFinancePaymentApplicationAccess](@PaymentApplicationId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE EXISTS (
                    SELECT 1 FROM [dbo].[PaymentApplications] AS p
                    WHERE p.[Id]=@PaymentApplicationId
                      AND (SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL OR p.[OrganizationId]=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.OrganizationId')))
                      AND (TRY_CONVERT(bit,SESSION_CONTEXT(N'AegiFinance.IsClient'))=0 OR p.[ClientId]=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.ClientId')))
                );
                """);

            migrationBuilder.Sql("""
                CREATE FUNCTION [dbo].[fn_AegiFinanceBillingItemAccess](@BillingItemId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE EXISTS (
                    SELECT 1 FROM [dbo].[BillingItems] AS b
                    INNER JOIN [dbo].[Clients] AS c ON c.[Id]=b.[ClientId]
                    WHERE b.[Id]=@BillingItemId
                      AND (SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL OR c.[OrganizationId]=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.OrganizationId')))
                      AND (TRY_CONVERT(bit,SESSION_CONTEXT(N'AegiFinance.IsClient'))=0 OR b.[ClientId]=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.ClientId')))
                );
                """);

            migrationBuilder.Sql("""
                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceRootAccess]([ClientId],[OrganizationId]) ON [dbo].[PaymentApplications],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRootAccess]([ClientId],[OrganizationId]) ON [dbo].[PaymentApplications] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRootAccess]([ClientId],[OrganizationId]) ON [dbo].[PaymentApplications] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[PaymentApplicationSettings],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[PaymentApplicationSettings] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[PaymentApplicationSettings] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinancePaymentApplicationAccess]([PaymentApplicationId]) ON [dbo].[PaymentApplicationPayments],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinancePaymentApplicationAccess]([PaymentApplicationId]) ON [dbo].[PaymentApplicationPayments] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinancePaymentApplicationAccess]([PaymentApplicationId]) ON [dbo].[PaymentApplicationPayments] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceBillingItemAccess]([BillingItemId]) ON [dbo].[SubscriptionAllocations],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceBillingItemAccess]([BillingItemId]) ON [dbo].[SubscriptionAllocations] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceBillingItemAccess]([BillingItemId]) ON [dbo].[SubscriptionAllocations] AFTER UPDATE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                DROP FILTER PREDICATE ON [dbo].[PaymentApplications],
                DROP BLOCK PREDICATE ON [dbo].[PaymentApplications],
                DROP FILTER PREDICATE ON [dbo].[PaymentApplicationSettings],
                DROP BLOCK PREDICATE ON [dbo].[PaymentApplicationSettings],
                DROP FILTER PREDICATE ON [dbo].[PaymentApplicationPayments],
                DROP BLOCK PREDICATE ON [dbo].[PaymentApplicationPayments],
                DROP FILTER PREDICATE ON [dbo].[SubscriptionAllocations],
                DROP BLOCK PREDICATE ON [dbo].[SubscriptionAllocations];
                DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinancePaymentApplicationAccess];
                DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceBillingItemAccess];
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionAllocations_PaymentApplications_PaymentApplicationId",
                table: "SubscriptionAllocations");

            migrationBuilder.DropTable(
                name: "PaymentApplicationPayments");

            migrationBuilder.DropTable(
                name: "PaymentApplicationSettings");

            migrationBuilder.DropTable(
                name: "PaymentApplications");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionAllocations_PaymentApplicationId_IsReversed",
                table: "SubscriptionAllocations");

            migrationBuilder.DropColumn(
                name: "IsReversed",
                table: "SubscriptionAllocations");

            migrationBuilder.DropColumn(
                name: "PaymentApplicationId",
                table: "SubscriptionAllocations");

            migrationBuilder.DropColumn(
                name: "ReversalReason",
                table: "SubscriptionAllocations");

            migrationBuilder.DropColumn(
                name: "ReversedAt",
                table: "SubscriptionAllocations");

            migrationBuilder.DropColumn(
                name: "ReversedBy",
                table: "SubscriptionAllocations");

            migrationBuilder.DropColumn(
                name: "PaymentAllocationVersion",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "PaymentAllocationVersion",
                table: "BillingItems");
        }
    }
}
