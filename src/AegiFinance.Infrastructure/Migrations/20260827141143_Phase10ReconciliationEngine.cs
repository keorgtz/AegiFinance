using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase10ReconciliationEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReconciliationVersion",
                table: "LedgerEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReconciliationVersion",
                table: "BankStatementLines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ReconciliationCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MatchType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ExplanationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LedgerAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DifferenceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DifferenceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DifferenceReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsAutomatic = table.Column<bool>(type: "bit", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReversedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReversalReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_ReconciliationCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReconciliationCases_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReconciliationCases_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    BankAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LedgerAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DifferenceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DifferenceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Justification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_ReconciliationPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReconciliationPeriods_BankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReconciliationPeriods_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateToleranceDays = table.Column<int>(type: "int", nullable: false),
                    AmountTolerance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SuggestionThreshold = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AutoConfirmThreshold = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AllowAutoConfirmExact = table.Column<bool>(type: "bit", nullable: false),
                    AmountWeight = table.Column<int>(type: "int", nullable: false),
                    DateWeight = table.Column<int>(type: "int", nullable: false),
                    ReferenceWeight = table.Column<int>(type: "int", nullable: false),
                    ClientWeight = table.Column<int>(type: "int", nullable: false),
                    PatternWeight = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ReconciliationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReconciliationSettings_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationCaseBankLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReconciliationCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankStatementLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_ReconciliationCaseBankLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReconciliationCaseBankLines_BankStatementLines_BankStatementLineId",
                        column: x => x.BankStatementLineId,
                        principalTable: "BankStatementLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReconciliationCaseBankLines_ReconciliationCases_ReconciliationCaseId",
                        column: x => x.ReconciliationCaseId,
                        principalTable: "ReconciliationCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationCaseLedgerEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReconciliationCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LedgerEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppliedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_ReconciliationCaseLedgerEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReconciliationCaseLedgerEntries_LedgerEntries_LedgerEntryId",
                        column: x => x.LedgerEntryId,
                        principalTable: "LedgerEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReconciliationCaseLedgerEntries_ReconciliationCases_ReconciliationCaseId",
                        column: x => x.ReconciliationCaseId,
                        principalTable: "ReconciliationCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationCaseBankLines_BankStatementLineId",
                table: "ReconciliationCaseBankLines",
                column: "BankStatementLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationCaseBankLines_ReconciliationCaseId_BankStatementLineId",
                table: "ReconciliationCaseBankLines",
                columns: new[] { "ReconciliationCaseId", "BankStatementLineId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationCaseLedgerEntries_LedgerEntryId",
                table: "ReconciliationCaseLedgerEntries",
                column: "LedgerEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationCaseLedgerEntries_ReconciliationCaseId_LedgerEntryId",
                table: "ReconciliationCaseLedgerEntries",
                columns: new[] { "ReconciliationCaseId", "LedgerEntryId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationCases_BankAccountId_Status_GeneratedAt",
                table: "ReconciliationCases",
                columns: new[] { "BankAccountId", "Status", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationCases_OrganizationId",
                table: "ReconciliationCases",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationPeriods_BankAccountId_StartDate_EndDate",
                table: "ReconciliationPeriods",
                columns: new[] { "BankAccountId", "StartDate", "EndDate" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationPeriods_OrganizationId",
                table: "ReconciliationPeriods",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationSettings_OrganizationId",
                table: "ReconciliationSettings",
                column: "OrganizationId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.Sql("""
                CREATE FUNCTION [dbo].[fn_AegiFinanceReconciliationCaseAccess](@ReconciliationCaseId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL
                   OR EXISTS (SELECT 1 FROM [dbo].[ReconciliationCases] r WHERE r.[Id]=@ReconciliationCaseId AND r.[OrganizationId]=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.OrganizationId')));
                """);

            migrationBuilder.Sql("""
                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[ReconciliationCases],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[ReconciliationCases] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[ReconciliationCases] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[ReconciliationPeriods],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[ReconciliationPeriods] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[ReconciliationPeriods] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[ReconciliationSettings],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[ReconciliationSettings] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[ReconciliationSettings] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceReconciliationCaseAccess]([ReconciliationCaseId]) ON [dbo].[ReconciliationCaseBankLines],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceReconciliationCaseAccess]([ReconciliationCaseId]) ON [dbo].[ReconciliationCaseBankLines] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceReconciliationCaseAccess]([ReconciliationCaseId]) ON [dbo].[ReconciliationCaseBankLines] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceReconciliationCaseAccess]([ReconciliationCaseId]) ON [dbo].[ReconciliationCaseLedgerEntries],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceReconciliationCaseAccess]([ReconciliationCaseId]) ON [dbo].[ReconciliationCaseLedgerEntries] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceReconciliationCaseAccess]([ReconciliationCaseId]) ON [dbo].[ReconciliationCaseLedgerEntries] AFTER UPDATE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                DROP FILTER PREDICATE ON [dbo].[ReconciliationCases],
                DROP BLOCK PREDICATE ON [dbo].[ReconciliationCases],
                DROP FILTER PREDICATE ON [dbo].[ReconciliationPeriods],
                DROP BLOCK PREDICATE ON [dbo].[ReconciliationPeriods],
                DROP FILTER PREDICATE ON [dbo].[ReconciliationSettings],
                DROP BLOCK PREDICATE ON [dbo].[ReconciliationSettings],
                DROP FILTER PREDICATE ON [dbo].[ReconciliationCaseBankLines],
                DROP BLOCK PREDICATE ON [dbo].[ReconciliationCaseBankLines],
                DROP FILTER PREDICATE ON [dbo].[ReconciliationCaseLedgerEntries],
                DROP BLOCK PREDICATE ON [dbo].[ReconciliationCaseLedgerEntries];
                DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceReconciliationCaseAccess];
                """);

            migrationBuilder.DropTable(
                name: "ReconciliationCaseBankLines");

            migrationBuilder.DropTable(
                name: "ReconciliationCaseLedgerEntries");

            migrationBuilder.DropTable(
                name: "ReconciliationPeriods");

            migrationBuilder.DropTable(
                name: "ReconciliationSettings");

            migrationBuilder.DropTable(
                name: "ReconciliationCases");

            migrationBuilder.DropColumn(
                name: "ReconciliationVersion",
                table: "LedgerEntries");

            migrationBuilder.DropColumn(
                name: "ReconciliationVersion",
                table: "BankStatementLines");
        }
    }
}
