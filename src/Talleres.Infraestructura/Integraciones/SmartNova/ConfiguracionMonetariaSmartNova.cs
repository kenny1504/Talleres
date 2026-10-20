using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.DTOs.Configuracion;
using Talleres.Dominio.Excepciones;

namespace Talleres.Infraestructura.Integraciones.SmartNova;

public sealed class ConfiguracionMonetariaSmartNova(SmartNovaDbContext contexto) : IConfiguracionMonetariaSmartNova
{
    public async Task<ConfiguracionMonetariaDto> ObtenerAsync(int empresaNovaId, CancellationToken cancellationToken)
    {
        try
        {
            // Se reutiliza la misma consulta oficial que utiliza Galileo para formatear sus importes.
            var configuraciones = await contexto.Database
                .SqlQuery<FilaConfiguracionMonetaria>($"EXEC dbo.SP_getEmpresaMonedaDecimal @empresa = {empresaNovaId}")
                .ToListAsync(cancellationToken);
            var configuracion = configuraciones.SingleOrDefault();
            if (configuracion is null || configuracion.EmpresaId != empresaNovaId
                || string.IsNullOrWhiteSpace(configuracion.Simbolo)
                || configuracion.CantidadDecimales is null or < 0 or > 20
                || configuracion.SeparadorDecimal is not ("." or ","))
            {
                throw new IntegracionNoDisponibleException("La empresa no tiene una configuración monetaria válida en Galileo.");
            }

            // Galileo obtiene los miles con el separador opuesto al decimal, fuera del procedimiento.
            return new ConfiguracionMonetariaDto(configuracion.Simbolo, configuracion.SeparadorDecimal,
                configuracion.CantidadDecimales.Value, configuracion.SeparadorDecimal == "." ? "," : ".");
        }
        catch (SqlException excepcion)
        {
            throw new IntegracionNoDisponibleException("No fue posible consultar la configuración monetaria de Galileo.", excepcion);
        }
    }

    private sealed record FilaConfiguracionMonetaria(int EmpresaId, string? Simbolo, string? SeparadorDecimal, int? CantidadDecimales);
}
