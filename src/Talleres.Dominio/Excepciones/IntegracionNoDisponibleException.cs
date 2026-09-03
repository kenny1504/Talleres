namespace Talleres.Dominio.Excepciones;

public sealed class IntegracionNoDisponibleException(string mensaje, Exception excepcion) :
    Exception(mensaje, excepcion);
