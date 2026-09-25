using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class RegistrarPagosClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_Clientes_EmpresaId_Id",
                table: "Clientes",
                columns: new[] { "EmpresaId", "Id" });

            migrationBuilder.CreateTable(
                name: "PagosClientes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaRegistroUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    FormaPago = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Referencia = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    FechaAnulacionUtc = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosClientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosClientes_Clientes_EmpresaId_ClienteId",
                        columns: x => new { x.EmpresaId, x.ClienteId },
                        principalTable: "Clientes",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagosClientes_EmpresaId_ClienteId_FechaRegistroUtc",
                table: "PagosClientes",
                columns: new[] { "EmpresaId", "ClienteId", "FechaRegistroUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagosClientes");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Clientes_EmpresaId_Id",
                table: "Clientes");

        }
    }
}
