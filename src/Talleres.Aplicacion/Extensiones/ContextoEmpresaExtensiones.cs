using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Extensiones;

internal static class ContextoEmpresaExtensiones
{
    public static long ObtenerEmpresaIdRequerido(this IContextoEmpresa contextoEmpresa)
    {
        if (!contextoEmpresa.EstaDisponible || contextoEmpresa.EmpresaId <= 0)
        {
            throw new ReglaNegocioException(
                "No se pudo determinar la empresa asociada a la operación.");
        }

        return contextoEmpresa.EmpresaId;
    }
}
