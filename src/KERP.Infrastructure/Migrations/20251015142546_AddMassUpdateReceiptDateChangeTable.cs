using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMassUpdateReceiptDateChangeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "f241");

            migrationBuilder.CreateTable(
                name: "MassUpdate_PurchaseOrder_ReceiptDate",
                schema: "f241",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderNumber = table.Column<string>(type: "nchar(9)", fixedLength: true, maxLength: 9, nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateType = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryId = table.Column<int>(type: "int", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsGenerated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MassUpdate_PurchaseOrder_ReceiptDate", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MassUpdate_PurchaseOrder_ReceiptDate",
                schema: "f241");
        }
    }
}
