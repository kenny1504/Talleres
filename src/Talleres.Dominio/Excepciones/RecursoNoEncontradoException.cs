namespace Talleres.Dominio.Excepciones;

/// <summary>
/// Indica que un recurso solicitado no existe dentro de la empresa actual.
/// </summary>
public sealed class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string mensaje)
        : base(mensaje)
    {
    }
}
