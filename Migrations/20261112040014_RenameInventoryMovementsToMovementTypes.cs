using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Flow.Migrations
{
    /// <inheritdoc />
    public partial class RenameInventoryMovementsToMovementTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "InventoryMovements",
                newName: "MovementTypes");

            migrationBuilder.RenameColumn(
                name: "IdInventoryMovement",
                table: "MovementTypes",
                newName: "IdMovementType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryMovements",
                table: "MovementTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MovementTypes",
                table: "MovementTypes",
                column: "IdMovementType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MovementTypes",
                table: "MovementTypes");

            migrationBuilder.RenameColumn(
                name: "IdMovementType",
                table: "MovementTypes",
                newName: "IdInventoryMovement");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryMovements",
                table: "MovementTypes",
                column: "IdInventoryMovement");

            migrationBuilder.RenameTable(
                name: "MovementTypes",
                newName: "InventoryMovements");
        }
    }
}
