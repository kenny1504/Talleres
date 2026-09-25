"use client";

import { useEffect, useRef, useState, type FormEvent } from "react";
import { CircleCheck, CreditCard, ReceiptText, TriangleAlert } from "lucide-react";
import { useCargadorPantalla } from "./ProveedorCargadorPantalla";

interface OrdenEstadoCuenta {
  id: number;
  numero: string;
  estado: string;
  fechaIngreso: string;
  total: number;
}

interface PagoCliente {
  id: number;
  monto: number;
  fechaRegistroUtc: string;
  formaPago: string | null;
  referencia: string | null;
  fechaAnulacionUtc: string | null;
}

interface EstadoCuenta {
  clienteId: number;
  nombreCliente: string;
  totalOrdenes: number;
  totalPagos: number;
  saldo: number;
  ordenes: OrdenEstadoCuenta[];
  pagos: PagoCliente[];
}

export function EstadoCuentaCliente({
  clienteId,
  direccionApi,
}: {
  clienteId: number;
  direccionApi: string;
}) {
  const { iniciarCargaPantalla, finalizarCargaPantalla, ejecutarConCargadorPantalla } = useCargadorPantalla();
  const [cuenta, setCuenta] = useState<EstadoCuenta | null>(null);
  const [monto, setMonto] = useState("");
  const [formaPago, setFormaPago] = useState("");
  const [referencia, setReferencia] = useState("");
  const [procesando, setProcesando] = useState(false);
  const procesandoRef = useRef(false);
  const [pagoPorAnularId, setPagoPorAnularId] = useState<number | null>(null);
  const [error, setError] = useState("");
  const [aviso, setAviso] = useState("");
  const direccionCuenta = `${direccionApi}/api/clientes/${clienteId}/estado-cuenta`;

  useEffect(() => {
    const controlador = new AbortController();
    const cargaId = iniciarCargaPantalla("Consultando el estado de cuenta del cliente…");
    fetch(direccionCuenta, { credentials: "include", signal: controlador.signal })
      .then(async (respuesta) => {
        if (!respuesta.ok) throw new Error(await obtenerError(respuesta, "No fue posible consultar el estado de cuenta."));
        return await respuesta.json() as EstadoCuenta;
      })
      .then((datos) => setCuenta(datos))
      .catch((motivo: unknown) => {
        if (!controlador.signal.aborted) setError(motivo instanceof Error ? motivo.message : "No fue posible consultar el estado de cuenta.");
      })
      .finally(() => finalizarCargaPantalla(cargaId));
    return () => controlador.abort();
  }, [direccionCuenta, finalizarCargaPantalla, iniciarCargaPantalla]);

  async function registrarAbono(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    if (procesandoRef.current) return;
    const montoNumerico = Number(monto);
    if (!Number.isFinite(montoNumerico) || montoNumerico <= 0 ||
        !/^\d+(?:\.\d{1,2})?$/.test(monto)) {
      setError("Ingresa un monto positivo con un máximo de dos decimales.");
      return;
    }

    procesandoRef.current = true;
    setProcesando(true);
    setError("");
    setAviso("");
    try {
      const datos = await ejecutarConCargadorPantalla("Registrando el abono del cliente…", async () => {
        const respuesta = await fetch(`${direccionApi}/api/clientes/${clienteId}/pagos`, {
          method: "POST",
          credentials: "include",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            monto: montoNumerico,
            formaPago: formaPago.trim() || null,
            referencia: referencia.trim() || null,
          }),
        });
        if (!respuesta.ok) throw new Error(await obtenerError(respuesta, "No fue posible registrar el abono."));
        return await respuesta.json() as EstadoCuenta;
      });
      setCuenta(datos);
      setMonto("");
      setFormaPago("");
      setReferencia("");
      setAviso("Abono registrado en la cuenta del cliente.");
    } catch (motivo) {
      setError(motivo instanceof Error ? motivo.message : "No fue posible registrar el abono.");
    } finally {
      procesandoRef.current = false;
      setProcesando(false);
    }
  }

  async function anularAbono(pagoId: number) {
    if (procesandoRef.current) return;
    procesandoRef.current = true;
    setProcesando(true);
    setError("");
    setAviso("");
    try {
      const datos = await ejecutarConCargadorPantalla("Anulando el abono del cliente…", async () => {
        const respuesta = await fetch(`${direccionApi}/api/clientes/${clienteId}/pagos/${pagoId}/anular`, {
          method: "POST",
          credentials: "include",
        });
        if (!respuesta.ok) throw new Error(await obtenerError(respuesta, "No fue posible anular el abono."));
        return await respuesta.json() as EstadoCuenta;
      });
      setCuenta(datos);
      setPagoPorAnularId(null);
      setAviso("Abono anulado. El saldo del cliente se actualizó.");
    } catch (motivo) {
      setError(motivo instanceof Error ? motivo.message : "No fue posible anular el abono.");
    } finally {
      procesandoRef.current = false;
      setProcesando(false);
    }
  }

  return (
    <div className="estado-cuenta-cliente">
      {error && <div className="estado-datos estado-datos-error" role="alert"><TriangleAlert size={20} />{error}</div>}
      {aviso && <div className="estado-datos estado-cuenta-aviso" role="status"><CircleCheck size={20} />{aviso}</div>}
      {cuenta && <>
        <section className="estado-cuenta-resumen" aria-label="Resumen de la cuenta">
          <div><span>Total de órdenes</span><strong>{moneda(cuenta.totalOrdenes)}</strong></div>
          <div><span>Abonos registrados</span><strong>{moneda(cuenta.totalPagos)}</strong></div>
          <div className="estado-cuenta-saldo"><span>{cuenta.saldo > 0 ? "Saldo pendiente" : cuenta.saldo < 0 ? "Saldo a favor" : "Cuenta al día"}</span><strong>{moneda(Math.abs(cuenta.saldo))}</strong></div>
        </section>
        <p className="estado-cuenta-aclaracion">Se suman los cargos actuales de las órdenes no canceladas. Los abonos se aplican a la cuenta completa del cliente.</p>
        <div className="estado-cuenta-columnas">
          <section className="estado-cuenta-panel">
            <header><ReceiptText size={21} /><div><h2>Órdenes</h2><p>{cuenta.ordenes.length} no canceladas</p></div></header>
            {cuenta.ordenes.length === 0 ? <p className="estado-cuenta-vacio">Este cliente todavía no tiene órdenes no canceladas.</p> :
              <div className="estado-cuenta-lista">
                {cuenta.ordenes.map((orden) => <article key={orden.id} className="estado-cuenta-fila">
                  <div><strong>{orden.numero}</strong><small>{fecha(orden.fechaIngreso)} · {etiquetaEstado(orden.estado)}</small></div>
                  <strong>{moneda(orden.total)}</strong>
                </article>)}
              </div>}
          </section>
          <div className="estado-cuenta-pagos">
            <section className="estado-cuenta-panel">
              <header><CreditCard size={21} /><div><h2>Registrar abono</h2><p>El pago se registra al cliente, sin elegir una orden.</p></div></header>
              <form className="estado-cuenta-formulario" onSubmit={registrarAbono}>
                <label htmlFor="monto-abono-cliente">Monto en córdobas</label>
                <input id="monto-abono-cliente" type="number" inputMode="decimal" min="0.01" step="0.01" required value={monto} onChange={(evento) => setMonto(evento.target.value)} disabled={procesando} placeholder="0.00" />
                <details>
                  <summary>Agregar datos opcionales</summary>
                  <div className="estado-cuenta-campos-opcionales">
                    <label htmlFor="forma-pago-cliente">Forma de pago (opcional)</label>
                    <input id="forma-pago-cliente" type="text" maxLength={60} value={formaPago} onChange={(evento) => setFormaPago(evento.target.value)} disabled={procesando} />
                    <label htmlFor="referencia-pago-cliente">Referencia (opcional)</label>
                    <input id="referencia-pago-cliente" type="text" maxLength={120} value={referencia} onChange={(evento) => setReferencia(evento.target.value)} disabled={procesando} />
                  </div>
                </details>
                <button className="boton-primario" type="submit" disabled={procesando}>Registrar abono</button>
              </form>
            </section>
            <section className="estado-cuenta-panel">
              <header><CreditCard size={21} /><div><h2>Abonos</h2><p>{cuenta.pagos.filter((pago) => !pago.fechaAnulacionUtc).length} vigentes</p></div></header>
              {cuenta.pagos.length === 0 ? <p className="estado-cuenta-vacio">No hay abonos registrados para este cliente.</p> :
                <div className="estado-cuenta-lista">
                  {cuenta.pagos.map((pago) => <article key={pago.id} className={`estado-cuenta-fila ${pago.fechaAnulacionUtc ? "estado-cuenta-pago-anulado" : ""}`}>
                    <div><strong>{moneda(pago.monto)}</strong><small>{fecha(pago.fechaRegistroUtc)}{pago.fechaAnulacionUtc ? " · Anulado" : ""}</small>{(pago.formaPago || pago.referencia) && <small>{[pago.formaPago, pago.referencia].filter(Boolean).join(" · ")}</small>}</div>
                    {!pago.fechaAnulacionUtc && (pagoPorAnularId === pago.id ?
                      <div className="estado-cuenta-confirmacion"><span>¿Anular?</span><button type="button" className="boton-secundario" disabled={procesando} onClick={() => setPagoPorAnularId(null)}>No</button><button type="button" className="boton-secundario" disabled={procesando} onClick={() => anularAbono(pago.id)}>Sí, anular</button></div> :
                      <button type="button" className="boton-secundario" disabled={procesando} onClick={() => setPagoPorAnularId(pago.id)}>Anular</button>)}
                  </article>)}
                </div>}
            </section>
          </div>
        </div>
      </>}
    </div>
  );
}

function moneda(valor: number) {
  return new Intl.NumberFormat("es-NI", { style: "currency", currency: "NIO" }).format(valor);
}

function fecha(valor: string) {
  return new Intl.DateTimeFormat("es-NI", { dateStyle: "medium" }).format(new Date(valor));
}

function etiquetaEstado(estado: string) {
  return ({ Recepcion: "Recepción", Diagnostico: "Diagnóstico", Cotizacion: "Cotización", PendienteAprobacion: "Por aprobar", PreparacionReparacion: "Preparación", Reparacion: "Reparación", ControlCalidad: "Control de calidad", ListaParaEntrega: "Lista para entregar", Entregada: "Entregada", Cerrada: "Cerrada" } as Record<string, string>)[estado] ?? estado;
}

async function obtenerError(respuesta: Response, mensajePredeterminado: string) {
  try {
    const datos = await respuesta.json() as { detail?: string; title?: string };
    return datos.detail || datos.title || mensajePredeterminado;
  } catch {
    return mensajePredeterminado;
  }
}
