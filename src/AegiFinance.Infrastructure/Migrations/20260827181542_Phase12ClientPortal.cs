using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase12ClientPortal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountStatementInquiries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JournalEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StatementVerificationCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_AccountStatementInquiries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountStatementInquiries_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountStatementInquiries_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountStatementInquiries_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountStatementInquiries_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountStatementInquiries_ClientId_Status_RequestedAt",
                table: "AccountStatementInquiries",
                columns: new[] { "ClientId", "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountStatementInquiries_JournalEntryId",
                table: "AccountStatementInquiries",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountStatementInquiries_OrganizationId_IdempotencyKey",
                table: "AccountStatementInquiries",
                columns: new[] { "OrganizationId", "IdempotencyKey" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AccountStatementInquiries_SubscriptionId",
                table: "AccountStatementInquiries",
                column: "SubscriptionId");

            migrationBuilder.Sql("""
                CREATE FUNCTION [dbo].[fn_AegiFinanceAccountStatementInquiryAccess](@ClientId uniqueidentifier, @OrganizationId uniqueidentifier, @SubscriptionId uniqueidentifier, @JournalEntryId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE (SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL OR @OrganizationId=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.OrganizationId')))
                  AND (TRY_CONVERT(bit,SESSION_CONTEXT(N'AegiFinance.IsClient'))=0 OR @ClientId=TRY_CONVERT(uniqueidentifier,SESSION_CONTEXT(N'AegiFinance.ClientId')))
                  AND (@SubscriptionId IS NULL OR EXISTS (SELECT 1 FROM [dbo].[Subscriptions] s WHERE s.[Id]=@SubscriptionId AND s.[ClientId]=@ClientId))
                  AND (@JournalEntryId IS NULL OR EXISTS (SELECT 1 FROM [dbo].[JournalEntries] j WHERE j.[Id]=@JournalEntryId AND j.[ClientId]=@ClientId));
                """);

            migrationBuilder.Sql("""
                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceAccountStatementInquiryAccess]([ClientId],[OrganizationId],[SubscriptionId],[JournalEntryId]) ON [dbo].[AccountStatementInquiries],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceAccountStatementInquiryAccess]([ClientId],[OrganizationId],[SubscriptionId],[JournalEntryId]) ON [dbo].[AccountStatementInquiries] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceAccountStatementInquiryAccess]([ClientId],[OrganizationId],[SubscriptionId],[JournalEntryId]) ON [dbo].[AccountStatementInquiries] AFTER UPDATE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                DROP FILTER PREDICATE ON [dbo].[AccountStatementInquiries],
                DROP BLOCK PREDICATE ON [dbo].[AccountStatementInquiries];
                DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceAccountStatementInquiryAccess];
                """);

            migrationBuilder.DropTable(
                name: "AccountStatementInquiries");
        }
    }
}
