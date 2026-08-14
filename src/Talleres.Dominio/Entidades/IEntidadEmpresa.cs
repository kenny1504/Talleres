namespace Talleres.Dominio.Entidades;

/// <summary>
/// Identifica una entidad cuyos datos pertenecen a una empresa.
/// </summary>
public interface IEntidadEmpresa
{
    /// <summary>
    /// Obtiene o establece el identificador de la empresa propietaria.
    /// </summary>
    long EmpresaId { get; set; }
}
