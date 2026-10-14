using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarElementosRecepcionVehiculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AceptaPruebaRuta",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DejaBotiquin",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DejaChalecoReflectante",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DejaExtintor",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DejaGanchoRemolque",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DejaGata",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DejaLlantaRepuesto",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DejaLlaveRuedas",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DejaTrianguloSeguridad",
                table: "RecepcionesVehiculo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OtrosElementos",
                table: "RecepcionesVehiculo",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AceptaPruebaRuta",
                table: "RecepcionesVehiculo");

            migrationBuilder.DropColumn(
                name: "DejaBotiquin",
                table: "RecepcionesVehiculo");

            migrationBuilder.DropColumn(
                name: "DejaChalecoReflectante",
                table: "RecepcionesVehiculo");

            migrationBuilder.DropColumn(
                name: "DejaExtintor",
                table: "RecepcionesVehiculo");

            migrationBuilder.DropColumn(
                name: "DejaGanchoRemolque",
                table: "RecepcionesVehiculo");

            migrationBuilder.DropColumn(
                name: "DejaGata",
                table: "RecepcionesVehiculo");

            migrationBuilder.DropColumn(
                name: "DejaLlantaRepuesto",
                table: "RecepcionesVehiculo");

            migrationBuilder.DropColumn(
                name: "DejaLlaveRuedas",
                table: "RecepcionesVehiculo");

            migrationBuilder.DropColumn(
                name: "DejaTrianguloSeguridad",
                table: "RecepcionesVehiculo");

            migrationBuilder.DropColumn(
                name: "OtrosElementos",
                table: "RecepcionesVehiculo");
        }
    }
}
