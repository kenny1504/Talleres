namespace Talleres.Aplicacion.Abstracciones.Multitenencia;

/// <summary>
/// Proporciona la empresa asociada a la operación actual.
/// </summary>
public interface IContextoEmpresa
{
    /// <summary>
    /// Obtiene el identificador de la empresa actual.
    /// </summary>
    long EmpresaId { get; }

    /// <summary>
    /// Indica si la operación tiene una empresa válida asociada.
    /// </summary>
    bool EstaDisponible { get; }
}
