using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase6ClientScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_Code",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_Name",
                table: "BankAccounts");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccountManagerUserId",
                table: "Clients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommercialTerms",
                table: "Clients",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "Clients",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedBillingEmail",
                table: "Clients",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Clients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTaxId",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Clients",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermsDays",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PresentationCurrency",
                table: "Clients",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "MXN");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "BankAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ClientDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ClientDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientDocuments_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientDuplicateRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchTaxId = table.Column<bool>(type: "bit", nullable: false),
                    MatchName = table.Column<bool>(type: "bit", nullable: false),
                    MatchBillingEmail = table.Column<bool>(type: "bit", nullable: false),
                    BlockOnMatch = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ClientDuplicateRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientDuplicateRules_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [dbo].[Organizations] WHERE [Id] = '11111111-1111-1111-1111-111111111111')
                    INSERT INTO [dbo].[Organizations] ([Id],[Code],[Name],[IsActive],[CreatedAt],[UpdatedAt],[IsDeleted])
                    VALUES ('11111111-1111-1111-1111-111111111111','DEFAULT','AegiFinance',1,SYSUTCDATETIME(),SYSUTCDATETIME(),0);
                UPDATE [dbo].[Users] SET [OrganizationId]='11111111-1111-1111-1111-111111111111' WHERE [OrganizationId] IS NULL;
                UPDATE [dbo].[Clients] SET [OrganizationId]='11111111-1111-1111-1111-111111111111',
                    [NormalizedName]=UPPER(REPLACE(REPLACE(LTRIM(RTRIM([Name])),' ',''),'.','')),
                    [NormalizedTaxId]=CASE WHEN [TaxId] IS NULL THEN NULL ELSE UPPER(REPLACE(REPLACE(LTRIM(RTRIM([TaxId])),' ',''),'-','')) END,
                    [NormalizedBillingEmail]=CASE WHEN [BillingEmail] IS NULL THEN NULL ELSE UPPER(REPLACE(LTRIM(RTRIM([BillingEmail])),' ','')) END;
                UPDATE [dbo].[BankAccounts] SET [OrganizationId]='11111111-1111-1111-1111-111111111111';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientUsers_ClientId",
                table: "ClientUsers",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientUsers_UserId",
                table: "ClientUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_AccountManagerUserId",
                table: "Clients",
                column: "AccountManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OrganizationId_Code",
                table: "Clients",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OrganizationId_NormalizedName",
                table: "Clients",
                columns: new[] { "OrganizationId", "NormalizedName" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OrganizationId_NormalizedTaxId",
                table: "Clients",
                columns: new[] { "OrganizationId", "NormalizedTaxId" });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_OrganizationId_Name",
                table: "BankAccounts",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientDocuments_ClientId_CreatedAt",
                table: "ClientDocuments",
                columns: new[] { "ClientId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientDuplicateRules_OrganizationId",
                table: "ClientDuplicateRules",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Code",
                table: "Organizations",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Organizations_OrganizationId",
                table: "BankAccounts",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Organizations_OrganizationId",
                table: "Clients",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Users_AccountManagerUserId",
                table: "Clients",
                column: "AccountManagerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientUsers_Clients_ClientId",
                table: "ClientUsers",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientUsers_Users_UserId",
                table: "ClientUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                DROP SECURITY POLICY IF EXISTS [dbo].[AegiFinanceTenantSecurityPolicy];
                DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceTenantAccess];
                CREATE FUNCTION [dbo].[fn_AegiFinanceRootAccess](@ClientId uniqueidentifier, @OrganizationId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE (SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL OR @OrganizationId=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.OrganizationId')))
                  AND (TRY_CONVERT(bit,SESSION_CONTEXT(N'AegiFinance.IsClient'))=0 OR @ClientId=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.ClientId')));
                CREATE FUNCTION [dbo].[fn_AegiFinanceOrganizationAccess](@OrganizationId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL OR @OrganizationId=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.OrganizationId'));
                CREATE FUNCTION [dbo].[fn_AegiFinanceRelatedClientAccess](@ClientId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE (SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL OR EXISTS (SELECT 1 FROM [dbo].[Clients] AS c WHERE c.[Id]=@ClientId AND c.[OrganizationId]=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.OrganizationId'))))
                  AND (TRY_CONVERT(bit,SESSION_CONTEXT(N'AegiFinance.IsClient'))=0 OR @ClientId=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.ClientId')));
                CREATE FUNCTION [dbo].[fn_AegiFinanceLedgerAccess](@ClientId uniqueidentifier, @BankAccountId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE (SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL OR EXISTS (SELECT 1 FROM [dbo].[BankAccounts] AS b WHERE b.[Id]=@BankAccountId AND b.[OrganizationId]=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.OrganizationId'))))
                  AND (TRY_CONVERT(bit,SESSION_CONTEXT(N'AegiFinance.IsClient'))=0 OR @ClientId=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.ClientId')));
                CREATE SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceRootAccess]([Id],[OrganizationId]) ON [dbo].[Clients],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRootAccess]([Id],[OrganizationId]) ON [dbo].[Clients] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRootAccess]([Id],[OrganizationId]) ON [dbo].[Clients] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceRootAccess]([ClientId],[OrganizationId]) ON [dbo].[Users],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRootAccess]([ClientId],[OrganizationId]) ON [dbo].[Users] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRootAccess]([ClientId],[OrganizationId]) ON [dbo].[Users] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[BankAccounts],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[BankAccounts] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[BankAccounts] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientUsers],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientUsers] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientUsers] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientNotes],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientNotes] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientNotes] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientContacts],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientContacts] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientContacts] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientDocuments],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientDocuments] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[ClientDocuments] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[Subscriptions],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[Subscriptions] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[Subscriptions] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[BillingItems],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[BillingItems] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceRelatedClientAccess]([ClientId]) ON [dbo].[BillingItems] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceLedgerAccess]([ClientId],[BankAccountId]) ON [dbo].[LedgerEntries],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceLedgerAccess]([ClientId],[BankAccountId]) ON [dbo].[LedgerEntries] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceLedgerAccess]([ClientId],[BankAccountId]) ON [dbo].[LedgerEntries] AFTER UPDATE
                WITH (STATE=ON);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SECURITY POLICY IF EXISTS [dbo].[AegiFinanceTenantSecurityPolicy]; DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceRootAccess]; DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceOrganizationAccess]; DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceRelatedClientAccess]; DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceLedgerAccess];");
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Organizations_OrganizationId",
                table: "BankAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Organizations_OrganizationId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Users_AccountManagerUserId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientUsers_Clients_ClientId",
                table: "ClientUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientUsers_Users_UserId",
                table: "ClientUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ClientDocuments");

            migrationBuilder.DropTable(
                name: "ClientDuplicateRules");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_ClientUsers_ClientId",
                table: "ClientUsers");

            migrationBuilder.DropIndex(
                name: "IX_ClientUsers_UserId",
                table: "ClientUsers");

            migrationBuilder.DropIndex(
                name: "IX_Clients_AccountManagerUserId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_OrganizationId_Code",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_OrganizationId_NormalizedName",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_OrganizationId_NormalizedTaxId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_OrganizationId_Name",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AccountManagerUserId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CommercialTerms",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "NormalizedBillingEmail",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "NormalizedTaxId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PaymentTermsDays",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PresentationCurrency",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "BankAccounts");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Code",
                table: "Clients",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_Name",
                table: "BankAccounts",
                column: "Name",
                unique: true);
        }
    }
}
