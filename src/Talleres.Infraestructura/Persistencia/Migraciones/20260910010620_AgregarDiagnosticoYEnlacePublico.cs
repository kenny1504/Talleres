using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarDiagnosticoYEnlacePublico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Diagnostico",
                table: "OrdenesServicio",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAutorizacionClienteUtc",
                table: "OrdenesServicio",
                type: "datetime2(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDiagnosticoUtc",
                table: "OrdenesServicio",
                type: "datetime2(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenPublico",
                table: "OrdenesServicio",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EvidenciasDiagnostico",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    OrdenServicioId = table.Column<long>(type: "bigint", nullable: false),
                    ClaveObjeto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TipoContenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Longitud = table.Column<long>(type: "bigint", nullable: false),
                    FechaCargaUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenciasDiagnostico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenciasDiagnostico_OrdenesServicio_OrdenServicioId",
                        column: x => x.OrdenServicioId,
                        principalTable: "OrdenesServicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesServicio_TokenPublico",
                table: "OrdenesServicio",
                column: "TokenPublico",
                unique: true,
                filter: "[TokenPublico] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenciasDiagnostico_ClaveObjeto",
                table: "EvidenciasDiagnostico",
                column: "ClaveObjeto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvidenciasDiagnostico_EmpresaId_OrdenServicioId",
                table: "EvidenciasDiagnostico",
                columns: new[] { "EmpresaId", "OrdenServicioId" });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenciasDiagnostico_OrdenServicioId",
                table: "EvidenciasDiagnostico",
                column: "OrdenServicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvidenciasDiagnostico");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesServicio_TokenPublico",
                table: "OrdenesServicio");

            migrationBuilder.DropColumn(
                name: "Diagnostico",
                table: "OrdenesServicio");

            migrationBuilder.DropColumn(
                name: "FechaAutorizacionClienteUtc",
                table: "OrdenesServicio");

            migrationBuilder.DropColumn(
                name: "FechaDiagnosticoUtc",
                table: "OrdenesServicio");

            migrationBuilder.DropColumn(
                name: "TokenPublico",
                table: "OrdenesServicio");
        }
    }
}
