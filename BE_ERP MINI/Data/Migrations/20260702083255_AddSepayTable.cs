using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE_ERP_MINI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSepayTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "SepayTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SepayTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    Gateway = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TransferType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TransferAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Accumulated = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferenceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MatchedInvoiceId = table.Column<int>(type: "int", nullable: true),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SepayTransactions", x => x.Id);
                });



            migrationBuilder.CreateIndex(
                name: "IX_SepayTransactions_Code",
                table: "SepayTransactions",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_SepayTransactions_MatchedInvoiceId",
                table: "SepayTransactions",
                column: "MatchedInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SepayTransactions_ReceivedAt",
                table: "SepayTransactions",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SepayTransactions_SepayTransactionId",
                table: "SepayTransactions",
                column: "SepayTransactionId",
                unique: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SepayTransactions");
        }
    }
}
