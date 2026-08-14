using Talleres.Aplicacion.Abstracciones.Multitenencia;

namespace Talleres.Pruebas.Soporte;

internal sealed class ContextoEmpresaPrueba(long empresaId) : IContextoEmpresa
{
    public long EmpresaId { get; } = empresaId;

    public bool EstaDisponible => EmpresaId > 0;
}
