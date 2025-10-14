using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFactory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cmn");

            migrationBuilder.CreateTable(
                name: "Factories",
                schema: "cmn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factories", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "cmn",
                table: "Factories",
                columns: new[] { "Id", "IsActive", "Name" },
                values: new object[,]
                {
                    { 241, true, "Stargard" },
                    { 260, true, "Shanghai" },
                    { 276, true, "Ottawa" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FactoryId",
                table: "AspNetUsers",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Factories_Name",
                schema: "cmn",
                table: "Factories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Factories_FactoryId",
                table: "AspNetUsers",
                column: "FactoryId",
                principalSchema: "cmn",
                principalTable: "Factories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Factories_FactoryId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Factories",
                schema: "cmn");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_FactoryId",
                table: "AspNetUsers");
        }
    }
}
