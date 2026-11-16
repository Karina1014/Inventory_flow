using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Flow.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAdminToRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "Controller",
                table: "Menus");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Roles");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Controller",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
