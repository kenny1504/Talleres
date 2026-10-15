export const actividadesRevisionOperativa = [
  { actividad: "LucesDelanteras", etiqueta: "Luces delanteras" },
  { actividad: "LucesBajas", etiqueta: "Luces bajas" },
  { actividad: "LucesAltas", etiqueta: "Luces altas" },
  { actividad: "LucesDireccionales", etiqueta: "Luces direccionales" },
  { actividad: "LucesLaterales", etiqueta: "Luces laterales" },
  { actividad: "LuzFreno", etiqueta: "Luz de freno" },
  { actividad: "Halogenos", etiqueta: "Halógenos" },
  { actividad: "AceiteMotor", etiqueta: "Aceite de motor" },
  { actividad: "LiquidoFrenos", etiqueta: "Líquido de frenos" },
  { actividad: "LiquidoEmbrague", etiqueta: "Líquido de embrague (clutch)" },
  { actividad: "AceiteDireccion", etiqueta: "Aceite de dirección" },
  { actividad: "Refrigerante", etiqueta: "Refrigerante" },
  { actividad: "RecordatorioAceite", etiqueta: "Recordatorio de aceite" },
  { actividad: "Bocina", etiqueta: "Bocina" },
] as const;

export type ActividadRevisionOperativa = typeof actividadesRevisionOperativa[number]["actividad"];

export interface ResultadoRevisionOperativa {
  actividad: ActividadRevisionOperativa;
  correcto: boolean;
}
