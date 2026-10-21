"use client";

import { useId, useState } from "react";
import { Plus } from "lucide-react";
import {
  etiquetaZona,
  vistasInspeccion,
  type DanioVisual,
  type VistaInspeccion,
  type ZonaVehiculo,
} from "../inspeccionVehiculo";

export function MapaInspeccion({
  danios,
  alMarcar,
  compacto = false,
}: {
  danios: ReadonlyArray<Pick<DanioVisual, "zona" | "severidad">>;
  alMarcar?: (zona: ZonaVehiculo) => void;
  compacto?: boolean;
}) {
  const [vistaActiva, setVistaActiva] = useState<VistaInspeccion>("superior");
  const identificador = useId();
  const vista = vistasInspeccion.find((item) => item.id === vistaActiva)!;

  return (
    <div className={["mapa-inspeccion", compacto ? "mapa-compacto" : ""].join(" ")}>
      <div className="selector-vistas-inspeccion" role="group" aria-label="Vistas del vehículo">
        {vistasInspeccion.map((opcion) => {
          const cantidad = danios.filter((danio) => opcion.puntos.some((punto) => punto.zona === danio.zona)).length;
          return (
            <button
              key={opcion.id}
              type="button"
              aria-pressed={vistaActiva === opcion.id}
              aria-controls={identificador + "-silueta"}
              onClick={() => setVistaActiva(opcion.id)}
            >
              {opcion.etiqueta}
              {cantidad > 0 && <span aria-label={cantidad + " hallazgos"}>{cantidad}</span>}
            </button>
          );
        })}
      </div>
      <p className="orientacion-inspeccion">{vistaActiva === "superior" ? "Frente del vehículo ↑" : vistaActiva === "frontal" ? "Vista de frente" : "Vista desde atrás"}</p>
      <div className={"escenario-inspeccion escenario-" + vistaActiva} id={identificador + "-silueta"} role="group" aria-label={"Silueta: " + vista.etiqueta}>
        <div className={"silueta-inspeccion silueta-" + vistaActiva}>
          <SiluetaVehiculo vista={vistaActiva} />
          {vista.puntos.map(({ zona, x, y }) => {
            const daniosZona = danios.filter((danio) => danio.zona === zona);
            const severidad = ["Severo", "Moderado", "Leve"].find((nivel) => daniosZona.some((danio) => danio.severidad === nivel));
            const etiqueta = etiquetaZona(zona);
            return (
              <button
                key={zona}
                type="button"
                disabled={!alMarcar}
                className={"marca-zona " + (daniosZona.length ? "con-danio nivel-" + severidad?.toLowerCase() : "")}
                style={{ left: x + "%", top: y + "%" }}
                data-borde={x < 30 ? "izquierdo" : x > 70 ? "derecho" : undefined}
                aria-label={etiqueta + (daniosZona.length ? ", " + daniosZona.length + (daniosZona.length === 1 ? " daño, severidad " : " daños, severidad ") + severidad : ", añadir hallazgo")}
                onClick={() => alMarcar?.(zona)}
              >
                {daniosZona.length > 0 ? daniosZona.length : <Plus size={16} aria-hidden="true" />}
                <span>{etiqueta}</span>
              </button>
            );
          })}
        </div>
      </div>
      <p className="ayuda-mapa">
        {alMarcar ? "Toca un punto para añadir un hallazgo. " : "Selecciona una vista para consultar los daños. "}
        Izquierda y derecha corresponden al vehículo.
      </p>
    </div>
  );
}

function SiluetaVehiculo({ vista }: { vista: VistaInspeccion }) {
  if (vista === "superior") {
    return (
      <svg viewBox="0 0 310 440" className="dibujo-vehiculo" aria-hidden="true" focusable="false">
        <g className="neumaticos-silueta">
          <rect x="33" y="83" width="27" height="66" rx="9" />
          <rect x="250" y="83" width="27" height="66" rx="9" />
          <rect x="33" y="312" width="27" height="66" rx="9" />
          <rect x="250" y="312" width="27" height="66" rx="9" />
        </g>
        <path className="chapa-silueta" d="M155 22 C91 22 50 44 50 90 V380 Q50 418 96 418 H214 Q260 418 260 380 V90 C260 44 219 22 155 22Z" />
        <path className="detalle-silueta" d="M66 79 Q155 51 244 79 M66 391 Q155 410 244 391 M68 88 L88 133 M242 88 L222 133 M68 349 L88 324 M242 349 L222 324" />
        <path className="cristal-silueta" d="M88 133 Q155 112 222 133 L226 170 Q155 181 84 170Z" />
        <rect className="chapa-silueta" x="84" y="183" width="142" height="127" rx="15" />
        <path className="cristal-silueta" d="M84 323 Q155 313 226 323 L222 357 Q155 379 88 357Z" />
        <path className="detalle-silueta" d="M60 157 L76 180 V295 L60 326 M250 157 L234 180 V295 L250 326 M51 234 H84 M226 234 H259 M84 247 H226" />
      </svg>
    );
  }

  const frontal = vista === "frontal";
  return (
    <svg viewBox="0 0 310 250" className="dibujo-vehiculo" aria-hidden="true" focusable="false">
      <g className="neumaticos-silueta">
        <rect x="41" y="176" width="34" height="61" rx="9" />
        <rect x="235" y="176" width="34" height="61" rx="9" />
      </g>
      {frontal && <path className="chapa-silueta" d="M47 76 H23 Q13 76 13 88 V97 H43 M263 76 H287 Q297 76 297 88 V97 H267" />}
      <path className="chapa-silueta" d="M43 115 L68 48 Q77 24 102 24 H208 Q233 24 242 48 L267 115 Q279 126 279 150 V205 Q279 225 258 225 H52 Q31 225 31 205 V150 Q31 126 43 115Z" />
      <path className="cristal-silueta" d="M85 46 H225 L245 103 H65Z" />
      {frontal ? (
        <>
          <path className="detalle-silueta" d="M47 117 Q155 99 263 117 M91 113 L102 133 H208 L219 113" />
          <path className="luces-silueta" d="M45 139 L109 145 L104 164 H45Z M265 139 L201 145 L206 164 H265Z" />
          <rect className="rejilla-silueta" x="120" y="148" width="70" height="34" rx="8" />
          <path className="detalle-silueta" d="M47 196 H263 M111 210 H199" />
        </>
      ) : (
        <>
          <path className="detalle-silueta" d="M44 117 H266 M99 119 V165 H211 V119" />
          <path className="luces-silueta" d="M42 135 H89 V169 H42Z M268 135 H221 V169 H268Z" />
          <rect className="placa-silueta" x="124" y="172" width="62" height="24" rx="4" />
          <path className="detalle-silueta" d="M44 202 H266" />
        </>
      )}
    </svg>
  );
}
