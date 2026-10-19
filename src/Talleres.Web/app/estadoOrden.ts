// La confirmación del cliente y el inicio de la reparación son pasos distintos.
export function estaAprobadaPorCliente(estado: string, fechaAutorizacionClienteUtc: string | null | undefined): boolean {
  return estado === "PendienteAprobacion" && Boolean(fechaAutorizacionClienteUtc);
}
