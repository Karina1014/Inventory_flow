using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Flow.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseAndProfileToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdWarehouse",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdWarehouse",
                table: "Users",
                column: "IdWarehouse");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Warehouses_IdWarehouse",
                table: "Users",
                column: "IdWarehouse",
                principalTable: "Warehouses",
                principalColumn: "IdWarehouse",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Warehouses_IdWarehouse",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IdWarehouse",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdWarehouse",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");
        }
    }
}
