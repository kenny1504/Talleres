using Talleres.Aplicacion.DTOs.Configuracion;

namespace Talleres.Aplicacion.Abstracciones.Integraciones;

public interface IConfiguracionMonetariaSmartNova
{
    /// <summary>Consulta la moneda y el formato numérico vigentes de una empresa en Galileo.</summary>
    /// <param name="empresaNovaId">Empresa propietaria de la orden; en enlaces públicos se obtiene de la orden resuelta por su token.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Símbolo de moneda, separadores y cantidad de decimales con el mismo criterio de formato que Galileo.</returns>
    /// <exception cref="Talleres.Dominio.Excepciones.IntegracionNoDisponibleException">La configuración no existe, es inválida o Galileo no está disponible.</exception>
    Task<ConfiguracionMonetariaDto> ObtenerAsync(int empresaNovaId, CancellationToken cancellationToken);
}
