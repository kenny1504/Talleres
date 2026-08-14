interface ContextoRutaApi {
  params: Promise<{ ruta: string[] }>;
}

const nombresEncabezadosSolicitud = [
  "accept",
  "authorization",
  "content-type",
  "cookie",
  "x-empresa-id",
];

const nombresEncabezadosRespuesta = [
  "content-disposition",
  "content-type",
  "location",
];

async function reenviarSolicitudAsync(
  solicitud: Request,
  contexto: ContextoRutaApi,
): Promise<Response> {
  const direccionApi = process.env.TALLERES_API_INTERNA_URL?.replace(/\/$/, "");

  if (!direccionApi) {
    return Response.json(
      {
        title: "API no configurada",
        detail: "No se configuró la dirección interna de la API.",
      },
      { status: 503 },
    );
  }

  const { ruta } = await contexto.params;
  const rutaCodificada = ruta.map(encodeURIComponent).join("/");
  const direccionDestino = new URL(`${direccionApi}/${rutaCodificada}`);
  direccionDestino.search = new URL(solicitud.url).search;

  const encabezadosSolicitud = new Headers();
  for (const nombre of nombresEncabezadosSolicitud) {
    const valor = solicitud.headers.get(nombre);
    if (valor) encabezadosSolicitud.set(nombre, valor);
  }

  const tieneCuerpo = solicitud.method !== "GET" && solicitud.method !== "HEAD";

  try {
    const respuesta = await fetch(direccionDestino, {
      method: solicitud.method,
      headers: encabezadosSolicitud,
      body: tieneCuerpo ? await solicitud.arrayBuffer() : undefined,
      cache: "no-store",
      redirect: "manual",
      signal: solicitud.signal,
    });

    const encabezadosRespuesta = new Headers();
    for (const nombre of nombresEncabezadosRespuesta) {
      const valor = respuesta.headers.get(nombre);
      if (valor) encabezadosRespuesta.set(nombre, valor);
    }

    return new Response(respuesta.body, {
      status: respuesta.status,
      statusText: respuesta.statusText,
      headers: encabezadosRespuesta,
    });
  } catch (error) {
    console.error(
      "No fue posible contactar la API interna.",
      error instanceof Error ? error.message : "Error de conexión no identificado.",
    );

    return Response.json(
      {
        title: "API no disponible",
        detail: "El servicio de Talleres no está disponible temporalmente.",
      },
      { status: 502 },
    );
  }
}

export const DELETE = reenviarSolicitudAsync;
export const GET = reenviarSolicitudAsync;
export const PATCH = reenviarSolicitudAsync;
export const POST = reenviarSolicitudAsync;
export const PUT = reenviarSolicitudAsync;
