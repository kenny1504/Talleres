"use client";

import type { FormEvent } from "react";
import { useEffect, useMemo, useState } from "react";
import {
  ArrowLeft,
  Bell,
  Boxes,
  CalendarDays,
  Camera,
  CarFront,
  Check,
  ChevronRight,
  CircleCheck,
  ClipboardCheck,
  ClipboardList,
  Clock3,
  Fuel,
  LayoutDashboard,
  MessageCircleMore,
  MoreHorizontal,
  ImagePlus,
  Pencil,
  Plus,
  RotateCcw,
  Search,
  Sparkles,
  Trash2,
  TriangleAlert,
  UserRound,
  Users,
  Wrench,
  X,
  type LucideIcon,
} from "lucide-react";

type Vista = "inicio" | "ordenes" | "clientes" | "vehiculos" | "inventario";
type EstadoOrden =
  | "Recepción"
  | "Diagnóstico"
  | "Cotización"
  | "Por aprobar"
  | "Reparación"
  | "Control de calidad"
  | "Lista para entregar";

interface OrdenTaller {
  id: number;
  clienteId: number;
  vehiculoId: number;
  numero: string;
  cliente: string;
  vehiculo: string;
  placa: string;
  estado: EstadoOrden;
  motivo: string;
  tecnico: string;
  hora: string;
  progreso: number;
  color: string;
  prioridad?: boolean;
}

type ZonaVehiculo =
  | "frente"
  | "capo"
  | "parabrisas"
  | "techo"
  | "lateral-izquierdo"
  | "lateral-derecho"
  | "maletero"
  | "posterior";

type TipoDanio = "Rayón" | "Abolladura" | "Golpe" | "Vidrio" | "Luz";
type SeveridadDanio = "Leve" | "Moderado" | "Severo";

interface DanioVisual {
  id: string;
  zona: ZonaVehiculo;
  tipo: TipoDanio;
  severidad: SeveridadDanio;
  observacion: string;
}

interface InspeccionVisual {
  kilometraje: number;
  porcentajeCombustible: number;
  descripcionEstado: string;
  dejaLlaves: boolean;
  dejaDocumentos: boolean;
  danios: DanioVisual[];
}

interface OrdenServicioApi {
  id: number;
  numero: string;
  clienteId: number;
  nombreCliente: string;
  vehiculoId: number;
  placaVehiculo: string;
  estado: string;
  fechaIngreso: string;
  observaciones: string | null;
}

interface ClienteApi {
  id: number;
  nombre: string;
  documentoIdentidad: string;
  telefono: string;
  correo: string | null;
  direccion: string | null;
  activo: boolean;
  fechaCreacion: string;
}

interface VehiculoApi {
  id: number;
  clienteId: number;
  nombreCliente: string;
  placa: string;
  marca: string;
  modelo: string;
  anio: number;
  color: string | null;
  numeroVin: string | null;
  activo: boolean;
  fechaCreacion: string;
}

interface ClienteTaller {
  id: number;
  iniciales: string;
  nombre: string;
  telefono: string;
  cantidadVehiculos: number;
  ordenActiva: string | null;
}

interface VehiculoTaller {
  id: number;
  clienteId: number;
  placa: string;
  nombre: string;
  detalle: string;
  cliente: string;
  activo: boolean;
}

interface DanioVehiculoApi {
  zona: string;
  tipo: string;
  severidad: string;
  observacion: string | null;
}

interface RecepcionVehiculoApi {
  kilometraje: number;
  porcentajeCombustible: number;
  descripcionEstado: string;
  dejaLlaves: boolean;
  dejaDocumentos: boolean;
  danios: DanioVehiculoApi[];
}

interface NavegacionItem {
  id: Vista;
  etiqueta: string;
  icono: LucideIcon;
}

const navegacion: NavegacionItem[] = [
  { id: "inicio", etiqueta: "Inicio", icono: LayoutDashboard },
  { id: "ordenes", etiqueta: "Órdenes", icono: ClipboardList },
  { id: "clientes", etiqueta: "Clientes", icono: Users },
  { id: "vehiculos", etiqueta: "Vehículos", icono: CarFront },
  { id: "inventario", etiqueta: "Inventario", icono: Boxes },
];

const zonasVehiculo: Array<{ id: ZonaVehiculo; etiqueta: string }> = [
  { id: "frente", etiqueta: "Frente" },
  { id: "capo", etiqueta: "Capó" },
  { id: "parabrisas", etiqueta: "Parabrisas" },
  { id: "techo", etiqueta: "Techo" },
  { id: "lateral-izquierdo", etiqueta: "Lateral izquierdo" },
  { id: "lateral-derecho", etiqueta: "Lateral derecho" },
  { id: "maletero", etiqueta: "Maletero" },
  { id: "posterior", etiqueta: "Posterior" },
];

export default function PaginaPrincipal() {
  const [vista, setVista] = useState<Vista>("inicio");
  const [ordenes, setOrdenes] = useState<OrdenTaller[]>([]);
  const [clientes, setClientes] = useState<ClienteTaller[]>([]);
  const [vehiculos, setVehiculos] = useState<VehiculoTaller[]>([]);
  const [cargandoDatos, setCargandoDatos] = useState(true);
  const [errorDatos, setErrorDatos] = useState("");
  const [guardandoOrden, setGuardandoOrden] = useState(false);
  const [busqueda, setBusqueda] = useState("");
  const [filtroEstado, setFiltroEstado] = useState("Todas");
  const [mostrarNuevaOrden, setMostrarNuevaOrden] = useState(false);
  const [ordenDetalle, setOrdenDetalle] = useState<OrdenTaller | null>(null);
  const [mostrarRecepcion, setMostrarRecepcion] = useState(false);
  const [aviso, setAviso] = useState("");
  const [inspecciones, setInspecciones] = useState<Record<number, InspeccionVisual>>({});
  const procesoActivo = mostrarNuevaOrden || ordenDetalle !== null;

  useEffect(() => {
    const controlador = new AbortController();
    cargarDatosApi(controlador.signal)
      .then((datos) => {
        setOrdenes(datos.ordenes);
        setClientes(datos.clientes);
        setVehiculos(datos.vehiculos);
        setErrorDatos("");
      })
      .catch(() => {
        if (controlador.signal.aborted) return;
        setOrdenes([]);
        setClientes([]);
        setVehiculos([]);
        setErrorDatos("No fue posible comunicarse con la API configurada.");
      })
      .finally(() => {
        if (!controlador.signal.aborted) setCargandoDatos(false);
      });
    return () => controlador.abort();
  }, []);

  useEffect(() => {
    if (!ordenDetalle || ordenDetalle.estado === "Recepción") return;
    const controlador = new AbortController();
    cargarInspeccionApi(ordenDetalle.id, controlador.signal)
      .then((inspeccion) => {
        if (inspeccion) {
          setInspecciones((actuales) => ({ ...actuales, [ordenDetalle.id]: inspeccion }));
        }
      })
      .catch(() => {
        mostrarAviso("No fue posible consultar la inspección en la API");
      });
    return () => controlador.abort();
  }, [ordenDetalle]);

  useEffect(() => {
    if (procesoActivo) window.scrollTo({ top: 0, behavior: "smooth" });
  }, [procesoActivo, mostrarRecepcion]);

  const ordenesFiltradas = useMemo(() => {
    const termino = busqueda.trim().toLocaleLowerCase("es");
    return ordenes.filter((orden) => {
      const coincideEstado = filtroEstado === "Todas" || orden.estado === filtroEstado;
      const coincideBusqueda =
        !termino ||
        [orden.numero, orden.cliente, orden.vehiculo, orden.placa]
          .join(" ")
          .toLocaleLowerCase("es")
          .includes(termino);
      return coincideEstado && coincideBusqueda;
    });
  }, [busqueda, filtroEstado, ordenes]);

  function navegar(nuevaVista: Vista) {
    setVista(nuevaVista);
    setBusqueda("");
  }

  async function crearOrden(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    setGuardandoOrden(true);
    try {
      const nuevaOrden = await crearOrdenApi({
        clienteId: Number(datos.get("clienteId")),
        vehiculoId: Number(datos.get("vehiculoId")),
        observaciones: String(datos.get("motivo")),
      });
      setOrdenes((actuales) => [nuevaOrden, ...actuales]);
      setMostrarNuevaOrden(false);
      setVista("ordenes");
      mostrarAviso(`Orden ${nuevaOrden.numero} creada y lista para recepción`);
    } catch (error) {
      mostrarAviso(
        error instanceof Error ? error.message : "No fue posible crear la orden en la API",
      );
    } finally {
      setGuardandoOrden(false);
    }
  }

  async function registrarRecepcion(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    if (!ordenDetalle) return;
    const datos = new FormData(evento.currentTarget);
    const danios = JSON.parse(String(datos.get("danios") || "[]")) as DanioVisual[];
    const esActualizacion = ordenDetalle.estado !== "Recepción";
    const inspeccion: InspeccionVisual = {
      kilometraje: Number(datos.get("kilometraje")),
      porcentajeCombustible: Number(datos.get("combustible")),
      descripcionEstado: String(datos.get("estado")),
      dejaLlaves: datos.has("dejaLlaves"),
      dejaDocumentos: datos.has("dejaDocumentos"),
      danios,
    };

    try {
      await guardarRecepcionApi(ordenDetalle.id, inspeccion, esActualizacion);
    } catch {
      mostrarAviso("No fue posible guardar la recepción en la API");
      return;
    }
    setOrdenes((actuales) =>
      actuales.map((orden) =>
        orden.id === ordenDetalle.id
          ? esActualizacion
            ? orden
            : { ...orden, estado: "Diagnóstico", progreso: 28, tecnico: "Por asignar" }
          : orden,
      ),
    );
    setInspecciones((actuales) => ({ ...actuales, [ordenDetalle.id]: inspeccion }));
    setMostrarRecepcion(false);
    setOrdenDetalle(null);
    mostrarAviso(
      esActualizacion
        ? "Inspección actualizada correctamente"
        : "Recepción guardada; la orden pasó a diagnóstico",
    );
  }

  function mostrarAviso(mensaje: string) {
    setAviso(mensaje);
    window.setTimeout(() => setAviso(""), 3600);
  }

  return (
    <div className="aplicacion">
      <BarraLateral vista={vista} alNavegar={navegar} />

      <div className={`superficie ${procesoActivo ? "proceso-activo" : ""}`}>
        <header className="barra-superior">
          <div className="marca-compacta">
            <span className="marca-simbolo">T</span>
            <span>Taller Uno</span>
          </div>

          <label className="buscador-global">
            <Search aria-hidden="true" size={21} />
            <span className="solo-lectores">Buscar</span>
            <input
              value={busqueda}
              onChange={(evento) => {
                setBusqueda(evento.target.value);
                if (evento.target.value) setVista("ordenes");
              }}
              placeholder="Buscar orden, cliente o placa"
            />
            <kbd>⌘ K</kbd>
          </label>

          <div className="acciones-superiores">
            <button className="boton-icono" aria-label="Ver notificaciones">
              <Bell size={22} />
              <span className="punto-notificacion" />
            </button>
            <div className="usuario">
              <span className="avatar">JM</span>
              <span className="usuario-texto">
                <strong>Javier M.</strong>
                <small>Jefe de taller</small>
              </span>
            </div>
          </div>
        </header>

        <main className={`contenido-principal ${procesoActivo ? "contenido-proceso" : ""}`}>
          {!procesoActivo && cargandoDatos && (
            <div className="estado-datos" role="status">
              <Clock3 size={20} /> Consultando datos de la API…
            </div>
          )}
          {!procesoActivo && errorDatos && (
            <div className="estado-datos estado-datos-error" role="alert">
              <TriangleAlert size={20} />
              <span><strong>Datos no disponibles.</strong> {errorDatos}</span>
            </div>
          )}
          {mostrarNuevaOrden ? (
            <PaginaProceso
              titulo="Nueva orden de servicio"
              descripcion="Registra el vehículo y el motivo de ingreso sin salir del área de trabajo."
              alRegresar={() => setMostrarNuevaOrden(false)}
            >
              <div className="contenedor-formulario-orden">
                <FormularioNuevaOrden
                  clientes={clientes}
                  vehiculos={vehiculos}
                  guardando={guardandoOrden}
                  alEnviar={crearOrden}
                />
              </div>
            </PaginaProceso>
          ) : ordenDetalle ? (
            <PaginaProceso
              titulo={mostrarRecepcion
                ? ordenDetalle.estado === "Recepción"
                  ? "Recepción e inspección"
                  : "Editar inspección"
                : ordenDetalle.numero}
              descripcion={mostrarRecepcion
                ? `${ordenDetalle.vehiculo} · ${ordenDetalle.placa}`
                : `${ordenDetalle.cliente} · ${ordenDetalle.vehiculo}`}
              alRegresar={() => {
                if (mostrarRecepcion) setMostrarRecepcion(false);
                else setOrdenDetalle(null);
              }}
              etiquetaRegreso={mostrarRecepcion ? "Volver a la orden" : "Volver al tablero"}
            >
              {mostrarRecepcion ? (
                <FormularioRecepcion
                  orden={ordenDetalle}
                  inspeccionInicial={inspecciones[ordenDetalle.id]}
                  alEnviar={registrarRecepcion}
                />
              ) : (
                <DetalleOrden
                  orden={ordenDetalle}
                  inspeccion={inspecciones[ordenDetalle.id]}
                  alRecibir={() => setMostrarRecepcion(true)}
                  alNotificar={() => mostrarAviso("Actualización enviada a WhatsApp")}
                />
              )}
            </PaginaProceso>
          ) : vista === "inicio" ? (
            <VistaInicio
              ordenes={ordenes}
              cargando={cargandoDatos}
              datosDisponibles={!errorDatos}
              alCrearOrden={() => setMostrarNuevaOrden(true)}
              alAbrirOrden={setOrdenDetalle}
              alVerOrdenes={() => navegar("ordenes")}
            />
          ) : vista === "ordenes" ? (
            <VistaOrdenes
              ordenes={ordenesFiltradas}
              cargando={cargandoDatos}
              datosDisponibles={!errorDatos}
              filtro={filtroEstado}
              alFiltrar={setFiltroEstado}
              alCrearOrden={() => setMostrarNuevaOrden(true)}
              alAbrirOrden={setOrdenDetalle}
            />
          ) : vista === "clientes" ? (
            <VistaClientes
              clientes={clientes}
              cargando={cargandoDatos}
              datosDisponibles={!errorDatos}
              alCrearOrden={() => setMostrarNuevaOrden(true)}
            />
          ) : vista === "vehiculos" ? (
            <VistaVehiculos
              vehiculos={vehiculos}
              cargando={cargandoDatos}
              datosDisponibles={!errorDatos}
            />
          ) : (
            <VistaInventario />
          )}
        </main>

        {!procesoActivo && <NavegacionInferior vista={vista} alNavegar={navegar} />}
      </div>

      {aviso && (
        <div className="aviso" role="status">
          <CircleCheck size={21} />
          {aviso}
        </div>
      )}
    </div>
  );
}

function BarraLateral({ vista, alNavegar }: { vista: Vista; alNavegar: (vista: Vista) => void }) {
  return (
    <aside className="barra-lateral">
      <button className="marca" onClick={() => alNavegar("inicio")} aria-label="Ir al inicio">
        <span className="marca-simbolo">T</span>
        <span className="marca-nombre">Taller Uno</span>
      </button>

      <nav className="navegacion-principal" aria-label="Navegación principal">
        {navegacion.map((item) => {
          const Icono = item.icono;
          return (
            <button
              key={item.id}
              className={vista === item.id ? "navegacion-activa" : ""}
              onClick={() => alNavegar(item.id)}
            >
              <Icono size={23} strokeWidth={1.9} />
              <span>{item.etiqueta}</span>
            </button>
          );
        })}
      </nav>

      <div className="estado-sede">
        <span className="estado-en-linea" />
        <div>
          <strong>Centro Managua</strong>
          <small>Operación en línea</small>
        </div>
        <ChevronRight size={18} />
      </div>
    </aside>
  );
}

function NavegacionInferior({ vista, alNavegar }: { vista: Vista; alNavegar: (vista: Vista) => void }) {
  return (
    <nav className="navegacion-inferior" aria-label="Navegación para tablet vertical">
      {navegacion.slice(0, 4).map((item) => {
        const Icono = item.icono;
        return (
          <button
            key={item.id}
            className={vista === item.id ? "navegacion-activa" : ""}
            onClick={() => alNavegar(item.id)}
          >
            <Icono size={22} />
            <span>{item.etiqueta}</span>
          </button>
        );
      })}
      <button
        className={vista === "inventario" ? "navegacion-activa" : ""}
        onClick={() => alNavegar("inventario")}
      >
        <MoreHorizontal size={22} />
        <span>Más</span>
      </button>
    </nav>
  );
}

function VistaInicio({
  ordenes,
  cargando,
  datosDisponibles,
  alCrearOrden,
  alAbrirOrden,
  alVerOrdenes,
}: {
  ordenes: OrdenTaller[];
  cargando: boolean;
  datosDisponibles: boolean;
  alCrearOrden: () => void;
  alAbrirOrden: (orden: OrdenTaller) => void;
  alVerOrdenes: () => void;
}) {
  const ordenesEnTaller = ordenes.filter((orden) => orden.estado !== "Lista para entregar");
  const ordenesPorAprobar = ordenes.filter((orden) => orden.estado === "Por aprobar");
  const ordenesListas = ordenes.filter((orden) => orden.estado === "Lista para entregar");
  const ordenesEnRecepcion = ordenes.filter((orden) => orden.estado === "Recepción");
  const fechaActual = new Intl.DateTimeFormat("es-NI", {
    weekday: "long",
    day: "numeric",
    month: "long",
  }).format(new Date());
  const valorMetrica = (cantidad: number) =>
    cargando ? "…" : datosDisponibles ? String(cantidad) : "—";

  return (
    <>
      <section className="encabezado-pagina encabezado-inicio">
        <div>
          <span className="sobrelinea">{fechaActual}</span>
          <h1>Buen día, Javier</h1>
          <p>
            {cargando
              ? "Consultando la operación actual del taller."
              : datosDisponibles
                ? `${ordenesPorAprobar.length} órdenes necesitan aprobación.`
                : "La operación no puede consultarse en este momento."}
          </p>
        </div>
        <button className="boton-primario" onClick={alCrearOrden}>
          <Plus size={21} />
          Nueva orden
        </button>
      </section>

      <section className="metricas" aria-label="Resumen del taller">
        <TarjetaMetrica etiqueta="En taller" valor={valorMetrica(ordenesEnTaller.length)} detalle={datosDisponibles ? "Órdenes activas" : "Sin conexión"} icono={Wrench} tono="verde" />
        <TarjetaMetrica etiqueta="Por aprobar" valor={valorMetrica(ordenesPorAprobar.length)} detalle={datosDisponibles ? "Pendientes del cliente" : "Sin conexión"} icono={Clock3} tono="ambar" />
        <TarjetaMetrica etiqueta="Listas" valor={valorMetrica(ordenesListas.length)} detalle={datosDisponibles ? "Para entregar" : "Sin conexión"} icono={CircleCheck} tono="azul" />
        <TarjetaMetrica etiqueta="Recepción" valor={valorMetrica(ordenesEnRecepcion.length)} detalle={datosDisponibles ? "Por inspeccionar" : "Sin conexión"} icono={ClipboardCheck} tono="rojo" />
      </section>

      <div className="tablero-principal">
        <section className="panel panel-operacion">
          <div className="titulo-panel">
            <div>
              <span className="sobrelinea">Operación en vivo</span>
              <h2>Bahías del taller</h2>
            </div>
            <button className="boton-texto" onClick={alVerOrdenes}>
              Ver todas <ChevronRight size={18} />
            </button>
          </div>

          <div className="lista-bahias">
            {ordenes.slice(0, 4).map((orden, indice) => (
              <button className="tarjeta-bahia" key={orden.id} onClick={() => alAbrirOrden(orden)}>
                <span className="numero-bahia">B{indice + 1}</span>
                <span className="datos-bahia">
                  <span className="fila-orden">
                    <strong>{orden.vehiculo.split(" · ")[0]}</strong>
                    <small>{orden.numero}</small>
                  </span>
                  <span className="detalle-orden">{orden.placa} · {orden.tecnico}</span>
                  <span className="progreso" aria-label={`${orden.progreso}% completado`}>
                    <span style={{ width: `${orden.progreso}%`, background: orden.color }} />
                  </span>
                </span>
                <span className={`etiqueta-estado estado-${normalizarClase(orden.estado)}`}>
                  {orden.estado}
                </span>
                <ChevronRight className="flecha-tarjeta" size={20} />
              </button>
            ))}
            {!cargando && ordenes.length === 0 && (
              <EstadoVacio
                icono={ClipboardList}
                titulo={datosDisponibles ? "No hay órdenes registradas" : "Órdenes no disponibles"}
                detalle={datosDisponibles
                  ? "La API respondió sin órdenes para la empresa activa."
                  : "No fue posible consultar las órdenes en la API."}
              />
            )}
          </div>
        </section>

        <aside className="panel panel-agenda">
          <div className="titulo-panel">
            <div>
              <span className="sobrelinea">Próximamente</span>
              <h2>Agenda de hoy</h2>
            </div>
            <CalendarDays size={22} />
          </div>

          <EstadoVacio
            icono={CalendarDays}
            titulo="Agenda sin datos"
            detalle="La agenda todavía no dispone de un contrato HTTP persistido."
          />
        </aside>
      </div>

      <section className="panel decisiones">
        <div className="titulo-panel">
          <div>
            <span className="sobrelinea">Requieren acción</span>
            <h2>Decisiones pendientes</h2>
          </div>
          <span className="contador">{ordenesPorAprobar.length}</span>
        </div>
        <div className="rejilla-decisiones">
          {ordenesPorAprobar.map((orden) => (
            <button key={orden.id} onClick={() => alAbrirOrden(orden)}>
              <span className="icono-decision ambar"><ClipboardCheck size={22} /></span>
              <span><strong>{orden.numero}</strong><small>Esperando aprobación de {orden.cliente}</small></span>
              <ChevronRight size={19} />
            </button>
          ))}
          {!cargando && ordenesPorAprobar.length === 0 && (
            <EstadoVacio
              icono={CircleCheck}
              titulo="Sin decisiones pendientes"
              detalle="No hay órdenes esperando aprobación."
            />
          )}
        </div>
      </section>
    </>
  );
}

function TarjetaMetrica({
  etiqueta,
  valor,
  detalle,
  icono: Icono,
  tono,
}: {
  etiqueta: string;
  valor: string;
  detalle: string;
  icono: LucideIcon;
  tono: string;
}) {
  return (
    <article className="tarjeta-metrica">
      <span className={`icono-metrica ${tono}`}><Icono size={22} /></span>
      <div><span>{etiqueta}</span><strong>{valor}</strong><small>{detalle}</small></div>
    </article>
  );
}

function VistaOrdenes({
  ordenes,
  cargando,
  datosDisponibles,
  filtro,
  alFiltrar,
  alCrearOrden,
  alAbrirOrden,
}: {
  ordenes: OrdenTaller[];
  cargando: boolean;
  datosDisponibles: boolean;
  filtro: string;
  alFiltrar: (filtro: string) => void;
  alCrearOrden: () => void;
  alAbrirOrden: (orden: OrdenTaller) => void;
}) {
  const filtros = ["Todas", "Recepción", "Diagnóstico", "Por aprobar", "Reparación", "Lista para entregar"];
  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Operación</span><h1>Órdenes de servicio</h1><p>Sigue cada vehículo desde la recepción hasta la entrega.</p></div>
        <button className="boton-primario" onClick={alCrearOrden}><Plus size={21} />Nueva orden</button>
      </section>
      <div className="filtros" role="group" aria-label="Filtrar órdenes por estado">
        {filtros.map((item) => <button key={item} className={filtro === item ? "activo" : ""} onClick={() => alFiltrar(item)}>{item}</button>)}
      </div>
      <section className="panel tabla-ordenes">
        <div className="cabecera-tabla"><span>Orden y vehículo</span><span>Cliente</span><span>Estado</span><span>Responsable</span><span /></div>
        {ordenes.map((orden) => (
          <button className="fila-tabla" key={orden.id} onClick={() => alAbrirOrden(orden)}>
            <span className="celda-vehiculo"><span className="mini-auto"><CarFront size={21} /></span><span><strong>{orden.vehiculo}</strong><small>{orden.numero} · {orden.placa}</small></span></span>
            <span className="cliente-tabla"><strong>{orden.cliente}</strong><small>{orden.motivo}</small></span>
            <span><span className={`etiqueta-estado estado-${normalizarClase(orden.estado)}`}>{orden.estado}</span></span>
            <span><strong>{orden.tecnico}</strong><small>Desde {orden.hora}</small></span>
            <ChevronRight size={20} />
          </button>
        ))}
        {!cargando && ordenes.length === 0 && (
          <EstadoVacio
            icono={Search}
            titulo={datosDisponibles ? "No hay órdenes para mostrar" : "Órdenes no disponibles"}
            detalle={datosDisponibles
              ? "La API no devolvió registros o ningún registro coincide con el filtro."
              : "No fue posible consultar las órdenes en la API."}
          />
        )}
      </section>
    </>
  );
}

function VistaClientes({
  clientes,
  cargando,
  datosDisponibles,
  alCrearOrden,
}: {
  clientes: ClienteTaller[];
  cargando: boolean;
  datosDisponibles: boolean;
  alCrearOrden: () => void;
}) {
  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Relaciones</span><h1>Clientes</h1><p>Información de contacto y vehículos en un solo lugar.</p></div>
        <button className="boton-primario"><Plus size={21} />Nuevo cliente</button>
      </section>
      <section className="rejilla-clientes">
        {clientes.map((cliente) => (
          <article className="tarjeta-cliente" key={cliente.nombre}>
            <div className="cliente-superior"><span className="avatar avatar-grande">{cliente.iniciales}</span><button className="boton-icono" aria-label={`Más opciones para ${cliente.nombre}`}><MoreHorizontal size={21} /></button></div>
            <h3>{cliente.nombre}</h3><p>{cliente.telefono}</p>
            <div className="datos-cliente"><span><CarFront size={17} />{cliente.cantidadVehiculos} vehículos</span><span><ClipboardList size={17} />{cliente.ordenActiva || "Sin orden activa"}</span></div>
            <button className="boton-secundario boton-ancho" onClick={alCrearOrden}>Crear orden</button>
          </article>
        ))}
        {!cargando && clientes.length === 0 && (
          <EstadoVacio
            icono={Users}
            titulo={datosDisponibles ? "No hay clientes registrados" : "Clientes no disponibles"}
            detalle={datosDisponibles
              ? "La API respondió sin clientes para la empresa activa."
              : "No fue posible consultar los clientes en la API."}
          />
        )}
      </section>
    </>
  );
}

function VistaVehiculos({
  vehiculos,
  cargando,
  datosDisponibles,
}: {
  vehiculos: VehiculoTaller[];
  cargando: boolean;
  datosDisponibles: boolean;
}) {
  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Parque vehicular</span><h1>Vehículos</h1><p>Historial y situación actual de cada unidad.</p></div>
        <button className="boton-primario"><Plus size={21} />Nuevo vehículo</button>
      </section>
      <section className="panel lista-vehiculos">
        {vehiculos.map((vehiculo) => (
          <button key={vehiculo.placa}>
            <span className="icono-vehiculo"><CarFront size={25} /></span>
            <span><strong>{vehiculo.nombre}</strong><small>{vehiculo.detalle}</small></span>
            <span><strong>{vehiculo.placa}</strong><small>{vehiculo.cliente}</small></span>
            <span className={`estado-vehiculo ${vehiculo.activo ? "listo" : ""}`}>{vehiculo.activo ? "Registrado" : "Inactivo"}</span>
            <ChevronRight size={20} />
          </button>
        ))}
        {!cargando && vehiculos.length === 0 && (
          <EstadoVacio
            icono={CarFront}
            titulo={datosDisponibles ? "No hay vehículos registrados" : "Vehículos no disponibles"}
            detalle={datosDisponibles
              ? "La API respondió sin vehículos para la empresa activa."
              : "No fue posible consultar los vehículos en la API."}
          />
        )}
      </section>
    </>
  );
}

function VistaInventario() {
  return (
    <>
      <section className="encabezado-pagina">
        <div><span className="sobrelinea">Repuestos y consumibles</span><h1>Inventario</h1><p>Disponibilidad rápida para decidir sin salir de la orden.</p></div>
        <button className="boton-primario"><Plus size={21} />Registrar entrada</button>
      </section>
      <section className="panel inventario">
        <EstadoVacio
          icono={Boxes}
          titulo="Inventario sin datos"
          detalle="El inventario todavía no dispone de un contrato HTTP persistido."
        />
      </section>
    </>
  );
}

function EstadoVacio({
  icono: Icono,
  titulo,
  detalle,
}: {
  icono: LucideIcon;
  titulo: string;
  detalle: string;
}) {
  return (
    <div className="estado-vacio">
      <Icono size={32} />
      <h3>{titulo}</h3>
      <p>{detalle}</p>
    </div>
  );
}

function PaginaProceso({
  titulo,
  descripcion,
  etiquetaRegreso = "Volver",
  alRegresar,
  children,
}: {
  titulo: string;
  descripcion: string;
  etiquetaRegreso?: string;
  alRegresar: () => void;
  children: React.ReactNode;
}) {
  return (
    <section className="pagina-proceso" aria-labelledby="titulo-pagina-proceso">
      <header className="encabezado-proceso">
        <button className="boton-regresar" type="button" onClick={alRegresar}>
          <ArrowLeft size={21} />
          <span>{etiquetaRegreso}</span>
        </button>
        <div>
          <span className="sobrelinea">Área de trabajo</span>
          <h1 id="titulo-pagina-proceso">{titulo}</h1>
          <p>{descripcion}</p>
        </div>
        <span className="indicador-guardado"><CircleCheck size={17} />Guardado automático</span>
      </header>
      <div className="cuerpo-proceso">{children}</div>
    </section>
  );
}

function FormularioNuevaOrden({
  clientes,
  vehiculos,
  guardando,
  alEnviar,
}: {
  clientes: ClienteTaller[];
  vehiculos: VehiculoTaller[];
  guardando: boolean;
  alEnviar: (evento: FormEvent<HTMLFormElement>) => void;
}) {
  const [clienteId, setClienteId] = useState(clientes[0]?.id ?? 0);
  const vehiculosCliente = vehiculos.filter((vehiculo) => vehiculo.clienteId === clienteId);
  const formularioDisponible = clientes.length > 0 && vehiculosCliente.length > 0;

  return (
    <form className="formulario" onSubmit={alEnviar}>
      <div className="paso-formulario"><span>1</span><div><strong>Cliente y vehículo</strong><small>Selecciona a quién vamos a atender</small></div></div>
      <label>
        Cliente
        <select
          name="clienteId"
          required
          value={clienteId || ""}
          onChange={(evento) => setClienteId(Number(evento.target.value))}
        >
          {clientes.length === 0 && <option value="">No hay clientes registrados</option>}
          {clientes.map((cliente) => <option key={cliente.id} value={cliente.id}>{cliente.nombre}</option>)}
        </select>
      </label>
      <label>
        Vehículo
        <select name="vehiculoId" required disabled={!formularioDisponible}>
          {vehiculosCliente.length === 0 && <option value="">El cliente no tiene vehículos</option>}
          {vehiculosCliente.map((vehiculo) => (
            <option key={vehiculo.id} value={vehiculo.id}>{vehiculo.nombre} · {vehiculo.placa}</option>
          ))}
        </select>
      </label>
      <div className="separador-formulario" />
      <div className="paso-formulario"><span>2</span><div><strong>Motivo de visita</strong><small>Describe lo que reporta el cliente</small></div></div>
      <label>Descripción<textarea name="motivo" rows={4} required placeholder="Ej. Se escucha un ruido al frenar..." /></label>
      <div className="opciones-prioridad"><label><input type="radio" name="prioridad" defaultChecked />Normal</label><label><input type="radio" name="prioridad" />Prioritaria</label></div>
      <button className="boton-primario boton-ancho" type="submit" disabled={!formularioDisponible || guardando}>
        <Check size={20} />{guardando ? "Creando orden…" : "Crear orden de servicio"}
      </button>
    </form>
  );
}

function DetalleOrden({
  orden,
  inspeccion,
  alRecibir,
  alNotificar,
}: {
  orden: OrdenTaller;
  inspeccion?: InspeccionVisual;
  alRecibir: () => void;
  alNotificar: () => void;
}) {
  return (
    <div className="detalle-panel detalle-orden-pagina">
      <div className="columna-orden">
        <div className="vehiculo-destacado"><span className="icono-auto-grande"><CarFront size={31} /></span><div><span className={`etiqueta-estado estado-${normalizarClase(orden.estado)}`}>{orden.estado}</span><h3>{orden.vehiculo}</h3><p>{orden.placa} · {orden.cliente}</p></div></div>
        <div className="bloque-detalle"><span className="sobrelinea">Motivo de ingreso</span><p>{orden.motivo}</p></div>
        <div className="datos-rapidos"><div><UserRound size={20} /><span><small>Responsable</small><strong>{orden.tecnico}</strong></span></div><div><Clock3 size={20} /><span><small>Ingreso</small><strong>{orden.hora} a. m.</strong></span></div></div>
        <div className="bloque-detalle"><span className="sobrelinea">Avance de la orden</span><div className="barra-avance"><span style={{ width: `${orden.progreso}%` }} /></div><strong>{orden.progreso}% completado</strong></div>
        <div className="linea-tiempo"><div className="completo"><span><Check size={14} /></span><div><strong>Orden creada</strong><small>Datos del cliente y vehículo confirmados</small></div></div><div className={orden.estado !== "Recepción" ? "completo" : "actual"}><span>{orden.estado !== "Recepción" ? <Check size={14} /> : ""}</span><div><strong>Recepción del vehículo</strong><small>Inspección visual y evidencias</small></div></div><div><span /><div><strong>Diagnóstico técnico</strong><small>Pendiente de resultados</small></div></div></div>
        <button className="boton-secundario boton-ancho" onClick={alNotificar}><MessageCircleMore size={20} />Enviar actualización al cliente</button>
      </div>

      <div className="columna-inspeccion-orden">
        {orden.estado !== "Recepción" ? (
          <ResumenInspeccion inspeccion={inspeccion} alEditar={alRecibir} />
        ) : (
          <section className="llamada-recepcion">
            <span className="icono-llamada"><ClipboardCheck size={28} /></span>
            <span className="sobrelinea">Siguiente paso</span>
            <h2>Recibe e inspecciona el vehículo</h2>
            <p>Registra kilometraje, combustible, fotografías y daños exteriores usando toda la pantalla de la tablet.</p>
            <button className="boton-primario boton-ancho" onClick={alRecibir}><ClipboardCheck size={20} />Iniciar recepción</button>
          </section>
        )}
      </div>
    </div>
  );
}

function FormularioRecepcion({
  orden,
  inspeccionInicial,
  alEnviar,
}: {
  orden: OrdenTaller;
  inspeccionInicial?: InspeccionVisual;
  alEnviar: (evento: FormEvent<HTMLFormElement>) => void;
}) {
  const [tipoDanio, setTipoDanio] = useState<TipoDanio>("Rayón");
  const [severidad, setSeveridad] = useState<SeveridadDanio>("Leve");
  const [danios, setDanios] = useState<DanioVisual[]>(inspeccionInicial?.danios || []);
  const [danioEditandoId, setDanioEditandoId] = useState<string | null>(null);
  const [cantidadFotos, setCantidadFotos] = useState(0);
  const danioEditando = danios.find((danio) => danio.id === danioEditandoId);

  function marcarZona(zona: ZonaVehiculo) {
    const id = `${zona}-${tipoDanio}-${Date.now()}`;
    setDanios((actuales) => [
      ...actuales,
      {
        id,
        zona,
        tipo: tipoDanio,
        severidad,
        observacion: "",
      },
    ]);
    setDanioEditandoId(id);
  }

  function actualizarDanio(cambios: Partial<DanioVisual>) {
    if (!danioEditandoId) return;
    setDanios((actuales) =>
      actuales.map((danio) =>
        danio.id === danioEditandoId ? { ...danio, ...cambios } : danio,
      ),
    );
  }

  return (
    <form className="formulario formulario-recepcion-pagina" onSubmit={alEnviar}>
      <div className="resumen-recepcion"><span className="icono-auto-grande"><CarFront size={28} /></span><div><strong>{orden.vehiculo}</strong><small>{orden.placa} · {orden.cliente}</small></div></div>
      <div className="fila-formulario"><label>Kilometraje<input type="number" name="kilometraje" min="0" placeholder="85,240" defaultValue={inspeccionInicial?.kilometraje || ""} required /></label><label><span className="etiqueta-con-icono"><Fuel size={17} />Combustible</span><select name="combustible" defaultValue={String(inspeccionInicial?.porcentajeCombustible || 50)}><option value="25">¼ de tanque</option><option value="50">½ tanque</option><option value="75">¾ de tanque</option><option value="100">Tanque lleno</option></select></label></div>
      <section className="inspeccion-visual">
        <div className="encabezado-inspeccion">
          <div>
            <span className="sobrelinea">Inspección visual</span>
            <h3>Marca los daños sobre el vehículo</h3>
            <p>Elige el tipo, la severidad y toca la zona afectada.</p>
          </div>
          <span className="contador-danios">{danios.length}</span>
        </div>

        <div className="selector-inspeccion">
          <span>Tipo de daño</span>
          <div className="chips-inspeccion">
            {(["Rayón", "Abolladura", "Golpe", "Vidrio", "Luz"] as TipoDanio[]).map(
              (tipo) => (
                <button
                  key={tipo}
                  type="button"
                  className={tipoDanio === tipo ? "activo" : ""}
                  onClick={() => setTipoDanio(tipo)}
                >
                  {tipo}
                </button>
              ),
            )}
          </div>
        </div>

        <div className="selector-inspeccion selector-severidad">
          <span>Severidad</span>
          <div className="chips-inspeccion">
            {(["Leve", "Moderado", "Severo"] as SeveridadDanio[]).map((nivel) => (
              <button
                key={nivel}
                type="button"
                className={`${severidad === nivel ? "activo" : ""} nivel-${normalizarClase(nivel)}`}
                onClick={() => setSeveridad(nivel)}
              >
                {nivel}
              </button>
            ))}
          </div>
        </div>

        <MapaInspeccion danios={danios} alMarcar={marcarZona} />

        {danioEditando && (
          <div className="editor-danio">
            <div className="encabezado-editor-danio">
              <div>
                <span className="sobrelinea">Editar hallazgo</span>
                <strong>{etiquetaZona(danioEditando.zona)}</strong>
              </div>
              <button type="button" onClick={() => setDanioEditandoId(null)} aria-label="Cerrar editor">
                <X size={18} />
              </button>
            </div>
            <div className="fila-formulario">
              <label>
                Tipo de daño
                <select
                  value={danioEditando.tipo}
                  onChange={(evento) => actualizarDanio({ tipo: evento.target.value as TipoDanio })}
                >
                  {(["Rayón", "Abolladura", "Golpe", "Vidrio", "Luz"] as TipoDanio[]).map((tipo) => (
                    <option key={tipo}>{tipo}</option>
                  ))}
                </select>
              </label>
              <label>
                Severidad
                <select
                  value={danioEditando.severidad}
                  onChange={(evento) => actualizarDanio({ severidad: evento.target.value as SeveridadDanio })}
                >
                  {(["Leve", "Moderado", "Severo"] as SeveridadDanio[]).map((nivel) => (
                    <option key={nivel}>{nivel}</option>
                  ))}
                </select>
              </label>
            </div>
            <label>
              Descripción u observación
              <textarea
                rows={3}
                maxLength={500}
                value={danioEditando.observacion}
                onChange={(evento) => actualizarDanio({ observacion: evento.target.value })}
                placeholder="Ej. Rayón superficial de 12 cm en la puerta trasera..."
              />
              <small>{danioEditando.observacion.length}/500 caracteres</small>
            </label>
            <button className="boton-secundario boton-ancho" type="button" onClick={() => setDanioEditandoId(null)}>
              <Check size={18} /> Listo
            </button>
          </div>
        )}

        {danios.length > 0 ? (
          <div className="danios-registrados">
            <div className="titulo-danios">
              <strong>Daños registrados</strong>
              <button type="button" onClick={() => { setDanios([]); setDanioEditandoId(null); }}>
                <RotateCcw size={15} /> Limpiar
              </button>
            </div>
            {danios.map((danio, indice) => (
              <div className="fila-danio" key={danio.id}>
                <span className={`numero-danio nivel-${normalizarClase(danio.severidad)}`}>
                  {indice + 1}
                </span>
                <span>
                  <strong>{danio.tipo} · {etiquetaZona(danio.zona)}</strong>
                  <small>{danio.observacion || `Severidad ${danio.severidad.toLocaleLowerCase("es")} · Sin observación`}</small>
                </span>
                <span className="acciones-danio">
                  <button
                    type="button"
                    aria-label={`Editar ${danio.tipo} en ${etiquetaZona(danio.zona)}`}
                    onClick={() => setDanioEditandoId(danio.id)}
                  >
                    <Pencil size={16} />
                  </button>
                  <button
                    type="button"
                    aria-label={`Eliminar ${danio.tipo} en ${etiquetaZona(danio.zona)}`}
                    onClick={() => {
                      setDanios((actuales) => actuales.filter((item) => item.id !== danio.id));
                      if (danioEditandoId === danio.id) setDanioEditandoId(null);
                    }}
                  >
                    <Trash2 size={16} />
                  </button>
                </span>
              </div>
            ))}
          </div>
        ) : (
          <div className="sin-danios"><Check size={18} />Aún no se marcaron daños visibles</div>
        )}
      </section>

      <input type="hidden" name="danios" value={JSON.stringify(danios)} />
      <label>Observaciones generales<textarea name="estado" rows={4} required defaultValue={inspeccionInicial?.descripcionEstado || ""} placeholder="Describe el estado interior, accesorios o cualquier detalle adicional..." /></label>
      <label className="zona-fotos">
        <input
          type="file"
          accept="image/*"
          capture="environment"
          multiple
          onChange={(evento) => setCantidadFotos(evento.target.files?.length || 0)}
        />
        {cantidadFotos > 0 ? <ImagePlus size={25} /> : <Camera size={25} />}
        <span>
          <strong>{cantidadFotos > 0 ? `${cantidadFotos} fotografías seleccionadas` : "Agregar fotografías"}</strong>
          <small>Frente, laterales y evidencia de cada daño</small>
        </span>
        <Plus size={20} />
      </label>
      <div className="lista-comprobacion"><label><input type="checkbox" name="dejaLlaves" defaultChecked={inspeccionInicial?.dejaLlaves} />Deja llaves</label><label><input type="checkbox" name="dejaDocumentos" defaultChecked={inspeccionInicial?.dejaDocumentos} />Deja documentos</label><label><input type="checkbox" name="aceptaPruebaRuta" />Acepta prueba de ruta</label></div>
      <button className="boton-primario boton-ancho" type="submit"><Sparkles size={20} />{inspeccionInicial ? "Guardar cambios de inspección" : "Guardar y pasar a diagnóstico"}</button>
    </form>
  );
}

function MapaInspeccion({
  danios,
  alMarcar,
  compacto = false,
}: {
  danios: DanioVisual[];
  alMarcar?: (zona: ZonaVehiculo) => void;
  compacto?: boolean;
}) {
  return (
    <div className={`mapa-inspeccion ${compacto ? "mapa-compacto" : ""}`}>
      <div className="orientacion-vehiculo"><span />Frente del vehículo<span /></div>
      <div className="silueta-vehiculo" aria-label="Esquema superior del vehículo">
        <span className="rueda rueda-1" />
        <span className="rueda rueda-2" />
        <span className="rueda rueda-3" />
        <span className="rueda rueda-4" />
        <span className="carroceria">
          <span className="parabrisas-esquema" />
          <span className="techo-esquema" />
          <span className="vidrio-trasero-esquema" />
        </span>
        {zonasVehiculo.map((zona) => {
          const daniosZona = danios.filter((danio) => danio.zona === zona.id);
          const severidadMayor = obtenerSeveridadMayor(daniosZona);
          return (
            <button
              key={zona.id}
              type="button"
              disabled={!alMarcar}
              className={`marca-zona zona-${zona.id} ${daniosZona.length ? "con-danio" : ""} ${severidadMayor ? `nivel-${normalizarClase(severidadMayor)}` : ""}`}
              aria-label={`${zona.etiqueta}${daniosZona.length ? `, ${daniosZona.length} daños` : ""}`}
              onClick={() => alMarcar?.(zona.id)}
            >
              {daniosZona.length > 0 ? daniosZona.length : <Plus size={14} />}
              <span>{zona.etiqueta}</span>
            </button>
          );
        })}
      </div>
      {!compacto && <p className="ayuda-mapa">Toca una zona para colocar un marcador</p>}
    </div>
  );
}

function ResumenInspeccion({
  inspeccion,
  alEditar,
}: {
  inspeccion?: InspeccionVisual;
  alEditar: () => void;
}) {
  const danios = inspeccion?.danios || [];
  return (
    <section className="resumen-inspeccion">
      <div className="encabezado-inspeccion">
        <div>
          <span className="sobrelinea">Recepción documentada</span>
          <h3>Inspección visual del vehículo</h3>
          <p>{danios.length > 0 ? `${danios.length} hallazgos registrados al ingresar` : "Sin daños exteriores reportados"}</p>
        </div>
        <span className={`contador-danios ${danios.length === 0 ? "sin-hallazgos" : ""}`}>{danios.length}</span>
      </div>
      {inspeccion && (
        <div className="datos-inspeccion">
          <span><small>Kilometraje</small><strong>{inspeccion.kilometraje.toLocaleString("es-NI")} km</strong></span>
          <span><small>Combustible</small><strong>{inspeccion.porcentajeCombustible}%</strong></span>
          <span><small>Elementos</small><strong>{inspeccion.dejaLlaves ? "Con llaves" : "Sin llaves"}</strong></span>
        </div>
      )}
      <MapaInspeccion danios={danios} compacto />
      {danios.length > 0 && (
        <div className="leyenda-inspeccion">
          {danios.map((danio, indice) => (
            <article key={danio.id}>
              <i className={`nivel-${normalizarClase(danio.severidad)}`}>{indice + 1}</i>
              <span>
                <strong>{danio.tipo} en {etiquetaZona(danio.zona)}</strong>
                <small>{danio.observacion || "Sin observación específica"}</small>
              </span>
            </article>
          ))}
        </div>
      )}
      <div className="observacion-inspeccion">
        <span className="sobrelinea">Observaciones generales</span>
        <p>{inspeccion?.descripcionEstado || "No se registraron observaciones generales durante la recepción."}</p>
      </div>
      <button className="boton-secundario boton-ancho" type="button" onClick={alEditar}>
        <Pencil size={18} /> Editar inspección y observaciones
      </button>
    </section>
  );
}

function etiquetaZona(zona: ZonaVehiculo) {
  return zonasVehiculo.find((item) => item.id === zona)?.etiqueta || zona;
}

function obtenerSeveridadMayor(danios: DanioVisual[]): SeveridadDanio | null {
  if (danios.some((danio) => danio.severidad === "Severo")) return "Severo";
  if (danios.some((danio) => danio.severidad === "Moderado")) return "Moderado";
  if (danios.some((danio) => danio.severidad === "Leve")) return "Leve";
  return null;
}

function normalizarClase(valor: string) {
  return valor
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLocaleLowerCase("es")
    .replaceAll(" ", "-");
}

function obtenerDireccionApi() {
  const direccionApi = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "");
  if (!direccionApi) {
    throw new Error("No se configuró NEXT_PUBLIC_API_URL para consultar la API.");
  }
  return direccionApi;
}

async function cargarDatosApi(senal: AbortSignal): Promise<{
  ordenes: OrdenTaller[];
  clientes: ClienteTaller[];
  vehiculos: VehiculoTaller[];
}> {
  const [ordenes, clientesApi, vehiculosApi] = await Promise.all([
    cargarOrdenesApi(senal),
    cargarClientesApi(senal),
    cargarVehiculosApi(senal),
  ]);

  const clientes = clientesApi.map((cliente) => ({
    id: cliente.id,
    iniciales: obtenerIniciales(cliente.nombre),
    nombre: cliente.nombre,
    telefono: cliente.telefono,
    cantidadVehiculos: vehiculosApi.filter((vehiculo) => vehiculo.clienteId === cliente.id).length,
    ordenActiva:
      ordenes.find(
        (orden) =>
          orden.clienteId === cliente.id && orden.estado !== "Lista para entregar",
      )?.numero ?? null,
  }));
  const vehiculos = vehiculosApi.map((vehiculo) => ({
    id: vehiculo.id,
    clienteId: vehiculo.clienteId,
    placa: vehiculo.placa,
    nombre: `${vehiculo.marca} ${vehiculo.modelo}`,
    detalle: `${vehiculo.anio} · ${vehiculo.color || "Color no registrado"}`,
    cliente: vehiculo.nombreCliente,
    activo: vehiculo.activo,
  }));

  return { ordenes, clientes, vehiculos };
}

async function cargarOrdenesApi(senal: AbortSignal): Promise<OrdenTaller[]> {
  const direccionApi = obtenerDireccionApi();

  const respuesta = await fetch(`${direccionApi}/api/ordenes-servicio`, {
    headers: {
      "X-Empresa-Id": process.env.NEXT_PUBLIC_EMPRESA_ID || "1",
    },
    signal: senal,
  });
  if (!respuesta.ok) throw new Error("No fue posible consultar las órdenes.");

  const ordenesApi = (await respuesta.json()) as OrdenServicioApi[];
  return ordenesApi.map(convertirOrdenApi);
}

async function cargarClientesApi(senal: AbortSignal): Promise<ClienteApi[]> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/clientes`, {
    headers: { "X-Empresa-Id": process.env.NEXT_PUBLIC_EMPRESA_ID || "1" },
    signal: senal,
  });
  if (!respuesta.ok) throw new Error("No fue posible consultar los clientes.");
  return (await respuesta.json()) as ClienteApi[];
}

async function cargarVehiculosApi(senal: AbortSignal): Promise<VehiculoApi[]> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/vehiculos`, {
    headers: { "X-Empresa-Id": process.env.NEXT_PUBLIC_EMPRESA_ID || "1" },
    signal: senal,
  });
  if (!respuesta.ok) throw new Error("No fue posible consultar los vehículos.");
  return (await respuesta.json()) as VehiculoApi[];
}

async function crearOrdenApi(solicitud: {
  clienteId: number;
  vehiculoId: number;
  observaciones: string;
}): Promise<OrdenTaller> {
  const respuesta = await fetch(`${obtenerDireccionApi()}/api/ordenes-servicio`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Empresa-Id": process.env.NEXT_PUBLIC_EMPRESA_ID || "1",
    },
    body: JSON.stringify(solicitud),
  });
  if (!respuesta.ok) throw new Error("No fue posible crear la orden en la API.");
  return convertirOrdenApi((await respuesta.json()) as OrdenServicioApi);
}

function convertirOrdenApi(orden: OrdenServicioApi): OrdenTaller {
  const estado = convertirEstadoApi(orden.estado);
  return {
    id: orden.id,
    clienteId: orden.clienteId,
    vehiculoId: orden.vehiculoId,
    numero: orden.numero,
    cliente: orden.nombreCliente,
    vehiculo: `Vehículo · ${orden.placaVehiculo}`,
    placa: orden.placaVehiculo,
    estado,
    motivo: orden.observaciones || "Sin observaciones registradas",
    tecnico: "Por asignar",
    hora: new Date(orden.fechaIngreso).toLocaleTimeString("es-NI", {
      hour: "numeric",
      minute: "2-digit",
    }),
    progreso: progresoPorEstado(estado),
    color: colorPorEstado(estado),
  };
}

function obtenerIniciales(nombre: string) {
  return nombre
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((parte) => parte[0]?.toLocaleUpperCase("es") ?? "")
    .join("");
}

async function guardarRecepcionApi(
  ordenServicioId: number,
  inspeccion: InspeccionVisual,
  esActualizacion: boolean,
) {
  const direccionApi = obtenerDireccionApi();

  const respuesta = await fetch(
    `${direccionApi}/api/ordenes-servicio/${ordenServicioId}/recepcion`,
    {
      method: esActualizacion ? "PUT" : "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Empresa-Id": process.env.NEXT_PUBLIC_EMPRESA_ID || "1",
      },
      body: JSON.stringify({
        kilometraje: inspeccion.kilometraje,
        porcentajeCombustible: inspeccion.porcentajeCombustible,
        descripcionEstado: inspeccion.descripcionEstado,
        dejaLlaves: inspeccion.dejaLlaves,
        dejaDocumentos: inspeccion.dejaDocumentos,
        danios: inspeccion.danios.map((danio) => ({
          zona: zonaParaApi(danio.zona),
          tipo: danio.tipo === "Rayón" ? "Rayon" : danio.tipo,
          severidad: danio.severidad,
          observacion: danio.observacion.trim() || null,
        })),
      }),
    },
  );

  if (!respuesta.ok) throw new Error("No fue posible registrar la recepción.");
}

async function cargarInspeccionApi(
  ordenServicioId: number,
  senal: AbortSignal,
): Promise<InspeccionVisual | null> {
  const direccionApi = obtenerDireccionApi();

  const respuesta = await fetch(
    `${direccionApi}/api/ordenes-servicio/${ordenServicioId}/recepcion`,
    {
      headers: { "X-Empresa-Id": process.env.NEXT_PUBLIC_EMPRESA_ID || "1" },
      signal: senal,
    },
  );
  if (!respuesta.ok) throw new Error("No fue posible consultar la inspección.");

  const recepcion = (await respuesta.json()) as RecepcionVehiculoApi;
  return {
    kilometraje: recepcion.kilometraje,
    porcentajeCombustible: recepcion.porcentajeCombustible,
    descripcionEstado: recepcion.descripcionEstado,
    dejaLlaves: recepcion.dejaLlaves,
    dejaDocumentos: recepcion.dejaDocumentos,
    danios: recepcion.danios.map((danio, indice) => ({
      id: `api-${ordenServicioId}-${indice}`,
      zona: zonaDesdeApi(danio.zona),
      tipo: danio.tipo === "Rayon" ? "Rayón" : danio.tipo as TipoDanio,
      severidad: danio.severidad as SeveridadDanio,
      observacion: danio.observacion || "",
    })),
  };
}

function zonaParaApi(zona: ZonaVehiculo) {
  const zonas: Record<ZonaVehiculo, string> = {
    frente: "Frente",
    capo: "Capo",
    parabrisas: "Parabrisas",
    techo: "Techo",
    "lateral-izquierdo": "LateralIzquierdo",
    "lateral-derecho": "LateralDerecho",
    maletero: "Maletero",
    posterior: "Posterior",
  };
  return zonas[zona];
}

function zonaDesdeApi(zona: string): ZonaVehiculo {
  const zonas: Record<string, ZonaVehiculo> = {
    Frente: "frente",
    Capo: "capo",
    Parabrisas: "parabrisas",
    Techo: "techo",
    LateralIzquierdo: "lateral-izquierdo",
    LateralDerecho: "lateral-derecho",
    Maletero: "maletero",
    Posterior: "posterior",
  };
  return zonas[zona] || "frente";
}

function convertirEstadoApi(estado: string): EstadoOrden {
  const equivalencias: Record<string, EstadoOrden> = {
    Recepcion: "Recepción",
    Diagnostico: "Diagnóstico",
    Cotizacion: "Cotización",
    PendienteAprobacion: "Por aprobar",
    PreparacionReparacion: "Reparación",
    Reparacion: "Reparación",
    ControlCalidad: "Control de calidad",
    ListaParaEntrega: "Lista para entregar",
    Entregada: "Lista para entregar",
    Cerrada: "Lista para entregar",
  };
  return equivalencias[estado] || "Recepción";
}

function progresoPorEstado(estado: EstadoOrden) {
  const progresos: Record<EstadoOrden, number> = {
    "Recepción": 10,
    "Diagnóstico": 28,
    "Cotización": 42,
    "Por aprobar": 48,
    "Reparación": 68,
    "Control de calidad": 88,
    "Lista para entregar": 100,
  };
  return progresos[estado];
}

function colorPorEstado(estado: EstadoOrden) {
  const colores: Record<EstadoOrden, string> = {
    "Recepción": "#c66c3d",
    "Diagnóstico": "#1f7a5b",
    "Cotización": "#d78b31",
    "Por aprobar": "#d78b31",
    "Reparación": "#315e9d",
    "Control de calidad": "#7b5aa6",
    "Lista para entregar": "#238a70",
  };
  return colores[estado];
}
