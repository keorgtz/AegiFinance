using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLedgerAllocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LedgerAllocations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LedgerAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LedgerEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LedgerAllocations_BillingItems_BillingItemId",
                        column: x => x.BillingItemId,
                        principalTable: "BillingItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LedgerAllocations_LedgerEntries_LedgerEntryId",
                        column: x => x.LedgerEntryId,
                        principalTable: "LedgerEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAllocations_BillingItemId",
                table: "LedgerAllocations",
                column: "BillingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAllocations_LedgerEntryId_BillingItemId",
                table: "LedgerAllocations",
                columns: new[] { "LedgerEntryId", "BillingItemId" },
                unique: true);
        }
    }
}
