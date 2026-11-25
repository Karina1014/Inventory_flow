using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Inventory_Flow.Migrations
{
    /// <inheritdoc />
    public partial class AddN8nNotificationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "N8nNotificationTypes",
                columns: table => new
                {
                    IdN8nNotificationType = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUserCreated = table.Column<int>(type: "int", nullable: false),
                    IdUserUpdated = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_N8nNotificationTypes", x => x.IdN8nNotificationType);
                    table.ForeignKey(
                        name: "FK_N8nNotificationTypes_Users_IdUserCreated",
                        column: x => x.IdUserCreated,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_N8nNotificationTypes_Users_IdUserUpdated",
                        column: x => x.IdUserUpdated,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "N8nNotifications",
                columns: table => new
                {
                    IdN8nNotification = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdN8nNotificationType = table.Column<int>(type: "int", nullable: false),
                    IdWarehouse = table.Column<int>(type: "int", nullable: false),
                    IdUser = table.Column<int>(type: "int", nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_N8nNotifications", x => x.IdN8nNotification);
                    table.ForeignKey(
                        name: "FK_N8nNotifications_N8nNotificationTypes_IdN8nNotificationType",
                        column: x => x.IdN8nNotificationType,
                        principalTable: "N8nNotificationTypes",
                        principalColumn: "IdN8nNotificationType",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_N8nNotifications_Users_IdUser",
                        column: x => x.IdUser,
                        principalTable: "Users",
                        principalColumn: "IdUser",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_N8nNotifications_Warehouses_IdWarehouse",
                        column: x => x.IdWarehouse,
                        principalTable: "Warehouses",
                        principalColumn: "IdWarehouse",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "N8nNotificationTypes",
                columns: new[] { "IdN8nNotificationType", "CreatedAt", "Description", "IdUserCreated", "IdUserUpdated", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Notificaciones de texto directo para alertas rápidas y avisos generales.", 1, 1, true, "Text", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Notificaciones con reporte PDF adjunto para alertas de stock bajo y umbrales de inventario.", 1, 1, true, "PDF", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Flujo de envíos de mensajes error (notificación de fallos en ejecución o entrega de n8n).", 1, 1, true, "Error", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_N8nNotifications_IdN8nNotificationType",
                table: "N8nNotifications",
                column: "IdN8nNotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_N8nNotifications_IdUser",
                table: "N8nNotifications",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_N8nNotifications_IdWarehouse",
                table: "N8nNotifications",
                column: "IdWarehouse");

            migrationBuilder.CreateIndex(
                name: "IX_N8nNotificationTypes_IdUserCreated",
                table: "N8nNotificationTypes",
                column: "IdUserCreated");

            migrationBuilder.CreateIndex(
                name: "IX_N8nNotificationTypes_IdUserUpdated",
                table: "N8nNotificationTypes",
                column: "IdUserUpdated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "N8nNotifications");

            migrationBuilder.DropTable(
                name: "N8nNotificationTypes");
        }
    }
}
