using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Inventory_Flow.Migrations
{
    /// <inheritdoc />
    public partial class AddInternalRequisitionsWithCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequisitionStatuses",
                columns: table => new
                {
                    IdRequisitionStatus = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BadgeColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsFinal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisitionStatuses", x => x.IdRequisitionStatus);
                });

            migrationBuilder.CreateTable(
                name: "RequisitionTypes",
                columns: table => new
                {
                    IdRequisitionType = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PriorityLevel = table.Column<int>(type: "int", nullable: false),
                    MaxResponseHours = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisitionTypes", x => x.IdRequisitionType);
                });

            migrationBuilder.CreateTable(
                name: "InternalRequisitions",
                columns: table => new
                {
                    IdInternalRequisition = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequisitionCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequisitionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdRequestingWarehouse = table.Column<int>(type: "int", nullable: false),
                    IdFulfillingWarehouse = table.Column<int>(type: "int", nullable: false),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    IdRequisitionType = table.Column<int>(type: "int", nullable: false),
                    IdRequisitionStatus = table.Column<int>(type: "int", nullable: false),
                    Observation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReviewedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdReviewedByUser = table.Column<int>(type: "int", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalRequisitions", x => x.IdInternalRequisition);
                    table.ForeignKey(
                        name: "FK_InternalRequisitions_RequisitionStatuses_IdRequisitionStatus",
                        column: x => x.IdRequisitionStatus,
                        principalTable: "RequisitionStatuses",
                        principalColumn: "IdRequisitionStatus",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalRequisitions_RequisitionTypes_IdRequisitionType",
                        column: x => x.IdRequisitionType,
                        principalTable: "RequisitionTypes",
                        principalColumn: "IdRequisitionType",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalRequisitions_Users_IdReviewedByUser",
                        column: x => x.IdReviewedByUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalRequisitions_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalRequisitions_Warehouses_IdFulfillingWarehouse",
                        column: x => x.IdFulfillingWarehouse,
                        principalTable: "Warehouses",
                        principalColumn: "IdWarehouse",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternalRequisitions_Warehouses_IdRequestingWarehouse",
                        column: x => x.IdRequestingWarehouse,
                        principalTable: "Warehouses",
                        principalColumn: "IdWarehouse",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InternalRequisitionDetails",
                columns: table => new
                {
                    IdInternalRequisitionDetail = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdInternalRequisition = table.Column<int>(type: "int", nullable: false),
                    IdProduct = table.Column<int>(type: "int", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "int", nullable: false),
                    DispatchedQuantity = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalRequisitionDetails", x => x.IdInternalRequisitionDetail);
                    table.ForeignKey(
                        name: "FK_InternalRequisitionDetails_InternalRequisitions_IdInternalRequisition",
                        column: x => x.IdInternalRequisition,
                        principalTable: "InternalRequisitions",
                        principalColumn: "IdInternalRequisition",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InternalRequisitionDetails_Products_IdProduct",
                        column: x => x.IdProduct,
                        principalTable: "Products",
                        principalColumn: "IdProduct",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RequisitionStatuses",
                columns: new[] { "IdRequisitionStatus", "BadgeColor", "Code", "Description", "IsFinal", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "warning", "SOLICITADO", "Enviada por la sucursal, pendiente de revisión en Carapungo.", false, "Solicitado", 1 },
                    { 2, "info", "EN_REVISION", "Carapungo está verificando disponibilidad de stock.", false, "En revisión", 2 },
                    { 3, "primary", "APROBADO_PARCIAL", "Se atiende solo una parte; el faltante pasa a compras.", false, "Aprobado parcial", 3 },
                    { 4, "primary", "DESPACHADO", "La mercadería salió de Carapungo hacia la sucursal.", false, "Despachado", 4 },
                    { 5, "success", "COMPLETADO", "La sucursal confirmó la recepción.", true, "Completado", 5 },
                    { 6, "danger", "RECHAZADO", "Carapungo rechazó la solicitud.", true, "Rechazado", 6 },
                    { 7, "secondary", "CANCELADO", "La solicitud fue anulada.", true, "Cancelado", 7 }
                });

            migrationBuilder.InsertData(
                table: "RequisitionTypes",
                columns: new[] { "IdRequisitionType", "Code", "Description", "IsActive", "MaxResponseHours", "Name", "PriorityLevel" },
                values: new object[,]
                {
                    { 1, "URGENCIA_QUIRURGICA", "Insumos críticos para cirugías o emergencias médicas.", true, 4, "Urgencia quirúrgica / emergencia", 1 },
                    { 2, "PACIENTE_ESPECIFICO", "Medicamento bajo pedido para un tratamiento puntual.", true, 24, "Paciente específico", 2 },
                    { 3, "CAMPANIA_SANITARIA", "Stock adicional para jornadas planificadas.", true, 48, "Campaña sanitaria / vacunación", 3 },
                    { 4, "REPOSICION_ORDINARIA", "Reabastecimiento rutinario semanal de la sucursal.", true, 72, "Reposición ordinaria", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InternalRequisitionDetails_IdInternalRequisition",
                table: "InternalRequisitionDetails",
                column: "IdInternalRequisition");

            migrationBuilder.CreateIndex(
                name: "IX_InternalRequisitionDetails_IdProduct",
                table: "InternalRequisitionDetails",
                column: "IdProduct");

            migrationBuilder.CreateIndex(
                name: "IX_InternalRequisitions_IdFulfillingWarehouse",
                table: "InternalRequisitions",
                column: "IdFulfillingWarehouse");

            migrationBuilder.CreateIndex(
                name: "IX_InternalRequisitions_IdRequestingWarehouse",
                table: "InternalRequisitions",
                column: "IdRequestingWarehouse");

            migrationBuilder.CreateIndex(
                name: "IX_InternalRequisitions_IdRequisitionStatus",
                table: "InternalRequisitions",
                column: "IdRequisitionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_InternalRequisitions_IdRequisitionType",
                table: "InternalRequisitions",
                column: "IdRequisitionType");

            migrationBuilder.CreateIndex(
                name: "IX_InternalRequisitions_IdReviewedByUser",
                table: "InternalRequisitions",
                column: "IdReviewedByUser");

            migrationBuilder.CreateIndex(
                name: "IX_InternalRequisitions_IdUser",
                table: "InternalRequisitions",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_InternalRequisitions_RequisitionCode",
                table: "InternalRequisitions",
                column: "RequisitionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequisitionStatuses_Code",
                table: "RequisitionStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequisitionTypes_Code",
                table: "RequisitionTypes",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InternalRequisitionDetails");

            migrationBuilder.DropTable(
                name: "InternalRequisitions");

            migrationBuilder.DropTable(
                name: "RequisitionStatuses");

            migrationBuilder.DropTable(
                name: "RequisitionTypes");
        }
    }
}
