using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase7VersionedPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Services_Code",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCategories_Name",
                table: "ServiceCategories");

            migrationBuilder.AddColumn<string>(
                name: "ContractTerms",
                table: "Subscriptions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomIntervalDays",
                table: "Subscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "Subscriptions",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProrationPolicy",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceVersionId",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxPercent",
                table: "Subscriptions",
                type: "decimal(9,4)",
                precision: 9,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Services",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "ServiceCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "BaseAmount",
                table: "BillingItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "BillingItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProrationFactor",
                table: "BillingItems",
                type: "decimal(12,8)",
                precision: 12,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionTermsVersionId",
                table: "BillingItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "BillingItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ServiceVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BillingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DefaultDiscountPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    DefaultTaxPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    CustomIntervalDays = table.Column<int>(type: "int", nullable: true),
                    ProrationPolicy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Terms = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ServiceVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceVersions_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionRenewals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PreviousEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreviousNextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NewNextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RenewedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_SubscriptionRenewals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionRenewals_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceVersionConcepts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ServiceVersionConcepts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceVersionConcepts_ServiceVersions_ServiceVersionId",
                        column: x => x.ServiceVersionId,
                        principalTable: "ServiceVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionTermsVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    ServiceVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BillingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    TaxPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    BillingDay = table.Column<int>(type: "int", nullable: false),
                    CustomIntervalDays = table.Column<int>(type: "int", nullable: true),
                    ProrationPolicy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Terms = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_SubscriptionTermsVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionTermsVersions_ServiceVersions_ServiceVersionId",
                        column: x => x.ServiceVersionId,
                        principalTable: "ServiceVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionTermsVersions_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                DECLARE @OrganizationId uniqueidentifier = (SELECT TOP (1) [Id] FROM [Organizations] ORDER BY [CreatedAt]);
                IF @OrganizationId IS NULL THROW 51000, 'Phase 7 requires an organization before migrating plans.', 1;

                UPDATE [Services] SET [OrganizationId] = @OrganizationId WHERE [OrganizationId] = '00000000-0000-0000-0000-000000000000';
                UPDATE [ServiceCategories] SET [OrganizationId] = @OrganizationId WHERE [OrganizationId] = '00000000-0000-0000-0000-000000000000';
                UPDATE [Subscriptions] SET [ProrationPolicy] = 'None' WHERE [ProrationPolicy] = '';
                UPDATE [BillingItems] SET [BaseAmount] = [Amount], [ProrationFactor] = 1 WHERE [ProrationFactor] = 0;

                INSERT INTO [ServiceVersions]
                    ([Id], [ServiceId], [VersionNumber], [Name], [Description], [BillingType], [BasePrice], [Currency],
                     [DefaultDiscountPercent], [DefaultTaxPercent], [CustomIntervalDays], [ProrationPolicy], [EffectiveFrom],
                     [EffectiveTo], [Terms], [IsPublished], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedAt], [DeletedBy])
                SELECT NEWID(), s.[Id], 1, s.[Name], s.[Description], s.[BillingType], s.[DefaultPrice], s.[Currency],
                       0, 0, NULL, 'None', s.[CreatedAt], NULL, NULL, NULL, 1, SYSUTCDATETIME(), NULL, SYSUTCDATETIME(), NULL, 0, NULL, NULL
                FROM [Services] s
                WHERE NOT EXISTS (SELECT 1 FROM [ServiceVersions] v WHERE v.[ServiceId] = s.[Id]);

                INSERT INTO [ServiceVersionConcepts]
                    ([Id], [ServiceVersionId], [Code], [Name], [Description], [Quantity], [UnitPrice], [TaxPercent], [SortOrder],
                     [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedAt], [DeletedBy])
                SELECT NEWID(), v.[Id], 'BASE', v.[Name], NULL, 1, v.[BasePrice], v.[DefaultTaxPercent], 0,
                       SYSUTCDATETIME(), NULL, SYSUTCDATETIME(), NULL, 0, NULL, NULL
                FROM [ServiceVersions] v
                WHERE v.[VersionNumber] = 1
                  AND NOT EXISTS (SELECT 1 FROM [ServiceVersionConcepts] c WHERE c.[ServiceVersionId] = v.[Id]);

                UPDATE sub SET [ServiceVersionId] = v.[Id]
                FROM [Subscriptions] sub
                INNER JOIN [ServiceVersions] v ON v.[ServiceId] = sub.[ServiceId] AND v.[VersionNumber] = 1
                WHERE sub.[ServiceVersionId] IS NULL;

                INSERT INTO [SubscriptionTermsVersions]
                    ([Id], [SubscriptionId], [VersionNumber], [ServiceVersionId], [EffectiveFrom], [EffectiveTo], [BillingType],
                     [BasePrice], [Currency], [DiscountPercent], [TaxPercent], [BillingDay], [CustomIntervalDays], [ProrationPolicy],
                     [Terms], [Reason], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedAt], [DeletedBy])
                SELECT NEWID(), sub.[Id], 1, sub.[ServiceVersionId], sub.[StartDate], NULL, sub.[BillingType], sub.[Price], sub.[Currency],
                       0, 0, sub.[BillingDay], NULL, 'None', NULL, 'Migración de condiciones existentes',
                       SYSUTCDATETIME(), NULL, SYSUTCDATETIME(), NULL, 0, NULL, NULL
                FROM [Subscriptions] sub
                WHERE NOT EXISTS (SELECT 1 FROM [SubscriptionTermsVersions] t WHERE t.[SubscriptionId] = sub.[Id]);

                UPDATE item SET [SubscriptionTermsVersionId] = terms.[Id]
                FROM [BillingItems] item
                INNER JOIN [SubscriptionTermsVersions] terms ON terms.[SubscriptionId] = item.[SubscriptionId] AND terms.[VersionNumber] = 1
                WHERE item.[SubscriptionTermsVersionId] IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_ServiceVersionId",
                table: "Subscriptions",
                column: "ServiceVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_OrganizationId_Code",
                table: "Services",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_OrganizationId_Name",
                table: "ServiceCategories",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingItems_SubscriptionTermsVersionId",
                table: "BillingItems",
                column: "SubscriptionTermsVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceVersionConcepts_ServiceVersionId_Code",
                table: "ServiceVersionConcepts",
                columns: new[] { "ServiceVersionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceVersions_ServiceId_EffectiveFrom",
                table: "ServiceVersions",
                columns: new[] { "ServiceId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceVersions_ServiceId_VersionNumber",
                table: "ServiceVersions",
                columns: new[] { "ServiceId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionRenewals_SubscriptionId_IdempotencyKey",
                table: "SubscriptionRenewals",
                columns: new[] { "SubscriptionId", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTermsVersions_ServiceVersionId",
                table: "SubscriptionTermsVersions",
                column: "ServiceVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTermsVersions_SubscriptionId_EffectiveFrom",
                table: "SubscriptionTermsVersions",
                columns: new[] { "SubscriptionId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTermsVersions_SubscriptionId_VersionNumber",
                table: "SubscriptionTermsVersions",
                columns: new[] { "SubscriptionId", "VersionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingItems_SubscriptionTermsVersions_SubscriptionTermsVersionId",
                table: "BillingItems",
                column: "SubscriptionTermsVersionId",
                principalTable: "SubscriptionTermsVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCategories_Organizations_OrganizationId",
                table: "ServiceCategories",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Organizations_OrganizationId",
                table: "Services",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_ServiceVersions_ServiceVersionId",
                table: "Subscriptions",
                column: "ServiceVersionId",
                principalTable: "ServiceVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                CREATE FUNCTION [dbo].[fn_AegiFinanceServiceAccess](@ServiceId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL
                   OR EXISTS (SELECT 1 FROM [dbo].[Services] s WHERE s.[Id]=@ServiceId AND s.[OrganizationId]=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.OrganizationId')));
                CREATE FUNCTION [dbo].[fn_AegiFinanceServiceVersionAccess](@ServiceVersionId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL
                   OR EXISTS (SELECT 1 FROM [dbo].[ServiceVersions] v INNER JOIN [dbo].[Services] s ON s.[Id]=v.[ServiceId] WHERE v.[Id]=@ServiceVersionId AND s.[OrganizationId]=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.OrganizationId')));
                CREATE FUNCTION [dbo].[fn_AegiFinanceSubscriptionAccess](@SubscriptionId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE (SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL
                   OR EXISTS (SELECT 1 FROM [dbo].[Subscriptions] sub INNER JOIN [dbo].[Clients] c ON c.[Id]=sub.[ClientId] WHERE sub.[Id]=@SubscriptionId AND c.[OrganizationId]=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.OrganizationId'))))
                  AND (TRY_CONVERT(bit,SESSION_CONTEXT(N'AegiFinance.IsClient'))=0 OR EXISTS (SELECT 1 FROM [dbo].[Subscriptions] sub WHERE sub.[Id]=@SubscriptionId AND sub.[ClientId]=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.ClientId'))));

                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[Services],
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[ServiceCategories],
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceServiceAccess]([ServiceId]) ON [dbo].[ServiceVersions],
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceServiceVersionAccess]([ServiceVersionId]) ON [dbo].[ServiceVersionConcepts],
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceSubscriptionAccess]([SubscriptionId]) ON [dbo].[SubscriptionTermsVersions],
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceSubscriptionAccess]([SubscriptionId]) ON [dbo].[SubscriptionRenewals];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                DROP FILTER PREDICATE ON [dbo].[Services],
                DROP FILTER PREDICATE ON [dbo].[ServiceCategories],
                DROP FILTER PREDICATE ON [dbo].[ServiceVersions],
                DROP FILTER PREDICATE ON [dbo].[ServiceVersionConcepts],
                DROP FILTER PREDICATE ON [dbo].[SubscriptionTermsVersions],
                DROP FILTER PREDICATE ON [dbo].[SubscriptionRenewals];
                DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceSubscriptionAccess];
                DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceServiceVersionAccess];
                DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceServiceAccess];
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_BillingItems_SubscriptionTermsVersions_SubscriptionTermsVersionId",
                table: "BillingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCategories_Organizations_OrganizationId",
                table: "ServiceCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Organizations_OrganizationId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_ServiceVersions_ServiceVersionId",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "ServiceVersionConcepts");

            migrationBuilder.DropTable(
                name: "SubscriptionRenewals");

            migrationBuilder.DropTable(
                name: "SubscriptionTermsVersions");

            migrationBuilder.DropTable(
                name: "ServiceVersions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_ServiceVersionId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Services_OrganizationId_Code",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCategories_OrganizationId_Name",
                table: "ServiceCategories");

            migrationBuilder.DropIndex(
                name: "IX_BillingItems_SubscriptionTermsVersionId",
                table: "BillingItems");

            migrationBuilder.DropColumn(
                name: "ContractTerms",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CustomIntervalDays",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ProrationPolicy",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ServiceVersionId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TaxPercent",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "BaseAmount",
                table: "BillingItems");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "BillingItems");

            migrationBuilder.DropColumn(
                name: "ProrationFactor",
                table: "BillingItems");

            migrationBuilder.DropColumn(
                name: "SubscriptionTermsVersionId",
                table: "BillingItems");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "BillingItems");

            migrationBuilder.CreateIndex(
                name: "IX_Services_Code",
                table: "Services",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_Name",
                table: "ServiceCategories",
                column: "Name",
                unique: true);
        }
    }
}
