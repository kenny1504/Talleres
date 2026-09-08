using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    [DbContext(typeof(TallerDbContext))]
    [Migration("20260908160000_AgregarEvidenciasInspeccion")]
    public partial class AgregarEvidenciasInspeccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvidenciasInspeccion",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    RecepcionVehiculoId = table.Column<long>(type: "bigint", nullable: false),
                    ClaveObjeto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Longitud = table.Column<long>(type: "bigint", nullable: false),
                    FechaCargaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenciasInspeccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenciasInspeccion_RecepcionesVehiculo_RecepcionVehiculoId",
                        column: x => x.RecepcionVehiculoId,
                        principalTable: "RecepcionesVehiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenciasInspeccion_ClaveObjeto",
                table: "EvidenciasInspeccion",
                column: "ClaveObjeto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvidenciasInspeccion_EmpresaId_RecepcionVehiculoId",
                table: "EvidenciasInspeccion",
                columns: new[] { "EmpresaId", "RecepcionVehiculoId" });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenciasInspeccion_RecepcionVehiculoId",
                table: "EvidenciasInspeccion",
                column: "RecepcionVehiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EvidenciasInspeccion");
        }
    }
}
