export interface ConfiguracionMonetaria {
  simbolo: string;
  separadorDecimal: string;
  cantidadDecimales: number;
  separadorMiles: string | null;
}

export function formatearImporte(valor: number, configuracion: ConfiguracionMonetaria): string {
  const numero = formatearNumero(valor, configuracion, configuracion.cantidadDecimales, configuracion.cantidadDecimales);
  return `${configuracion.simbolo} ${numero}`;
}

export function formatearCantidad(valor: number, configuracion: ConfiguracionMonetaria): string {
  return formatearNumero(valor, configuracion, 0, 4);
}

function formatearNumero(valor: number, configuracion: ConfiguracionMonetaria, minimoDecimales: number, maximoDecimales: number): string {
  return new Intl.NumberFormat("en-US", { minimumFractionDigits: minimoDecimales, maximumFractionDigits: maximoDecimales })
    .formatToParts(valor)
    .map((parte) => parte.type === "decimal" ? configuracion.separadorDecimal : parte.type === "group" ? configuracion.separadorMiles ?? "" : parte.value)
    .join("");
}
