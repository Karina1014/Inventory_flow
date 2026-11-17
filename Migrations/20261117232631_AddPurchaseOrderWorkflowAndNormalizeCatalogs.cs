using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Inventory_Flow.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderWorkflowAndNormalizeCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseOrderStatuses",
                columns: table => new
                {
                    IdPurchaseOrderStatus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BadgeColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderStatuses", x => x.IdPurchaseOrderStatus);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderTypes",
                columns: table => new
                {
                    IdPurchaseOrderType = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriorityLevel = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderTypes", x => x.IdPurchaseOrderType);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    IdPurchaseOrder = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdSupplier = table.Column<int>(type: "int", nullable: false),
                    IdDestinationWarehouse = table.Column<int>(type: "int", nullable: false),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    IdPurchaseOrderType = table.Column<int>(type: "int", nullable: false),
                    IdPurchaseOrderStatus = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinancialApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdFinancialApprovalUser = table.Column<int>(type: "int", nullable: true),
                    FinancialApprovalNote = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.IdPurchaseOrder);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_PurchaseOrderStatuses_IdPurchaseOrderStatus",
                        column: x => x.IdPurchaseOrderStatus,
                        principalTable: "PurchaseOrderStatuses",
                        principalColumn: "IdPurchaseOrderStatus",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_PurchaseOrderTypes_IdPurchaseOrderType",
                        column: x => x.IdPurchaseOrderType,
                        principalTable: "PurchaseOrderTypes",
                        principalColumn: "IdPurchaseOrderType",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Suppliers_IdSupplier",
                        column: x => x.IdSupplier,
                        principalTable: "Suppliers",
                        principalColumn: "IdSupplier",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Users_IdFinancialApprovalUser",
                        column: x => x.IdFinancialApprovalUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Warehouses_IdDestinationWarehouse",
                        column: x => x.IdDestinationWarehouse,
                        principalTable: "Warehouses",
                        principalColumn: "IdWarehouse",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDetails",
                columns: table => new
                {
                    IdPurchaseOrderDetail = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPurchaseOrder = table.Column<int>(type: "int", nullable: false),
                    IdProduct = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IdInternalRequisition = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderDetails", x => x.IdPurchaseOrderDetail);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_InternalRequisitions_IdInternalRequisition",
                        column: x => x.IdInternalRequisition,
                        principalTable: "InternalRequisitions",
                        principalColumn: "IdInternalRequisition",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_Products_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "Products",
                        principalColumn: "IdProduct",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetails_PurchaseOrders_IdPurchaseOrder",
                        column: x => x.IdPurchaseOrder,
                        principalTable: "PurchaseOrders",
                        principalColumn: "IdPurchaseOrder",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PurchaseOrderStatuses",
                columns: new[] { "IdPurchaseOrderStatus", "BadgeColor", "Code", "Description", "IsFinal", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "secondary", "BORRADOR", "En preparación por compras / bodega central.", false, "Borrador", 1 },
                    { 2, "warning", "PENDIENTE_FINANCIERO", "Enviada a dirección financiera para validación de gasto.", false, "Pendiente Aprobación Financiera", 2 },
                    { 3, "info", "APROBADO_FINANCIERO", "Presupuesto aprobado y lista para emitir a proveedor.", false, "Aprobado por Finanzas", 3 },
                    { 4, "danger", "RECHAZADO_FINANCIERO", "Rechazado o devuelto con observaciones financieras.", false, "Rechazado por Finanzas", 4 },
                    { 5, "primary", "EMITIDO_PROVEEDOR", "Orden enviada formalmente al proveedor para despacho.", false, "Emitido al Proveedor", 5 },
                    { 6, "primary", "RECIBIDO_PARCIAL", "Mercadería ingresada parcialmente en Carapungo.", false, "Recibido Parcial", 6 },
                    { 7, "success", "RECIBIDO_TOTAL", "Total de ítems recibidos e ingresados al inventario.", true, "Recibido Completo", 7 },
                    { 8, "secondary", "CANCELADO", "Orden de compra anulada formalmente.", true, "Cancelado", 8 }
                });

            migrationBuilder.InsertData(
                table: "PurchaseOrderTypes",
                columns: new[] { "IdPurchaseOrderType", "Code", "Description", "IsActive", "Name", "PriorityLevel" },
                values: new object[,]
                {
                    { 1, "DESABASTECIMIENTO_CRITICO", "Compra urgente de fármacos o insumos esenciales agotados.", true, "Desabastecimiento Crítico", 1 },
                    { 2, "REPOSICION_CENTRAL", "Reabastecimiento regular de stock de seguridad para Carapungo.", true, "Reposición Periódica CEDI", 2 },
                    { 3, "CAMPANIA_ESTACIONAL", "Volumen extraordinario para campañas masivas de vacunación.", true, "Campaña / Temporada", 3 },
                    { 4, "PEDIDO_ESPECIAL_IMPORTADO", "Insumos o equipos específicos no habituales en inventario.", true, "Bajo Pedido Especial", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_IdInternalRequisition",
                table: "PurchaseOrderDetails",
                column: "IdInternalRequisition");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_IdProduct",
                table: "PurchaseOrderDetails",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetails_IdPurchaseOrder",
                table: "PurchaseOrderDetails",
                column: "IdPurchaseOrder");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_IdDestinationWarehouse",
                table: "PurchaseOrders",
                column: "IdDestinationWarehouse");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_IdFinancialApprovalUser",
                table: "PurchaseOrders",
                column: "IdFinancialApprovalUser");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_IdPurchaseOrderStatus",
                table: "PurchaseOrders",
                column: "IdPurchaseOrderStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_IdPurchaseOrderType",
                table: "PurchaseOrders",
                column: "IdPurchaseOrderType");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_IdSupplier",
                table: "PurchaseOrders",
                column: "IdSupplier");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_IdUser",
                table: "PurchaseOrders",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_OrderNumber",
                table: "PurchaseOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderStatuses_Code",
                table: "PurchaseOrderStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderTypes_Code",
                table: "PurchaseOrderTypes",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrderDetails");

            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.DropTable(
                name: "PurchaseOrderStatuses");

            migrationBuilder.DropTable(
                name: "PurchaseOrderTypes");
        }
    }
}
