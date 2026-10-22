// La confirmación del cliente y el inicio de la reparación son pasos distintos.
export function estaAprobadaPorCliente(estado: string, fechaAutorizacionClienteUtc: string | null | undefined): boolean {
  return estado === "PendienteAprobacion" && Boolean(fechaAutorizacionClienteUtc);
}

export function etiquetaEstado(estado: string) { return ({ Recepcion: "Recepción", Diagnostico: "Diagnóstico", Cotizacion: "Cotización", PendienteAprobacion: "Por autorizar", PreparacionReparacion: "Preparación de reparación", Reparacion: "En reparación", ControlCalidad: "Control de calidad", ListaParaEntrega: "Lista para entregar", Entregada: "Entregada", Cerrada: "Cerrada", Cancelada: "Cancelada" } as Record<string, string>)[estado] ?? estado; }
