using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarRevisionOperativaRecepcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ObservacionRevisionOperativa",
                table: "RecepcionesVehiculo",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ResultadosRevisionOperativa",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    RecepcionVehiculoId = table.Column<long>(type: "bigint", nullable: false),
                    Actividad = table.Column<int>(type: "int", nullable: false),
                    Correcto = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadosRevisionOperativa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultadosRevisionOperativa_RecepcionesVehiculo_RecepcionVehiculoId",
                        column: x => x.RecepcionVehiculoId,
                        principalTable: "RecepcionesVehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosRevisionOperativa_EmpresaId_RecepcionVehiculoId_Actividad",
                table: "ResultadosRevisionOperativa",
                columns: new[] { "EmpresaId", "RecepcionVehiculoId", "Actividad" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosRevisionOperativa_RecepcionVehiculoId",
                table: "ResultadosRevisionOperativa",
                column: "RecepcionVehiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResultadosRevisionOperativa");

            migrationBuilder.DropColumn(
                name: "ObservacionRevisionOperativa",
                table: "RecepcionesVehiculo");
        }
    }
}
