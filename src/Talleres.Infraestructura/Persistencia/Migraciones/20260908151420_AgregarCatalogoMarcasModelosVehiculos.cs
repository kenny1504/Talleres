using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talleres.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarCatalogoMarcasModelosVehiculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ModeloVehiculoId",
                table: "Vehiculos",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MarcasVehiculo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarcasVehiculo", x => x.Id);
                    table.UniqueConstraint("AK_MarcasVehiculo_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "ModelosVehiculo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<long>(type: "bigint", nullable: false),
                    MarcaVehiculoId = table.Column<long>(type: "bigint", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelosVehiculo", x => x.Id);
                    table.UniqueConstraint("AK_ModelosVehiculo_EmpresaId_Id", x => new { x.EmpresaId, x.Id });
                    table.ForeignKey(
                        name: "FK_ModelosVehiculo_MarcasVehiculo_EmpresaId_MarcaVehiculoId",
                        columns: x => new { x.EmpresaId, x.MarcaVehiculoId },
                        principalTable: "MarcasVehiculo",
                        principalColumns: new[] { "EmpresaId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_EmpresaId_ModeloVehiculoId",
                table: "Vehiculos",
                columns: new[] { "EmpresaId", "ModeloVehiculoId" });

            migrationBuilder.CreateIndex(
                name: "IX_MarcasVehiculo_EmpresaId_Nombre",
                table: "MarcasVehiculo",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelosVehiculo_EmpresaId_MarcaVehiculoId_Nombre",
                table: "ModelosVehiculo",
                columns: new[] { "EmpresaId", "MarcaVehiculoId", "Nombre" },
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO MarcasVehiculo (EmpresaId, Nombre, Activa, FechaCreacion)
                SELECT EmpresaId, LTRIM(RTRIM(Marca)), 1, MIN(FechaCreacion)
                FROM Vehiculos
                GROUP BY EmpresaId, LTRIM(RTRIM(Marca));

                INSERT INTO ModelosVehiculo (EmpresaId, MarcaVehiculoId, Nombre, Activo, FechaCreacion)
                SELECT v.EmpresaId, ma.Id, LTRIM(RTRIM(v.Modelo)), 1, MIN(v.FechaCreacion)
                FROM Vehiculos v
                INNER JOIN MarcasVehiculo ma
                    ON ma.EmpresaId = v.EmpresaId
                    AND ma.Nombre = LTRIM(RTRIM(v.Marca))
                GROUP BY v.EmpresaId, ma.Id, LTRIM(RTRIM(v.Modelo));

                UPDATE v
                SET ModeloVehiculoId = mo.Id
                FROM Vehiculos v
                INNER JOIN MarcasVehiculo ma
                    ON ma.EmpresaId = v.EmpresaId
                    AND ma.Nombre = LTRIM(RTRIM(v.Marca))
                INNER JOIN ModelosVehiculo mo
                    ON mo.EmpresaId = v.EmpresaId
                    AND mo.MarcaVehiculoId = ma.Id
                    AND mo.Nombre = LTRIM(RTRIM(v.Modelo));
                """);

            migrationBuilder.AlterColumn<long>(
                name: "ModeloVehiculoId",
                table: "Vehiculos",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Marca",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "Modelo",
                table: "Vehiculos");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehiculos_ModelosVehiculo_EmpresaId_ModeloVehiculoId",
                table: "Vehiculos",
                columns: new[] { "EmpresaId", "ModeloVehiculoId" },
                principalTable: "ModelosVehiculo",
                principalColumns: new[] { "EmpresaId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Marca",
                table: "Vehiculos",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Modelo",
                table: "Vehiculos",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE v
                SET Marca = ma.Nombre, Modelo = mo.Nombre
                FROM Vehiculos v
                INNER JOIN ModelosVehiculo mo
                    ON mo.EmpresaId = v.EmpresaId
                    AND mo.Id = v.ModeloVehiculoId
                INNER JOIN MarcasVehiculo ma
                    ON ma.EmpresaId = mo.EmpresaId
                    AND ma.Id = mo.MarcaVehiculoId;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Vehiculos_ModelosVehiculo_EmpresaId_ModeloVehiculoId",
                table: "Vehiculos");

            migrationBuilder.DropTable(
                name: "ModelosVehiculo");

            migrationBuilder.DropTable(
                name: "MarcasVehiculo");

            migrationBuilder.DropIndex(
                name: "IX_Vehiculos_EmpresaId_ModeloVehiculoId",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "ModeloVehiculoId",
                table: "Vehiculos");

        }
    }
}
