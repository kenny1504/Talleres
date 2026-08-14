using Talleres.Infraestructura.Persistencia;

namespace Talleres.Pruebas.Persistencia;

public sealed class ConfiguracionConexionSqlPruebas
{
    [Theory]
    [InlineData("Server=(localdb)\\mssqllocaldb;Database=TalleresDb")]
    [InlineData("Server=localhost;Database=TalleresDb")]
    [InlineData("Server=127.0.0.1,1433;Database=TalleresDb")]
    public void ValidarRemota_RechazaServidoresLocales(string cadenaConexion)
    {
        var excepcion = Assert.Throws<InvalidOperationException>(
            () => ConfiguracionConexionSql.ValidarRemota(cadenaConexion));

        Assert.Contains("remoto", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidarRemota_AceptaServidorRemoto()
    {
        const string cadenaConexion =
            "Server=sql.example.com,1433;Database=Talleres;User ID=usuario;Password=clave";

        var resultado = ConfiguracionConexionSql.ValidarRemota(cadenaConexion);

        Assert.Equal(cadenaConexion, resultado);
    }
}
