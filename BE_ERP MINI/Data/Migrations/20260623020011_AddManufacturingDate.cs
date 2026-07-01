using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE_ERP_MINI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturingDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ManufacturingDate",
                table: "WarehouseReceiptLines",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ManufacturingDate",
                table: "Lots",
                type: "date",
                nullable: true);

            // Bỏ qua tạo ProductCategories vì bảng đã tồn tại sẵn trong CSDL
            // migrationBuilder.CreateTable(...)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Bỏ qua drop ProductCategories
            // migrationBuilder.DropTable(...)

            migrationBuilder.DropColumn(
                name: "ManufacturingDate",
                table: "WarehouseReceiptLines");

            migrationBuilder.DropColumn(
                name: "ManufacturingDate",
                table: "Lots");
        }
    }
}
