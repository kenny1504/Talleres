using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarTalleresSincronizados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TalleresSincronizados",
                columns: table => new
                {
                    EmpresaNovaId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaConfiguracionUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalleresSincronizados", x => x.EmpresaNovaId);
                });

            migrationBuilder.InsertData(
                table: "TalleresSincronizados",
                columns: new[] { "EmpresaNovaId", "Activo", "FechaConfiguracionUtc" },
                values: new object[] { 3071, true, new DateTime(2026, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_TalleresSincronizados_Activo",
                table: "TalleresSincronizados",
                column: "Activo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TalleresSincronizados");
        }
    }
}
