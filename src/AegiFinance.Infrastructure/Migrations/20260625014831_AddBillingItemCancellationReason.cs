using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AegiFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingItemCancellationReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "BillingItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "BillingItems");
        }
    }
}
