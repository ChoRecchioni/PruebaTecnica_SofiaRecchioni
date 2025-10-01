using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PruebaTecnica_SofiaRecchioni.Migrations
{
    /// <inheritdoc />
    public partial class AddCantidadAndPrecioUnitario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Cantidad",
                table: "OrdenProductos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioUnitario",
                table: "OrdenProductos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "OrdenProductos");

            migrationBuilder.DropColumn(
                name: "PrecioUnitario",
                table: "OrdenProductos");
        }
    }
}
