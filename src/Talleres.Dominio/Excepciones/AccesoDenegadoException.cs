namespace Talleres.Dominio.Excepciones;

public sealed class AccesoDenegadoException(string mensaje) : Exception(mensaje);
