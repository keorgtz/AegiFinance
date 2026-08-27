using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase13AccountingGovernance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccountingPeriods_StartDate_EndDate",
                table: "AccountingPeriods");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CloseChecklistJson",
                table: "AccountingPeriods",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CloseVerificationCode",
                table: "AccountingPeriods",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GovernanceVersion",
                table: "AccountingPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AccountingPeriods",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReopenReason",
                table: "AccountingPeriods",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReopenedAt",
                table: "AccountingPeriods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReopenedBy",
                table: "AccountingPeriods",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE auditLog SET [OrganizationId] = users.[OrganizationId]
                FROM [dbo].[AuditLogs] auditLog
                INNER JOIN [dbo].[Users] users ON users.[Id] = auditLog.[UserId]
                WHERE auditLog.[OrganizationId] IS NULL;

                UPDATE auditLog SET [OrganizationId] = users.[OrganizationId]
                FROM [dbo].[AuditLogs] auditLog
                INNER JOIN [dbo].[UserSessions] sessions ON sessions.[Id] = TRY_CONVERT(uniqueidentifier, auditLog.[EntityId])
                INNER JOIN [dbo].[Users] users ON users.[Id] = sessions.[UserId]
                WHERE auditLog.[OrganizationId] IS NULL AND auditLog.[EntityType] = N'UserSession';

                IF NOT EXISTS (SELECT 1 FROM [dbo].[Organizations])
                    THROW 51013, 'Phase 13 requires at least one organization before migrating accounting periods.', 1;

                IF EXISTS
                (
                    SELECT entry.[Id]
                    FROM [dbo].[JournalEntries] entry
                    INNER JOIN [dbo].[JournalLines] line ON line.[JournalEntryId] = entry.[Id]
                    INNER JOIN [dbo].[BankAccounts] bank ON bank.[Id] = line.[BankAccountId]
                    GROUP BY entry.[Id]
                    HAVING COUNT(DISTINCT bank.[OrganizationId]) > 1
                ) THROW 51014, 'A journal entry references bank accounts from different organizations.', 1;

                CREATE TABLE #EntryOrganization
                (
                    [EntryId] uniqueidentifier NOT NULL PRIMARY KEY,
                    [OldPeriodId] uniqueidentifier NOT NULL,
                    [OrganizationId] uniqueidentifier NULL
                );

                INSERT INTO #EntryOrganization ([EntryId], [OldPeriodId], [OrganizationId])
                SELECT entry.[Id], entry.[AccountingPeriodId],
                       COALESCE(client.[OrganizationId], bankScope.[OrganizationId], postedUser.[OrganizationId], createdUser.[OrganizationId])
                FROM [dbo].[JournalEntries] entry
                LEFT JOIN [dbo].[Clients] client ON client.[Id] = entry.[ClientId]
                LEFT JOIN [dbo].[Users] postedUser ON postedUser.[Id] = entry.[PostedBy]
                LEFT JOIN [dbo].[Users] createdUser ON createdUser.[Id] = entry.[CreatedBy]
                OUTER APPLY
                (
                    SELECT MIN(bank.[OrganizationId]) AS [OrganizationId], COUNT(DISTINCT bank.[OrganizationId]) AS [OrganizationCount]
                    FROM [dbo].[JournalLines] line
                    INNER JOIN [dbo].[BankAccounts] bank ON bank.[Id] = line.[BankAccountId]
                    WHERE line.[JournalEntryId] = entry.[Id]
                ) bankScope;

                IF EXISTS
                (
                    SELECT 1
                    FROM [dbo].[JournalEntries] entry
                    LEFT JOIN [dbo].[Clients] client ON client.[Id] = entry.[ClientId]
                    LEFT JOIN [dbo].[Users] postedUser ON postedUser.[Id] = entry.[PostedBy]
                    LEFT JOIN [dbo].[Users] createdUser ON createdUser.[Id] = entry.[CreatedBy]
                    OUTER APPLY
                    (
                        SELECT MIN(bank.[OrganizationId]) AS [OrganizationId]
                        FROM [dbo].[JournalLines] line
                        INNER JOIN [dbo].[BankAccounts] bank ON bank.[Id] = line.[BankAccountId]
                        WHERE line.[JournalEntryId] = entry.[Id]
                    ) bankScope
                    CROSS APPLY
                    (
                        SELECT COUNT(DISTINCT candidate.[OrganizationId]) AS [OrganizationCount]
                        FROM (VALUES (client.[OrganizationId]), (bankScope.[OrganizationId]),
                                     (postedUser.[OrganizationId]), (createdUser.[OrganizationId])) candidate([OrganizationId])
                        WHERE candidate.[OrganizationId] IS NOT NULL
                    ) scope
                    WHERE scope.[OrganizationCount] > 1
                ) THROW 51014, 'A journal entry has conflicting organization evidence.', 1;

                IF (SELECT COUNT(*) FROM [dbo].[Organizations]) = 1
                    UPDATE #EntryOrganization SET [OrganizationId] = (SELECT MIN([Id]) FROM [dbo].[Organizations]) WHERE [OrganizationId] IS NULL;

                IF EXISTS (SELECT 1 FROM #EntryOrganization WHERE [OrganizationId] IS NULL)
                    THROW 51015, 'Some historical journal entries cannot be assigned safely to an organization.', 1;

                CREATE TABLE #PeriodOrganization
                (
                    [OldPeriodId] uniqueidentifier NOT NULL,
                    [OrganizationId] uniqueidentifier NOT NULL,
                    [NewPeriodId] uniqueidentifier NOT NULL,
                    PRIMARY KEY ([OldPeriodId], [OrganizationId])
                );

                INSERT INTO #PeriodOrganization ([OldPeriodId], [OrganizationId], [NewPeriodId])
                SELECT DISTINCT [OldPeriodId], [OrganizationId], NEWID() FROM #EntryOrganization;

                INSERT INTO #PeriodOrganization ([OldPeriodId], [OrganizationId], [NewPeriodId])
                SELECT period.[Id], organization.[Id], NEWID()
                FROM [dbo].[AccountingPeriods] period
                CROSS JOIN [dbo].[Organizations] organization
                WHERE NOT EXISTS (SELECT 1 FROM #EntryOrganization entryScope WHERE entryScope.[OldPeriodId] = period.[Id]);

                INSERT INTO [dbo].[AccountingPeriods]
                ([Id], [OrganizationId], [Name], [StartDate], [EndDate], [Status], [ClosedAt], [ClosedBy],
                 [CloseChecklistJson], [CloseVerificationCode], [GovernanceVersion], [ReopenReason], [ReopenedAt], [ReopenedBy],
                 [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedAt], [DeletedBy])
                SELECT map.[NewPeriodId], map.[OrganizationId], period.[Name], period.[StartDate], period.[EndDate], period.[Status], period.[ClosedAt], period.[ClosedBy],
                       NULL, NULL, 0, NULL, NULL, NULL,
                       period.[CreatedAt], period.[CreatedBy], period.[UpdatedAt], period.[UpdatedBy], period.[IsDeleted], period.[DeletedAt], period.[DeletedBy]
                FROM #PeriodOrganization map
                INNER JOIN [dbo].[AccountingPeriods] period ON period.[Id] = map.[OldPeriodId];

                UPDATE entry SET [AccountingPeriodId] = map.[NewPeriodId]
                FROM [dbo].[JournalEntries] entry
                INNER JOIN #EntryOrganization scope ON scope.[EntryId] = entry.[Id]
                INNER JOIN #PeriodOrganization map ON map.[OldPeriodId] = scope.[OldPeriodId] AND map.[OrganizationId] = scope.[OrganizationId];

                DELETE period FROM [dbo].[AccountingPeriods] period
                WHERE EXISTS (SELECT 1 FROM #PeriodOrganization map WHERE map.[OldPeriodId] = period.[Id]);
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizationId",
                table: "AccountingPeriods",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "AccountingPeriodReopenRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewComment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_AccountingPeriodReopenRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodReopenRequests_AccountingPeriods_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalTable: "AccountingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccountingPeriodReopenRequests_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OrganizationId_Timestamp",
                table: "AuditLogs",
                columns: new[] { "OrganizationId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_OrganizationId_StartDate_EndDate",
                table: "AccountingPeriods",
                columns: new[] { "OrganizationId", "StartDate", "EndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodReopenRequests_AccountingPeriodId_Status",
                table: "AccountingPeriodReopenRequests",
                columns: new[] { "AccountingPeriodId", "Status" },
                unique: true,
                filter: "[Status] = 'Pending' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriodReopenRequests_OrganizationId_RequestedAt",
                table: "AccountingPeriodReopenRequests",
                columns: new[] { "OrganizationId", "RequestedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_AccountingPeriods_Organizations_OrganizationId",
                table: "AccountingPeriods",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                CREATE FUNCTION [dbo].[fn_AegiFinanceJournalPeriodAccess](@AccountingPeriodId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL
                   OR EXISTS
                   (
                       SELECT 1 FROM [dbo].[AccountingPeriods] period
                       WHERE period.[Id] = @AccountingPeriodId
                         AND period.[OrganizationId] = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'AegiFinance.OrganizationId'))
                   );
                """);

            migrationBuilder.Sql("""
                CREATE FUNCTION [dbo].[fn_AegiFinanceJournalLinePeriodAccess](@JournalEntryId uniqueidentifier)
                RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT 1 AS [Allowed]
                WHERE SESSION_CONTEXT(N'AegiFinance.OrganizationId') IS NULL
                   OR EXISTS
                   (
                       SELECT 1 FROM [dbo].[JournalEntries] entry
                       INNER JOIN [dbo].[AccountingPeriods] period ON period.[Id] = entry.[AccountingPeriodId]
                       WHERE entry.[Id] = @JournalEntryId
                         AND period.[OrganizationId] = TRY_CONVERT(uniqueidentifier, SESSION_CONTEXT(N'AegiFinance.OrganizationId'))
                   );
                """);

            migrationBuilder.Sql("""
                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[AccountingPeriods],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[AccountingPeriods] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[AccountingPeriods] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[AccountingPeriodReopenRequests],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[AccountingPeriodReopenRequests] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[AccountingPeriodReopenRequests] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[AuditLogs],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[AuditLogs] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceOrganizationAccess]([OrganizationId]) ON [dbo].[AuditLogs] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceJournalPeriodAccess]([AccountingPeriodId]) ON [dbo].[JournalEntries],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceJournalPeriodAccess]([AccountingPeriodId]) ON [dbo].[JournalEntries] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceJournalPeriodAccess]([AccountingPeriodId]) ON [dbo].[JournalEntries] AFTER UPDATE,
                ADD FILTER PREDICATE [dbo].[fn_AegiFinanceJournalLinePeriodAccess]([JournalEntryId]) ON [dbo].[JournalLines],
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceJournalLinePeriodAccess]([JournalEntryId]) ON [dbo].[JournalLines] AFTER INSERT,
                ADD BLOCK PREDICATE [dbo].[fn_AegiFinanceJournalLinePeriodAccess]([JournalEntryId]) ON [dbo].[JournalLines] AFTER UPDATE;
                """);

            migrationBuilder.Sql("""
                CREATE OR ALTER TRIGGER [dbo].[TR_JournalEntries_RequireOpenPeriod]
                ON [dbo].[JournalEntries]
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS
                    (
                        SELECT 1 FROM inserted entry
                        INNER JOIN [dbo].[AccountingPeriods] period ON period.[Id] = entry.[AccountingPeriodId]
                        WHERE entry.[Status] IN (N'Draft', N'Posted') AND period.[Status] <> N'Open'
                    ) THROW 51016, 'Journal entries cannot be created or posted in a closed accounting period.', 1;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS [dbo].[TR_JournalEntries_RequireOpenPeriod];");

            migrationBuilder.Sql("""
                ALTER SECURITY POLICY [dbo].[AegiFinanceTenantSecurityPolicy]
                DROP FILTER PREDICATE ON [dbo].[AccountingPeriods],
                DROP BLOCK PREDICATE ON [dbo].[AccountingPeriods],
                DROP FILTER PREDICATE ON [dbo].[AccountingPeriodReopenRequests],
                DROP BLOCK PREDICATE ON [dbo].[AccountingPeriodReopenRequests],
                DROP FILTER PREDICATE ON [dbo].[AuditLogs],
                DROP BLOCK PREDICATE ON [dbo].[AuditLogs],
                DROP FILTER PREDICATE ON [dbo].[JournalEntries],
                DROP BLOCK PREDICATE ON [dbo].[JournalEntries],
                DROP FILTER PREDICATE ON [dbo].[JournalLines],
                DROP BLOCK PREDICATE ON [dbo].[JournalLines];
                """);
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceJournalLinePeriodAccess]; DROP FUNCTION IF EXISTS [dbo].[fn_AegiFinanceJournalPeriodAccess];");
            migrationBuilder.DropForeignKey(
                name: "FK_AccountingPeriods_Organizations_OrganizationId",
                table: "AccountingPeriods");

            migrationBuilder.DropTable(
                name: "AccountingPeriodReopenRequests");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_OrganizationId_Timestamp",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AccountingPeriods_OrganizationId_StartDate_EndDate",
                table: "AccountingPeriods");

            migrationBuilder.Sql("""
                ;WITH keepers AS
                (
                    SELECT [StartDate], [EndDate], MIN([Id]) AS [KeeperId]
                    FROM [dbo].[AccountingPeriods]
                    GROUP BY [StartDate], [EndDate]
                )
                UPDATE entry SET [AccountingPeriodId] = keeper.[KeeperId]
                FROM [dbo].[JournalEntries] entry
                INNER JOIN [dbo].[AccountingPeriods] period ON period.[Id] = entry.[AccountingPeriodId]
                INNER JOIN keepers keeper ON keeper.[StartDate] = period.[StartDate] AND keeper.[EndDate] = period.[EndDate];

                ;WITH ranked AS
                (
                    SELECT [Id], ROW_NUMBER() OVER (PARTITION BY [StartDate], [EndDate] ORDER BY [Id]) AS [Position]
                    FROM [dbo].[AccountingPeriods]
                )
                DELETE period FROM [dbo].[AccountingPeriods] period
                INNER JOIN ranked duplicatePeriod ON duplicatePeriod.[Id] = period.[Id]
                WHERE duplicatePeriod.[Position] > 1;
                """);

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CloseChecklistJson",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "CloseVerificationCode",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "GovernanceVersion",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "ReopenReason",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "ReopenedAt",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "ReopenedBy",
                table: "AccountingPeriods");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_StartDate_EndDate",
                table: "AccountingPeriods",
                columns: new[] { "StartDate", "EndDate" },
                unique: true);
        }
    }
}
