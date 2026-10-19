using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ValidarTelefonoUnicoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string normalizacionTelefono = "REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE([Telefono], N' ', N''), N'-', N''), N'(', N''), N')', N''), N'+', N''), N'.', N''), NCHAR(9), N''), NCHAR(13), N''), NCHAR(10), N''), NCHAR(160), N'')";

            migrationBuilder.Sql($"""
                IF EXISTS (
                    SELECT 1 FROM [Clientes]
                    GROUP BY [EmpresaId], {normalizacionTelefono}
                    HAVING COUNT_BIG(*) > 1
                )
                    THROW 51000, N'Existen clientes con teléfonos repetidos en la misma empresa. Revise esos registros antes de aplicar ValidarTelefonoUnicoCliente; no se eliminará ni fusionará ningún cliente automáticamente.', 1;
                """);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoNormalizado",
                table: "Clientes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                computedColumnSql: normalizacionTelefono,
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_EmpresaId_TelefonoNormalizado",
                table: "Clientes",
                columns: new[] { "EmpresaId", "TelefonoNormalizado" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_EmpresaId_TelefonoNormalizado",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TelefonoNormalizado",
                table: "Clientes");
        }
    }
}
