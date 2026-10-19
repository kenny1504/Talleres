using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarAccesoriosRecepcionVehiculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DejaAlfombras",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DejaCopaRueda",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DejaPasacorrientes",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DejaAlfombras",
                table: "RecepcionesVehiculo");

            migrationBuilder.DropColumn(
                name: "DejaCopaRueda",
                table: "RecepcionesVehiculo");

            migrationBuilder.DropColumn(
                name: "DejaPasacorrientes",
                table: "RecepcionesVehiculo");
        }
    }
}
