using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Flow.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCodeAndPriceIva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Iva",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SuggestedPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Iva",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SuggestedPrice",
                table: "Products");
        }
    }
}
