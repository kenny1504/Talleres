using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarTecnicosTallerYResponsableOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TecnicoTallerId",
                table: "OrdenesServicio",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TecnicosTaller",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EsPredeterminado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TecnicosTaller", x => x.Id);
                    table.UniqueConstraint("AK_TecnicosTaller_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesServicio_EmpresaId_TecnicoTallerId",
                table: "OrdenesServicio",
                columns: new[] { "EmpresaId", "TecnicoTallerId" });

            migrationBuilder.CreateIndex(
                name: "IX_TecnicosTaller_EmpresaId_EsPredeterminado",
                table: "TecnicosTaller",
                columns: new[] { "EmpresaId", "EsPredeterminado" },
                unique: true,
                filter: "[EsPredeterminado] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TecnicosTaller_EmpresaId_Nombre",
                table: "TecnicosTaller",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesServicio_TecnicosTaller_EmpresaId_TecnicoTallerId",
                table: "OrdenesServicio",
                columns: new[] { "EmpresaId", "TecnicoTallerId" },
                principalTable: "TecnicosTaller",
                principalColumns: new[] { "EmpresaId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesServicio_TecnicosTaller_EmpresaId_TecnicoTallerId",
                table: "OrdenesServicio");

            migrationBuilder.DropTable(
                name: "TecnicosTaller");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesServicio_EmpresaId_TecnicoTallerId",
                table: "OrdenesServicio");

            migrationBuilder.DropColumn(
                name: "TecnicoTallerId",
                table: "OrdenesServicio");

        }
    }
}
