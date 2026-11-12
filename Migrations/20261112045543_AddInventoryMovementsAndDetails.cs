using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Flow.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryMovementsAndDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    IdInventoryMovement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdMovementType = table.Column<int>(type: "int", nullable: false),
                    IdOriginWarehouse = table.Column<int>(type: "int", nullable: true),
                    IdDestinationWarehouse = table.Column<int>(type: "int", nullable: true),
                    Observation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.IdInventoryMovement);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_MovementTypes_IdMovementType",
                        column: x => x.IdMovementType,
                        principalTable: "MovementTypes",
                        principalColumn: "IdMovementType",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Warehouses_IdDestinationWarehouse",
                        column: x => x.IdDestinationWarehouse,
                        principalTable: "Warehouses",
                        principalColumn: "IdWarehouse");
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Warehouses_IdOriginWarehouse",
                        column: x => x.IdOriginWarehouse,
                        principalTable: "Warehouses",
                        principalColumn: "IdWarehouse");
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovementDetails",
                columns: table => new
                {
                    IdInventoryMovementDetail = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdInventoryMovement = table.Column<int>(type: "int", nullable: false),
                    IdProduct = table.Column<int>(type: "int", nullable: false),
                    IdLot = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovementDetails", x => x.IdInventoryMovementDetail);
                    table.ForeignKey(
                        name: "FK_InventoryMovementDetails_InventoryMovements_IdInventoryMovement",
                        column: x => x.IdInventoryMovement,
                        principalTable: "InventoryMovements",
                        principalColumn: "IdInventoryMovement",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryMovementDetails_Lots_IdLot",
                        column: x => x.IdLot,
                        principalTable: "Lots",
                        principalColumn: "IdLot");
                    table.ForeignKey(
                        name: "FK_InventoryMovementDetails_Products_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "Products",
                        principalColumn: "IdProduct",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovementDetails_IdInventoryMovement",
                table: "InventoryMovementDetails",
                column: "IdInventoryMovement");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovementDetails_IdLot",
                table: "InventoryMovementDetails",
                column: "IdLot");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovementDetails_IdProduct",
                table: "InventoryMovementDetails",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_IdDestinationWarehouse",
                table: "InventoryMovements",
                column: "IdDestinationWarehouse");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_IdMovementType",
                table: "InventoryMovements",
                column: "IdMovementType");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_IdOriginWarehouse",
                table: "InventoryMovements",
                column: "IdOriginWarehouse");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryMovementDetails");

            migrationBuilder.DropTable(
                name: "InventoryMovements");
        }
    }
}
