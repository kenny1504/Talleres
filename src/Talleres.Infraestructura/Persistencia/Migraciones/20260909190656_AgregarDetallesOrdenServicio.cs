using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarDetallesOrdenServicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DetallesOrdenesServicio",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    OrdenServicioId = table.Column<long>(type: "bigint", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    ProductoInventarioId = table.Column<int>(type: "int", nullable: true),
                    BodegaInventarioId = table.Column<int>(type: "int", nullable: true),
                    SalidaInventarioId = table.Column<int>(type: "int", nullable: true),
                    CodigoProducto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Cantidad = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ExistenciaDescontada = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesOrdenesServicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesOrdenesServicio_OrdenesServicio_OrdenServicioId",
                        column: x => x.OrdenServicioId,
                        principalTable: "OrdenesServicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrdenesServicio_EmpresaId_OrdenServicioId",
                table: "DetallesOrdenesServicio",
                columns: new[] { "EmpresaId", "OrdenServicioId" });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrdenesServicio_EmpresaId_SalidaInventarioId",
                table: "DetallesOrdenesServicio",
                columns: new[] { "EmpresaId", "SalidaInventarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrdenesServicio_OrdenServicioId",
                table: "DetallesOrdenesServicio",
                column: "OrdenServicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesOrdenesServicio");
        }
    }
}
