"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";

interface OperacionCargaPantalla {
  id: number;
  mensaje: string;
}

interface ContextoCargadorPantalla {
  iniciarCargaPantalla: (mensaje: string) => number;
  finalizarCargaPantalla: (id: number) => void;
  ejecutarConCargadorPantalla: <T>(mensaje: string, operacion: () => Promise<T>) => Promise<T>;
}

const CargadorPantallaContexto = createContext<ContextoCargadorPantalla | null>(null);

export function ProveedorCargadorPantalla({ children }: { children: ReactNode }) {
  const [operaciones, setOperaciones] = useState<OperacionCargaPantalla[]>([]);
  const siguienteId = useRef(0);
  const operacionActual = operaciones[operaciones.length - 1];
  const cargando = operaciones.length > 0;

  const iniciarCargaPantalla = useCallback((mensaje: string) => {
    siguienteId.current += 1;
    const id = siguienteId.current;
    setOperaciones((actuales) => [...actuales, { id, mensaje }]);
    return id;
  }, []);

  const finalizarCargaPantalla = useCallback((id: number) => {
    setOperaciones((actuales) => actuales.filter((operacion) => operacion.id !== id));
  }, []);

  const ejecutarConCargadorPantalla = useCallback(async <T,>(
    mensaje: string,
    operacion: () => Promise<T>,
  ) => {
    const id = iniciarCargaPantalla(mensaje);
    try {
      return await operacion();
    } finally {
      finalizarCargaPantalla(id);
    }
  }, [finalizarCargaPantalla, iniciarCargaPantalla]);

  useEffect(() => {
    if (!cargando) return;
    const desbordamientoAnterior = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.body.style.overflow = desbordamientoAnterior;
    };
  }, [cargando]);

  const contexto = useMemo(
    () => ({ iniciarCargaPantalla, finalizarCargaPantalla, ejecutarConCargadorPantalla }),
    [ejecutarConCargadorPantalla, finalizarCargaPantalla, iniciarCargaPantalla],
  );

  return (
    <CargadorPantallaContexto.Provider value={contexto}>
      <div className="contenido-bajo-cargador" inert={cargando ? true : undefined} aria-busy={cargando}>
        {children}
      </div>
      {operacionActual && <CargadorPantalla mensaje={operacionActual.mensaje} />}
    </CargadorPantallaContexto.Provider>
  );
}

export function useCargadorPantalla() {
  const contexto = useContext(CargadorPantallaContexto);
  if (!contexto) {
    throw new Error("useCargadorPantalla debe utilizarse dentro de ProveedorCargadorPantalla.");
  }
  return contexto;
}

export function CargadorPantalla({ mensaje }: { mensaje: string }) {
  return (
    <div className="cargador-pantalla" role="status" aria-live="assertive" aria-atomic="true">
      <div className="cargador-pantalla-contenido">
        <span className="cargador-pantalla-marca" aria-hidden="true">T</span>
        <span className="cargador-pantalla-indicador" aria-hidden="true">
          <span />
          <span />
          <span />
        </span>
        <strong>{mensaje}</strong>
        <small>Espera un momento. No cierres esta pantalla.</small>
      </div>
    </div>
  );
}
