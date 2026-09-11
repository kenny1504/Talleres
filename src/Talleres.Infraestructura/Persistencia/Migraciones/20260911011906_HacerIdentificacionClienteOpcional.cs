using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class HacerIdentificacionClienteOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_EmpresaId_DocumentoIdentidad",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentoIdentidad",
                table: "Clientes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_EmpresaId_DocumentoIdentidad",
                table: "Clientes",
                columns: new[] { "EmpresaId", "DocumentoIdentidad" },
                unique: true,
                filter: "[DocumentoIdentidad] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_EmpresaId_DocumentoIdentidad",
                table: "Clientes");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentoIdentidad",
                table: "Clientes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_EmpresaId_DocumentoIdentidad",
                table: "Clientes",
                columns: new[] { "EmpresaId", "DocumentoIdentidad" },
                unique: true);
        }
    }
}
