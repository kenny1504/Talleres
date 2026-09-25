const formatoFechaHoraOrden = new Intl.DateTimeFormat("es-NI", {
  day: "2-digit",
  month: "2-digit",
  year: "numeric",
  hour: "numeric",
  minute: "2-digit",
  hour12: true,
});

export function formatearFechaHoraOrden(fechaIngresoUtc: string): string {
  // SQL Server devuelve fechas UTC sin zona; al leerlas, el navegador debe tratarlas como UTC.
  const fechaConZona = /(?:Z|[+-]\d{2}:\d{2})$/i.test(fechaIngresoUtc)
    ? fechaIngresoUtc
    : `${fechaIngresoUtc}Z`;
  return formatoFechaHoraOrden.format(new Date(fechaConZona));
}
