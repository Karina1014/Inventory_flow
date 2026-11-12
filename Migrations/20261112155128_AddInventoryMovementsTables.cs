using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Flow.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryMovementsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovementDetails_Products_IdProduct",
                table: "InventoryMovementDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovements_MovementTypes_IdMovementType",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "InventoryMovements");

            migrationBuilder.AddColumn<int>(
                name: "IdUser",
                table: "InventoryMovements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferenceId",
                table: "InventoryMovements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceSource",
                table: "InventoryMovements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "InventoryMovementDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_IdUser",
                table: "InventoryMovements",
                column: "IdUser");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovementDetails_Products_IdProduct",
                table: "InventoryMovementDetails",
                column: "IdProduct",
                principalTable: "Products",
                principalColumn: "IdProduct",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovements_MovementTypes_IdMovementType",
                table: "InventoryMovements",
                column: "IdMovementType",
                principalTable: "MovementTypes",
                principalColumn: "IdMovementType",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovements_Users_IdUser",
                table: "InventoryMovements",
                column: "IdUser",
                principalTable: "Users",
                principalColumn: "IdUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovementDetails_Products_IdProduct",
                table: "InventoryMovementDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovements_MovementTypes_IdMovementType",
                table: "InventoryMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryMovements_Users_IdUser",
                table: "InventoryMovements");

            migrationBuilder.DropIndex(
                name: "IX_InventoryMovements_IdUser",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "IdUser",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "ReferenceSource",
                table: "InventoryMovements");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "InventoryMovementDetails");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "InventoryMovements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovementDetails_Products_IdProduct",
                table: "InventoryMovementDetails",
                column: "IdProduct",
                principalTable: "Products",
                principalColumn: "IdProduct",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryMovements_MovementTypes_IdMovementType",
                table: "InventoryMovements",
                column: "IdMovementType",
                principalTable: "MovementTypes",
                principalColumn: "IdMovementType",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
