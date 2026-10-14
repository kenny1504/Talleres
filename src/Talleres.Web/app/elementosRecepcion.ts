export interface ElementosRecepcion {
  dejaLlaves: boolean;
  dejaDocumentos: boolean;
  dejaLlantaRepuesto: boolean;
  dejaTrianguloSeguridad: boolean;
  dejaGata: boolean;
  dejaLlaveRuedas: boolean;
  dejaGanchoRemolque: boolean;
  dejaExtintor: boolean;
  dejaBotiquin: boolean;
  dejaChalecoReflectante: boolean;
  otrosElementos: string | null;
  aceptaPruebaRuta: boolean;
}

export const elementosRecepcion: ReadonlyArray<{
  campo: Exclude<keyof ElementosRecepcion, "otrosElementos" | "aceptaPruebaRuta">;
  etiqueta: string;
}> = [
  { campo: "dejaLlaves", etiqueta: "Llaves" },
  { campo: "dejaDocumentos", etiqueta: "Documentos" },
  { campo: "dejaLlantaRepuesto", etiqueta: "Llanta de repuesto" },
  { campo: "dejaTrianguloSeguridad", etiqueta: "Triángulo de seguridad" },
  { campo: "dejaGata", etiqueta: "Gata (gato)" },
  { campo: "dejaLlaveRuedas", etiqueta: "Llave de ruedas" },
  { campo: "dejaGanchoRemolque", etiqueta: "Gancho de remolque" },
  { campo: "dejaExtintor", etiqueta: "Extintor" },
  { campo: "dejaBotiquin", etiqueta: "Botiquín" },
  { campo: "dejaChalecoReflectante", etiqueta: "Chaleco reflectante" },
];

export function describirElementosEntregados(recepcion: ElementosRecepcion): string {
  const entregados = elementosRecepcion
    .filter(({ campo }) => recepcion[campo])
    .map(({ etiqueta }) => etiqueta);
  if (recepcion.otrosElementos?.trim()) entregados.push(recepcion.otrosElementos.trim());
  return entregados.length > 0 ? entregados.join(", ") : "Ninguno registrado";
}
