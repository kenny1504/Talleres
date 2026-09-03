using System.Security.Claims;
using Talleres.Api.Autenticacion;
using Talleres.Aplicacion.Abstracciones.Multitenencia;

namespace Talleres.Api.Multitenencia;

public sealed class ContextoEmpresaHttp(IHttpContextAccessor httpContextAccessor) :
    IContextoEmpresa
{
    public long EmpresaId
    {
        get
        {
            var valor = httpContextAccessor.HttpContext?.User
                .FindFirstValue(ReclamosSesion.EmpresaNovaId);

            return long.TryParse(valor, out var empresaId) && empresaId > 0
                ? empresaId
                : 0;
        }
    }

    public bool EstaDisponible => EmpresaId > 0;
}
