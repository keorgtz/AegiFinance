using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase1DynamicPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_UserId_PermissionId",
                table: "UserPermissions");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "UserPermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "UserPermissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                table: "UserPermissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGranted",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "Permissions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemGenerated",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "Permissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Module",
                table: "Permissions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE [Permissions]
                SET [Kind] = 'Business',
                    [IsActive] = 1,
                    [Module] = CASE
                        WHEN [Code] LIKE '%Clients' THEN 'Clients'
                        WHEN [Code] LIKE '%Services' THEN 'Services'
                        WHEN [Code] LIKE '%Subscriptions' THEN 'Subscriptions'
                        WHEN [Code] LIKE '%Users' THEN 'Users'
                        WHEN [Code] LIKE '%Roles' THEN 'Roles'
                        WHEN [Code] LIKE '%Billing' OR [Code] LIKE '%Payments' THEN 'Billing'
                        ELSE 'System'
                    END,
                    [Action] = CASE
                        WHEN [Code] LIKE 'View%' THEN 'View'
                        WHEN [Code] LIKE 'Create%' THEN 'Create'
                        WHEN [Code] LIKE 'Update%' THEN 'Update'
                        WHEN [Code] LIKE 'Delete%' THEN 'Delete'
                        ELSE 'Manage'
                    END;
                """);

            migrationBuilder.CreateTable(
                name: "UiControlDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ControlKey = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ControlType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequiredPermissionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsSystemRequired = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_UiControlDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UiControlPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UiControlDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccessMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_UiControlPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UiControlPolicies_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UiControlPolicies_UiControlDefinitions_UiControlDefinitionId",
                        column: x => x.UiControlDefinitionId,
                        principalTable: "UiControlDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UiControlPolicies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId_PermissionId",
                table: "UserPermissions",
                columns: new[] { "UserId", "PermissionId" },
                unique: true,
                filter: "[ClientId] IS NULL AND [SubscriptionId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId_PermissionId_ClientId",
                table: "UserPermissions",
                columns: new[] { "UserId", "PermissionId", "ClientId" },
                unique: true,
                filter: "[ClientId] IS NOT NULL AND [SubscriptionId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId_PermissionId_SubscriptionId",
                table: "UserPermissions",
                columns: new[] { "UserId", "PermissionId", "SubscriptionId" },
                unique: true,
                filter: "[SubscriptionId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPermissions_SubscriptionId",
                table: "SubscriptionPermissions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UiControlDefinitions_ControlKey",
                table: "UiControlDefinitions",
                column: "ControlKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UiControlPolicies_RoleId",
                table: "UiControlPolicies",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UiControlPolicies_UiControlDefinitionId_RoleId",
                table: "UiControlPolicies",
                columns: new[] { "UiControlDefinitionId", "RoleId" },
                unique: true,
                filter: "[RoleId] IS NOT NULL AND [UserId] IS NULL AND [ClientId] IS NULL AND [SubscriptionId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UiControlPolicies_UiControlDefinitionId_RoleId_ClientId_SubscriptionId",
                table: "UiControlPolicies",
                columns: new[] { "UiControlDefinitionId", "RoleId", "ClientId", "SubscriptionId" },
                unique: true,
                filter: "[RoleId] IS NOT NULL AND [UserId] IS NULL AND ([ClientId] IS NOT NULL OR [SubscriptionId] IS NOT NULL) AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UiControlPolicies_UiControlDefinitionId_UserId",
                table: "UiControlPolicies",
                columns: new[] { "UiControlDefinitionId", "UserId" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [RoleId] IS NULL AND [ClientId] IS NULL AND [SubscriptionId] IS NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UiControlPolicies_UiControlDefinitionId_UserId_ClientId_SubscriptionId",
                table: "UiControlPolicies",
                columns: new[] { "UiControlDefinitionId", "UserId", "ClientId", "SubscriptionId" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [RoleId] IS NULL AND ([ClientId] IS NOT NULL OR [SubscriptionId] IS NOT NULL) AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UiControlPolicies_UserId",
                table: "UiControlPolicies",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPermissions_Subscriptions_SubscriptionId",
                table: "SubscriptionPermissions",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql("""
                CREATE FUNCTION [dbo].[fn_AegiFinanceTenantAccess](@ClientId uniqueidentifier)
                RETURNS TABLE
                WITH SCHEMABINDING
                AS
                RETURN SELECT 1 AS [Allowed]
                WHERE TRY_CONVERT(bit, SESSION_CONTEXT(N'AegiFinance.IsClient')) = 0
                   OR (TRY_CONVERT(bit, SESSION_CONTEXT(N'AegiFinance.IsClient')) = 1
                       AND @ClientId = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'AegiFinance.ClientId')));
                """);

            migrationBuilder.Sql("""
                CREATE SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([Id]) ON [dbo].[Clients],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([Id]) ON [dbo].[Clients] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([Id]) ON [dbo].[Clients] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[Users],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[Users] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[Users] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[ClientUsers],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[ClientUsers] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[ClientUsers] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[ClientNotes],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[ClientNotes] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[ClientNotes] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[ClientContacts],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[ClientContacts] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[ClientContacts] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[Subscriptions],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[Subscriptions] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[Subscriptions] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[BillingItems],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[BillingItems] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[BillingItems] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[LedgerEntries],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[LedgerEntries] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceTenantAccess]([ClientId]) ON [dbo].[LedgerEntries] AFTER UPDATE
                WITH (STATE = ON);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SECURITY POLICY IF EXISTS [dbo].[AegiFinanceTenantSecurityPolicy]; DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceTenantAccess];");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPermissions_Subscriptions_SubscriptionId",
                table: "SubscriptionPermissions");

            migrationBuilder.DropTable(
                name: "UiControlPolicies");

            migrationBuilder.DropTable(
                name: "UiControlDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_UserId_PermissionId",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_UserId_PermissionId_ClientId",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_UserId_PermissionId_SubscriptionId",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPermissions_SubscriptionId",
                table: "SubscriptionPermissions");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "IsGranted",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsSystemGenerated",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Module",
                table: "Permissions");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId_PermissionId",
                table: "UserPermissions",
                columns: new[] { "UserId", "PermissionId" },
                unique: true);
        }
    }
}
