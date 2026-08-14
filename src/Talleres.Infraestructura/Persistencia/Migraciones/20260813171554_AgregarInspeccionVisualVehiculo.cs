using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarInspeccionVisualVehiculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DaniosVehiculo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    RecepcionVehiculoId = table.Column<long>(type: "bigint", nullable: false),
                    Zona = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Severidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaniosVehiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DaniosVehiculo_RecepcionesVehiculo_RecepcionVehiculoId",
                        column: x => x.RecepcionVehiculoId,
                        principalTable: "RecepcionesVehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DaniosVehiculo_EmpresaId_RecepcionVehiculoId",
                table: "DaniosVehiculo",
                columns: new[] { "EmpresaId", "RecepcionVehiculoId" });

            migrationBuilder.CreateIndex(
                name: "IX_DaniosVehiculo_RecepcionVehiculoId",
                table: "DaniosVehiculo",
                column: "RecepcionVehiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DaniosVehiculo");
        }
    }
}
