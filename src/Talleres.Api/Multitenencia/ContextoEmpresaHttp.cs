using Talleres.Api.Middleware;
using Talleres.Aplicacion.Abstracciones.Multitenencia;

namespace Talleres.Api.Multitenencia;

public sealed class ContextoEmpresaHttp(IHttpContextAccessor httpContextAccessor) :
    IContextoEmpresa
{
    public long EmpresaId
    {
        get
        {
            var valor = httpContextAccessor.HttpContext?
                .Request.Headers[ValidacionEmpresaMiddleware.NombreEncabezado]
                .ToString();

            return long.TryParse(valor, out var empresaId) && empresaId > 0
                ? empresaId
                : 0;
        }
    }

    public bool EstaDisponible => EmpresaId > 0;
}
