const zonasVehiculo = {
  "frente": { etiqueta: "Frente", api: "Frente" },
  "capo": { etiqueta: "Capó", api: "Capo" },
  "parabrisas": { etiqueta: "Parabrisas", api: "Parabrisas" },
  "techo": { etiqueta: "Techo", api: "Techo" },
  "lateral-izquierdo": { etiqueta: "Lateral izquierdo", api: "LateralIzquierdo" },
  "lateral-derecho": { etiqueta: "Lateral derecho", api: "LateralDerecho" },
  "maletero": { etiqueta: "Maletero", api: "Maletero" },
  "posterior": { etiqueta: "Posterior", api: "Posterior" },
  "guardafango-delantero-izquierdo": { etiqueta: "Guardafango delantero izquierdo", api: "GuardafangoDelanteroIzquierdo" },
  "guardafango-delantero-derecho": { etiqueta: "Guardafango delantero derecho", api: "GuardafangoDelanteroDerecho" },
  "puerta-delantera-izquierda": { etiqueta: "Puerta delantera izquierda", api: "PuertaDelanteraIzquierda" },
  "puerta-delantera-derecha": { etiqueta: "Puerta delantera derecha", api: "PuertaDelanteraDerecha" },
  "puerta-trasera-izquierda": { etiqueta: "Puerta trasera izquierda", api: "PuertaTraseraIzquierda" },
  "puerta-trasera-derecha": { etiqueta: "Puerta trasera derecha", api: "PuertaTraseraDerecha" },
  "guardafango-trasero-izquierdo": { etiqueta: "Guardafango trasero izquierdo", api: "GuardafangoTraseroIzquierdo" },
  "guardafango-trasero-derecho": { etiqueta: "Guardafango trasero derecho", api: "GuardafangoTraseroDerecho" },
  "retrovisor-izquierdo": { etiqueta: "Retrovisor izquierdo", api: "RetrovisorIzquierdo" },
  "retrovisor-derecho": { etiqueta: "Retrovisor derecho", api: "RetrovisorDerecho" },
  "faro-izquierdo": { etiqueta: "Faro izquierdo", api: "FaroIzquierdo" },
  "faro-derecho": { etiqueta: "Faro derecho", api: "FaroDerecho" },
  "parachoques-delantero-izquierdo": { etiqueta: "Parachoques delantero izquierdo", api: "ParachoquesDelanteroIzquierdo" },
  "parachoques-delantero-derecho": { etiqueta: "Parachoques delantero derecho", api: "ParachoquesDelanteroDerecho" },
  "vidrio-trasero": { etiqueta: "Vidrio trasero", api: "VidrioTrasero" },
  "luz-trasera-izquierda": { etiqueta: "Luz trasera izquierda", api: "LuzTraseraIzquierda" },
  "luz-trasera-derecha": { etiqueta: "Luz trasera derecha", api: "LuzTraseraDerecha" },
  "parachoques-trasero-izquierdo": { etiqueta: "Parachoques trasero izquierdo", api: "ParachoquesTraseroIzquierdo" },
  "parachoques-trasero-derecho": { etiqueta: "Parachoques trasero derecho", api: "ParachoquesTraseroDerecho" },
} as const;

export type ZonaVehiculo = keyof typeof zonasVehiculo;
export type TipoDanio = "Rayón" | "Abolladura" | "Golpe" | "Vidrio" | "Luz";
export type SeveridadDanio = "Leve" | "Moderado" | "Severo";

export interface DanioVisual {
  id: string;
  zona: ZonaVehiculo;
  tipo: TipoDanio;
  severidad: SeveridadDanio;
  observacion: string;
}

interface PuntoInspeccion {
  zona: ZonaVehiculo;
  x: number;
  y: number;
}

export type VistaInspeccion = "superior" | "frontal" | "trasera";

export const vistasInspeccion: Array<{
  id: VistaInspeccion;
  etiqueta: string;
  puntos: PuntoInspeccion[];
}> = [
  {
    id: "superior", etiqueta: "Vista superior",
    puntos: [
      { zona: "frente", x: 50, y: 6 },
      { zona: "capo", x: 50, y: 20 },
      { zona: "parabrisas", x: 50, y: 34 },
      { zona: "techo", x: 50, y: 50 },
      { zona: "maletero", x: 50, y: 80 },
      { zona: "posterior", x: 50, y: 94 },
      { zona: "guardafango-delantero-izquierdo", x: 13, y: 22 },
      { zona: "guardafango-delantero-derecho", x: 87, y: 22 },
      { zona: "puerta-delantera-izquierda", x: 13, y: 39 },
      { zona: "puerta-delantera-derecha", x: 87, y: 39 },
      { zona: "lateral-izquierdo", x: 13, y: 53 },
      { zona: "lateral-derecho", x: 87, y: 53 },
      { zona: "puerta-trasera-izquierda", x: 13, y: 67 },
      { zona: "puerta-trasera-derecha", x: 87, y: 67 },
      { zona: "guardafango-trasero-izquierdo", x: 13, y: 83 },
      { zona: "guardafango-trasero-derecho", x: 87, y: 83 },
    ],
  },
  {
    id: "frontal", etiqueta: "Frontal",
    puntos: [
      { zona: "parabrisas", x: 50, y: 25 },
      // De frente, el lado izquierdo del vehículo queda a la derecha de la imagen.
      { zona: "retrovisor-derecho", x: 9, y: 35 },
      { zona: "retrovisor-izquierdo", x: 91, y: 35 },
      { zona: "faro-derecho", x: 23, y: 61 },
      { zona: "faro-izquierdo", x: 77, y: 61 },
      { zona: "frente", x: 50, y: 67 },
      { zona: "parachoques-delantero-derecho", x: 22, y: 87 },
      { zona: "parachoques-delantero-izquierdo", x: 78, y: 87 },
    ],
  },
  {
    id: "trasera", etiqueta: "Trasera",
    puntos: [
      { zona: "vidrio-trasero", x: 50, y: 25 },
      { zona: "maletero", x: 50, y: 54 },
      { zona: "luz-trasera-izquierda", x: 23, y: 61 },
      { zona: "luz-trasera-derecha", x: 77, y: 61 },
      { zona: "posterior", x: 50, y: 87 },
      { zona: "parachoques-trasero-izquierdo", x: 22, y: 87 },
      { zona: "parachoques-trasero-derecho", x: 78, y: 87 },
    ],
  },
];

export function etiquetaZona(zona: ZonaVehiculo) {
  return zonasVehiculo[zona]?.etiqueta || zona;
}

export function zonaParaApi(zona: ZonaVehiculo) {
  return zonasVehiculo[zona].api;
}

export function zonaDesdeApi(zona: string): ZonaVehiculo {
  return (Object.keys(zonasVehiculo) as ZonaVehiculo[])
    .find((id) => zonasVehiculo[id].api === zona) || "frente";
}
