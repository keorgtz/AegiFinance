using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase9BankStatementImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BankStatements_BankAccountId",
                table: "BankStatements");

            migrationBuilder.DropIndex(
                name: "IX_BankImportAttempts_BankAccountId",
                table: "BankImportAttempts");

            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "BankStatements",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImportAttemptId",
                table: "BankStatements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BankBalance",
                table: "BankStatementLines",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "BankStatementLines",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeduplicationHash",
                table: "BankStatementLines",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceRowNumber",
                table: "BankStatementLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdapterCode",
                table: "BankImportAttempts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "BankStatementId",
                table: "BankImportAttempts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DuplicateRecords",
                table: "BankImportAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "BankImportAttempts",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "BankImportAttempts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "BankImportAttempts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IncompleteRecords",
                table: "BankImportAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ProfileId",
                table: "BankImportAttempts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedRecords",
                table: "BankImportAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RolledBackAt",
                table: "BankImportAttempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RolledBackBy",
                table: "BankImportAttempts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalRecords",
                table: "BankImportAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ValidRecords",
                table: "BankImportAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE bsl
                SET [Currency] = ba.[Currency]
                FROM [BankStatementLines] bsl
                INNER JOIN [BankStatements] bs ON bs.[Id] = bsl.[BankStatementId]
                INNER JOIN [BankAccounts] ba ON ba.[Id] = bs.[BankAccountId]
                WHERE bsl.[Currency] = '';

                UPDATE [BankImportAttempts]
                SET [Status] = CASE WHEN [Status] IN ('Succeeded', 'Committed') THEN 'Committed' ELSE 'Failed' END,
                    [AdapterCode] = 'generic',
                    [FileType] = CASE WHEN CHARINDEX('.', REVERSE([FileName])) > 0
                        THEN LOWER(RIGHT([FileName], CHARINDEX('.', REVERSE([FileName])) - 1)) ELSE 'csv' END,
                    [FileHash] = LOWER(CONVERT(varchar(64), HASHBYTES('SHA2_256', CONVERT(varchar(36), [Id])), 2));
                """);

            migrationBuilder.CreateTable(
                name: "BankImportProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AdapterCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateColumn = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    DescriptionColumn = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ReferenceColumn = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    AmountColumn = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DebitColumn = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CreditColumn = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    CurrencyColumn = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    BalanceColumn = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DateFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Delimiter = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    HeaderRow = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_BankImportProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankImportProfiles_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BankImportRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    RawDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DeduplicationHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IssuesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankStatementLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_BankImportRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankImportRows_BankImportAttempts_ImportAttemptId",
                        column: x => x.ImportAttemptId,
                        principalTable: "BankImportAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankImportRows_BankStatementLines_BankStatementLineId",
                        column: x => x.BankStatementLineId,
                        principalTable: "BankStatementLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_BankAccountId_FileHash",
                table: "BankStatements",
                columns: new[] { "BankAccountId", "FileHash" },
                filter: "[FileHash] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatementLines_DeduplicationHash",
                table: "BankStatementLines",
                column: "DeduplicationHash",
                unique: true,
                filter: "[DeduplicationHash] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BankImportAttempts_BankAccountId_FileHash",
                table: "BankImportAttempts",
                columns: new[] { "BankAccountId", "FileHash" });

            migrationBuilder.CreateIndex(
                name: "IX_BankImportAttempts_BankStatementId",
                table: "BankImportAttempts",
                column: "BankStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_BankImportAttempts_ProfileId",
                table: "BankImportAttempts",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BankImportProfiles_OrganizationId_Name",
                table: "BankImportProfiles",
                columns: new[] { "OrganizationId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BankImportRows_BankStatementLineId",
                table: "BankImportRows",
                column: "BankStatementLineId");

            migrationBuilder.CreateIndex(
                name: "IX_BankImportRows_ImportAttemptId_RowNumber",
                table: "BankImportRows",
                columns: new[] { "ImportAttemptId", "RowNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_BankImportRows_Status_DeduplicationHash",
                table: "BankImportRows",
                columns: new[] { "Status", "DeduplicationHash" });

            migrationBuilder.AddForeignKey(
                name: "FK_BankImportAttempts_BankImportProfiles_ProfileId",
                table: "BankImportAttempts",
                column: "ProfileId",
                principalTable: "BankImportProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BankImportAttempts_BankStatements_BankStatementId",
                table: "BankImportAttempts",
                column: "BankStatementId",
                principalTable: "BankStatements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankImportAttempts_BankImportProfiles_ProfileId",
                table: "BankImportAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_BankImportAttempts_BankStatements_BankStatementId",
                table: "BankImportAttempts");

            migrationBuilder.DropTable(
                name: "BankImportProfiles");

            migrationBuilder.DropTable(
                name: "BankImportRows");

            migrationBuilder.DropIndex(
                name: "IX_BankStatements_BankAccountId_FileHash",
                table: "BankStatements");

            migrationBuilder.DropIndex(
                name: "IX_BankStatementLines_DeduplicationHash",
                table: "BankStatementLines");

            migrationBuilder.DropIndex(
                name: "IX_BankImportAttempts_BankAccountId_FileHash",
                table: "BankImportAttempts");

            migrationBuilder.DropIndex(
                name: "IX_BankImportAttempts_BankStatementId",
                table: "BankImportAttempts");

            migrationBuilder.DropIndex(
                name: "IX_BankImportAttempts_ProfileId",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "BankStatements");

            migrationBuilder.DropColumn(
                name: "ImportAttemptId",
                table: "BankStatements");

            migrationBuilder.DropColumn(
                name: "BankBalance",
                table: "BankStatementLines");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "BankStatementLines");

            migrationBuilder.DropColumn(
                name: "DeduplicationHash",
                table: "BankStatementLines");

            migrationBuilder.DropColumn(
                name: "SourceRowNumber",
                table: "BankStatementLines");

            migrationBuilder.DropColumn(
                name: "AdapterCode",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "BankStatementId",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "DuplicateRecords",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "IncompleteRecords",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "RejectedRecords",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "RolledBackAt",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "RolledBackBy",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "TotalRecords",
                table: "BankImportAttempts");

            migrationBuilder.DropColumn(
                name: "ValidRecords",
                table: "BankImportAttempts");

            migrationBuilder.CreateIndex(
                name: "IX_BankStatements_BankAccountId",
                table: "BankStatements",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankImportAttempts_BankAccountId",
                table: "BankImportAttempts",
                column: "BankAccountId");
        }
    }
}
