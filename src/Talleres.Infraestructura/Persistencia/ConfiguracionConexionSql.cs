using Microsoft.Data.SqlClient;

namespace Talleres.Infraestructura.Persistencia;

public static class ConfiguracionConexionSql
{
    public static string ValidarRemota(string? cadenaConexion)
    {
        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            throw new InvalidOperationException(
                "No se configuró la cadena de conexión remota.");
        }

        var configuracionSql = new SqlConnectionStringBuilder(cadenaConexion);
        var servidor = configuracionSql.DataSource.Trim();
        if (string.IsNullOrWhiteSpace(servidor) ||
            servidor.StartsWith("Server=", StringComparison.OrdinalIgnoreCase) ||
            servidor.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "La cadena de conexión no contiene un servidor SQL válido.");
        }

        var anfitrion = servidor
            .Replace("tcp:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Split(',', '\\')[0]
            .Trim();
        var servidoresLocales = new[]
        {
            ".",
            "(local)",
            "localhost",
            "127.0.0.1",
            "::1",
            Environment.MachineName
        };
        if (servidor.Contains("(localdb)", StringComparison.OrdinalIgnoreCase) ||
            servidoresLocales.Contains(anfitrion, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Talleres requiere SQL Server remoto; no se permite una base de datos local.");
        }

        return cadenaConexion;
    }
}
