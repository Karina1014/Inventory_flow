using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Flow.Migrations
{
    /// <inheritdoc />
    public partial class FixAddressFieldName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Addres",
                table: "Warehouses",
                newName: "Address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Warehouses",
                newName: "Addres");
        }
    }
}
