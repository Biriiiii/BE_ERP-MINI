using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE_ERP_MINI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptNotesAndPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "WarehouseReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "WarehouseReceipts",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "WarehouseReceiptLines",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "WarehouseReceipts");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "WarehouseReceipts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "WarehouseReceiptLines");
        }
    }
}
