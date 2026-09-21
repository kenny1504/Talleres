import assert from "node:assert/strict";
import { access, readFile } from "node:fs/promises";
import { createServer } from "node:http";
import { once } from "node:events";
import test from "node:test";

async function renderizar() {
  const direccionWorker = new URL("../dist/server/index.js", import.meta.url);
  direccionWorker.searchParams.set("prueba", `${process.pid}-${Date.now()}`);
  const { default: worker } = await import(direccionWorker.href);

  return worker.fetch(
    new Request("http://localhost/", {
      headers: { accept: "text/html" },
    }),
    {
      ASSETS: {
        fetch: async () => new Response("No encontrado", { status: 404 }),
      },
    },
    {
      waitUntil() {},
      passThroughOnException() {},
    },
  );
}

test("representa el acceso protegido a Talleres en español", async () => {
  const respuesta = await renderizar();
  assert.equal(respuesta.status, 200);
  assert.match(respuesta.headers.get("content-type") ?? "", /^text\/html\b/i);

  const html = await respuesta.text();
  assert.match(html, /<html[^>]*\blang=["']es["']/i);
  assert.match(html, /<title>Taller Smart \| Operación del taller<\/title>/i);
  assert.match(html, /Preparando Talleres/i);
  assert.match(html, /href=["']\/icono-talleres\.png["']/i);
  assert.doesNotMatch(html, /codex-preview|Your site is taking shape|Building your site/i);
});

test("no sustituye la API con registros de demostración", async () => {
  const pagina = await readFile(new URL("../app/page.tsx", import.meta.url), "utf8");

  assert.match(pagina, /useState<OrdenTaller\[]>\(\[\]\)/);
  assert.match(pagina, /setOrdenes\(datos\.ordenes\)/);
  assert.match(pagina, /setErrorDatos\(/);
  assert.doesNotMatch(pagina, /ordenesIniciales|inspeccionesIniciales|demostración local/);
  assert.doesNotMatch(pagina, /Ana Martínez|Carlos Herrera|OT-2039|M 347-891/);
});

test("conserva el flujo de inspección en página completa y adaptable", async () => {
  const [pagina, estilos, disposicion, paquete] = await Promise.all([
    readFile(new URL("../app/page.tsx", import.meta.url), "utf8"),
    readFile(new URL("../app/globals.css", import.meta.url), "utf8"),
    readFile(new URL("../app/layout.tsx", import.meta.url), "utf8"),
    readFile(new URL("../package.json", import.meta.url), "utf8"),
  ]);

  assert.match(pagina, /function PaginaProceso\(/);
  assert.match(pagina, /className="pagina-proceso"/);
  assert.match(pagina, /Guardar cambios de inspección/);
  assert.match(pagina, /Editar inspección y observaciones/);
  assert.match(pagina, /Descripción u observación/);
  assert.match(pagina, /Observaciones generales/);
  assert.match(estilos, /\.formulario-recepcion-pagina\s*\{[^}]*grid-template-columns:\s*minmax/s);
  assert.match(estilos, /@media \(max-width:\s*900px\)/);
  assert.match(estilos, /\.detalle-orden-pagina\s*\{\s*grid-template-columns:\s*1fr/s);
  assert.match(estilos, /@media \(prefers-reduced-motion:\s*reduce\)/);
  assert.match(disposicion, /title:\s*"Taller Smart \| Operación del taller"/);
  assert.doesNotMatch(pagina, /_sites-preview|SkeletonPreview/);
  assert.doesNotMatch(paquete, /react-loading-skeleton|drizzle-(orm|kit)/);

  await access(new URL("../public/icono-talleres.png", import.meta.url));
  await access(new URL("../public/og.png", import.meta.url));
});

test("mantiene el proxy interno de Docker fuera del código cliente", async () => {
  const [proxy, servidor] = await Promise.all([
    readFile(new URL("../app/backend/[...ruta]/route.ts", import.meta.url), "utf8"),
    readFile(new URL("../servidor.mjs", import.meta.url), "utf8"),
  ]);

  assert.match(proxy, /process\.env\.TALLERES_API_INTERNA_URL/);
  assert.match(proxy, /headers\.getSetCookie\(\)/);
  assert.match(proxy, /append\("set-cookie", cookie\)/);
  assert.doesNotMatch(proxy, /x-empresa-id/i);
  assert.match(proxy, /cache:\s*"no-store"/);
  assert.match(proxy, /"x-forwarded-host"/);
  assert.match(proxy, /"x-forwarded-proto"/);
  assert.doesNotMatch(proxy, /http:\/\/api:8080/);
  assert.match(servidor, /startProdServer/);
  assert.match(servidor, /host:\s*anfitrion/);
});

test("el proxy conserva por separado todas las cookies de autenticación", async () => {
  const cookies = [
    "Talleres.Externo=identidad; Path=/backend; HttpOnly; Secure; SameSite=Lax",
    ".AspNetCore.Correlation.Google=; Path=/backend; Expires=Thu, 01 Jan 1970 00:00:00 GMT; Secure; SameSite=None",
  ];
  const servidorApi = createServer((solicitud, respuesta) => {
    assert.equal(solicitud.headers["x-forwarded-host"], "talleres.example.com");
    assert.equal(solicitud.headers["x-forwarded-proto"], "https");
    respuesta.writeHead(302, {
      location: "https://talleres.example.com/",
      "set-cookie": cookies,
    });
    respuesta.end();
  });

  servidorApi.listen(0, "127.0.0.1");
  await once(servidorApi, "listening");

  try {
    const direccion = servidorApi.address();
    assert.ok(direccion && typeof direccion !== "string");
    process.env.TALLERES_API_INTERNA_URL = `http://127.0.0.1:${direccion.port}`;

    const direccionWorker = new URL("../dist/server/index.js", import.meta.url);
    direccionWorker.searchParams.set("prueba-proxy", `${process.pid}-${Date.now()}`);
    const { default: worker } = await import(direccionWorker.href);
    const respuesta = await worker.fetch(
      new Request("https://talleres.example.com/backend/api/autenticacion/externo/google/callback"),
      {
        ASSETS: {
          fetch: async () => new Response("No encontrado", { status: 404 }),
        },
      },
      {
        waitUntil() {},
        passThroughOnException() {},
      },
    );

    assert.equal(respuesta.status, 302);
    assert.deepEqual(respuesta.headers.getSetCookie(), cookies);
  } finally {
    delete process.env.TALLERES_API_INTERNA_URL;
    servidorApi.close();
    await once(servidorApi, "close");
  }
});

test("el login usa la identidad de NOVA y no acepta la empresa desde el navegador", async () => {
  const pagina = await readFile(new URL("../app/page.tsx", import.meta.url), "utf8");

  assert.match(pagina, /function PantallaInicioSesion/);
  assert.match(pagina, /SMART TPV NOVA/);
  assert.match(pagina, /Todo tu taller/);
  assert.match(pagina, /Gestiona órdenes, clientes, vehículos e inventario/);
  assert.match(pagina, /aria-label=\{mostrarContrasena \? "Ocultar contraseña" : "Mostrar contraseña"\}/);
  assert.doesNotMatch(pagina, /Información aislada por empresa/);
  assert.doesNotMatch(pagina, /El sistema abrirá únicamente el taller que tienes autorizado/);
  assert.match(pagina, /api\/autenticacion\/iniciar/);
  assert.match(pagina, /api\/autenticacion\/seleccionar-taller/);
  assert.match(pagina, /if \(!sesion\) \{\s*return <PantallaInicioSesion/);
  assert.match(pagina, /function VistaAdministracion/);
  assert.match(pagina, /sesion\.esSuperUsuario && \(/);
  assert.match(pagina, /api\/talleres-sincronizados/);
  assert.match(pagina, /credentials:\s*"include"/);
  assert.doesNotMatch(pagina, /NEXT_PUBLIC_EMPRESA_ID|X-Empresa-Id/);
});

test("la navegación global cierra cualquier proceso activo", async () => {
  const pagina = await readFile(new URL("../app/page.tsx", import.meta.url), "utf8");

  const inicioNavegar = pagina.indexOf("function navegar(nuevaVista: Vista)");
  const finNavegar = pagina.indexOf("\n  function buscar", inicioNavegar);
  const cuerpoNavegar = pagina.slice(inicioNavegar, finNavegar);

  assert.notEqual(inicioNavegar, -1);
  assert.match(cuerpoNavegar, /setMostrarNuevaOrden\(false\)/);
  assert.match(cuerpoNavegar, /setOrdenDetalle\(null\)/);
  assert.match(cuerpoNavegar, /setMostrarRecepcion\(false\)/);
  assert.match(pagina, /onChange=\{\(evento\) => buscar\(evento\.target\.value\)\}/);
  assert.match(pagina, /<NavegacionInferior vista=\{vista\} alNavegar=\{navegar\} \/>/);
  assert.doesNotMatch(pagina, /!procesoActivo && <NavegacionInferior/);
});

test("muestra el logo del taller en la marca y simplifica su información en el inicio", async () => {
  const pagina = await readFile(new URL("../app/page.tsx", import.meta.url), "utf8");
  const estilos = await readFile(new URL("../app/globals.css", import.meta.url), "utf8");

  assert.match(pagina, /logoTaller=\{sesion\.taller\.logo\}/);
  assert.match(pagina, /function LogoTaller\(/);
  assert.match(pagina, /<img\s+src=\{logo\}/);
  assert.doesNotMatch(pagina, /<strong>Horario<\/strong>/);
  assert.doesNotMatch(pagina, /Abrir logo registrado/);
  assert.match(estilos, /\.marca-simbolo img\s*\{[^}]*object-fit:\s*contain/s);
  assert.match(pagina, /taller\.nombreComercial && taller\.nombreLegal !== taller\.nombreComercial/);
  assert.match(estilos, /@media \(max-width:\s*620px\)[\s\S]*\.datos-taller\s*\{[^}]*grid-template-columns:\s*repeat\(2,/s);
  assert.match(estilos, /\.dato-taller-contacto,[\s\S]*\.dato-taller-ancho\s*\{[^}]*grid-column:\s*1 \/ -1/s);
});

test("crear una orden desde clientes conserva el cliente y el regreso", async () => {
  const pagina = await readFile(new URL("../app/page.tsx", import.meta.url), "utf8");

  assert.match(pagina, /alCrearOrden=\{iniciarNuevaOrden\}/);
  assert.match(pagina, /alCrearOrden\(cliente\.id\)/);
  assert.match(pagina, /clienteInicialId && clientes\.some/);
  assert.match(pagina, /`Volver a \$\{navegacion\.find/);
});

test("nueva orden registra clientes y vehículos sin abandonar el formulario", async () => {
  const [pagina, estilos] = await Promise.all([
    readFile(new URL("../app/page.tsx", import.meta.url), "utf8"),
    readFile(new URL("../app/globals.css", import.meta.url), "utf8"),
  ]);

  assert.match(pagina, /aria-label="Registrar nuevo cliente"/);
  assert.match(pagina, /aria-label="Registrar nuevo vehículo"/);
  assert.match(pagina, /aria-haspopup="dialog"/);
  assert.match(pagina, /role="dialog"/);
  assert.match(pagina, /aria-modal="true"/);
  assert.match(pagina, /alRegistrarCliente=\{registrarClienteDesdeOrden\}/);
  assert.match(pagina, /alRegistrarVehiculo=\{registrarVehiculoDesdeOrden\}/);
  assert.match(pagina, /setClienteId\(clienteGuardado\.id\)/);
  assert.match(pagina, /setVehiculoId\(vehiculoGuardado\.id\)/);
  assert.match(pagina, /Al guardar quedará seleccionado automáticamente/);
  assert.match(estilos, /\.campo-con-accion\s*\{[^}]*grid-template-columns:\s*minmax\(0, 1fr\) auto/s);
  assert.match(estilos, /\.boton-alta-rapida\s*\{[^}]*width:\s*44px;[^}]*height:\s*44px/s);
  assert.match(estilos, /\.fondo-modal-alta\s*\{[^}]*position:\s*fixed/s);
  assert.match(estilos, /@media \(max-width:\s*680px\)[\s\S]*\.modal-alta\s*\{[^}]*max-height:\s*calc\(100dvh - 16px\)/s);
});

test("la descripción de una orden admite dictado por voz y edición manual", async () => {
  const [pagina, estilos] = await Promise.all([
    readFile(new URL("../app/page.tsx", import.meta.url), "utf8"),
    readFile(new URL("../app/globals.css", import.meta.url), "utf8"),
  ]);

  assert.match(pagina, /webkitSpeechRecognition/);
  assert.match(pagina, /reconocimiento\.lang = "es-NI"/);
  assert.match(pagina, /aria-pressed=\{dictando\}/);
  assert.match(pagina, /Dictar descripción/);
  assert.match(pagina, /Detener dictado/);
  assert.match(pagina, /value=\{motivo\}/);
  assert.match(pagina, /onChange=\{\(evento\) => setMotivo\(evento\.target\.value\)\}/);
  assert.match(pagina, /Permite el acceso al micrófono/);
  assert.match(estilos, /\.boton-dictado\s*\{[^}]*min-height:\s*44px/s);
});

test("alta y edición de clientes usan un formulario completo y persisten mediante la API", async () => {
  const pagina = await readFile(new URL("../app/page.tsx", import.meta.url), "utf8");

  assert.match(pagina, /onClick=\{alCrearCliente\}><Plus[^>]*\/>Nuevo cliente/);
  assert.match(pagina, /function FormularioCliente\(/);
  assert.match(pagina, /name="nombre"/);
  assert.match(pagina, /name="telefono"/);
  assert.match(pagina, /Nombre completo <span aria-hidden="true">\*<\/span>/);
  assert.match(pagina, /Teléfono <span aria-hidden="true">\*<\/span>/);
  assert.match(pagina, /name="direccion"/);
  assert.match(pagina, /name="activo"/);
  assert.match(pagina, /method: clienteId \? "PUT" : "POST"/);
  assert.match(pagina, /onClick=\{\(\) => alEditarCliente\(cliente\)\}/);
  assert.match(pagina, /setClientes\(\(actuales\) =>/);
  assert.match(pagina, /obtenerMensajeErrorApi/);
});

test("vehículos permite registrar y actualizar mediante el contrato HTTP", async () => {
  const pagina = await readFile(new URL("../app/page.tsx", import.meta.url), "utf8");

  assert.match(pagina, /onClick=\{alCrear\}><Plus[^>]*\/>Nuevo vehículo/);
  assert.match(pagina, /aria-label=\{`Editar vehículo \$\{vehiculo\.placa\}`\}/);
  assert.match(pagina, /function FormularioVehiculo\(/);
  assert.match(pagina, /name="clienteId"/);
  assert.match(pagina, /name="placa"/);
  assert.match(pagina, /value=\{marcaVehiculoId \|\| ""\}/);
  assert.match(pagina, /name="modeloVehiculoId"/);
  assert.match(pagina, /modelo\.marcaVehiculoId === marcaVehiculoId/);
  assert.match(pagina, /api\/catalogos-vehiculos\/marcas/);
  assert.match(pagina, /api\/catalogos-vehiculos\/modelos/);
  assert.match(pagina, /name="anio"/);
  assert.match(pagina, /name="numeroVin"/);
  assert.match(pagina, /method: vehiculoId \? "PUT" : "POST"/);
  assert.match(pagina, /setVehiculos\(\(actuales\) =>/);
  assert.match(pagina, /vehiculo\.id === vehiculoGuardado\.id/);
});

test("la inspección carga y muestra evidencias públicas de Amazon S3", async () => {
  const [pagina, estilos] = await Promise.all([
    readFile(new URL("../app/page.tsx", import.meta.url), "utf8"),
    readFile(new URL("../app/globals.css", import.meta.url), "utf8"),
  ]);

  assert.match(pagina, /name="fotografias"/);
  assert.match(pagina, /image\/jpeg,image\/png,image\/webp/);
  assert.match(pagina, /recepcion\/evidencias/);
  assert.match(pagina, /function direccionEvidencia/);
  assert.match(pagina, /<ResumenInspeccion\s+ordenServicioId=\{orden\.id\}/);
  assert.match(pagina, /function ResumenInspeccion\(\{\s*ordenServicioId,/);
  assert.match(pagina, /direccionEvidencia\(ordenServicioId, evidencia\.id\)/);
  assert.match(pagina, /Evidencia fotográfica/);
  assert.match(pagina, /disabled=\{guardando\}/);
  assert.match(estilos, /\.galeria-evidencias\s*\{/);
  assert.match(estilos, /\.galeria-evidencias img\s*\{[^}]*object-fit:\s*cover/s);
});

test("confirma en un modal la eliminación de fotografías guardadas", async () => {
  const pagina = await readFile(new URL("../app/page.tsx", import.meta.url), "utf8");
  const estilos = await readFile(new URL("../app/globals.css", import.meta.url), "utf8");

  assert.match(pagina, /method:\s*"DELETE"/);
  assert.match(pagina, /titulo="Eliminar fotografía"/);
  assert.match(pagina, /alConfirmar=\{eliminarEvidenciaConfirmada\}/);
  assert.match(pagina, /alEliminarEvidencia/);
  assert.match(estilos, /\.boton-eliminar-evidencia/);
  assert.match(estilos, /min-height:\s*44px/);
});

test("permite eliminar productos descontados y comunica la devolución al inventario", async () => {
  const pagina = await readFile(new URL("../app/page.tsx", import.meta.url), "utf8");

  assert.match(pagina, /!soloLectura \? <button[^>]+onClick=\{\(\) => alEliminar\(detalle\)\}/);
  assert.match(pagina, /detallePorEliminar\.tipo === "Inventario"/);
  assert.match(pagina, /detallePorEliminar\.existenciaDescontada/);
  assert.match(pagina, /<ModalConfirmacion/);
  assert.match(pagina, /role="alertdialog"/);
  assert.match(pagina, /Devolviendo el producto al inventario…/);
  assert.match(pagina, /Producto eliminado y existencia devuelta al inventario/);
  assert.doesNotMatch(pagina, /window\.(?:alert|confirm)\s*\(/);
});

test("lleva la orden hasta entrega y registra productos y cargos durante reparación", async () => {
  const [pagina, estilos] = await Promise.all([
    readFile(new URL("../app/page.tsx", import.meta.url), "utf8"),
    readFile(new URL("../app/globals.css", import.meta.url), "utf8"),
  ]);

  assert.match(pagina, /Solicitar aprobación por WhatsApp/);
  assert.match(pagina, /Iniciar reparación/);
  assert.match(pagina, /Marcar lista para entregar/);
  assert.match(pagina, /Agregar desde inventario/);
  assert.match(pagina, /Escribe al menos 2 caracteres para buscar un producto/);
  assert.match(pagina, /terminoBusquedaProducto, controlador\.signal/);
  assert.match(pagina, /Cantidad para \{articuloSeleccionado\.nombre\}/);
  assert.match(pagina, /productosCoincidentes = articulos\.slice\(0, 6\)/);
  assert.match(pagina, /Cargo manual/);
  assert.match(pagina, /Añadir cargo manual/);
  assert.match(pagina, /titulo-modal-cargo-manual/);
  assert.match(pagina, /Guarda cada cargo para añadir otro sin cerrar esta ventana/);
  assert.match(pagina, /setMensajeCargoManual\(null\); setMostrarCargoManual\(false\)/);
  assert.match(pagina, /mensajeCargoManual && <div className=\{mensajeCargoManual\.esError/);
  assert.match(pagina, /setMensajeCargoManual\(\{ texto: "Cargo manual agregado a la orden\."/);
  assert.match(pagina, /mensaje-modal-cargo mensaje-modal-cargo-exito/);
  assert.match(pagina, /window\.setTimeout\(\(\) => \{[\s\S]*setMensajeCargoManual[\s\S]*\}, 4000\)/);
  assert.match(pagina, /window\.clearTimeout\(temporizador\)/);
  assert.match(pagina, /aria-live="polite"/);
  assert.doesNotMatch(pagina, /name="unidadMedida"/);
  assert.match(pagina, /Total de la orden/);
  assert.match(pagina, /existencia insuficiente/);
  assert.match(pagina, /const detallesEditables = orden\.estado === "Reparación" \|\| orden\.estado === "Lista para entregar"/);
  assert.match(pagina, /Puedes corregir el diagnóstico, las evidencias, los productos y los cargos antes de entregarlo/);
  assert.match(pagina, /api\/ordenes-servicio\/\$\{ordenServicioId\}\/detalles\/inventario/);
  assert.match(estilos, /\.formularios-cargos\s*\{[^}]*grid-template-columns:\s*minmax\(0, 1\.25fr\)/s);
  assert.match(estilos, /\.formulario-cargo input,[\s\S]*min-height:\s*46px/s);
  assert.match(estilos, /\.producto-seleccionado-inventario\s*\{/);
  assert.match(estilos, /\.modal-cargo-manual\s*\{/);
  assert.match(estilos, /\.mensaje-modal-cargo\s*\{[^}]*position:\s*sticky;[^}]*top:\s*0;[^}]*z-index:\s*2/s);
  assert.match(estilos, /\.mensaje-modal-cargo-exito\s*\{[^}]*background:\s*var\(--verde-claro\);[^}]*color:\s*var\(--verde-profundo\)/s);
  assert.match(estilos, /\.aviso\s*\{[^}]*z-index:\s*130/s);
  assert.match(estilos, /@media \(max-width:\s*680px\)[\s\S]*\.formularios-cargos\s*\{\s*grid-template-columns:\s*1fr/s);
});

test("registra, dicta y comparte el diagnóstico mediante un enlace público", async () => {
  const [pagina, paginaPublica, estilos] = await Promise.all([
    readFile(new URL("../app/page.tsx", import.meta.url), "utf8"),
    readFile(new URL("../app/orden/[token]/page.tsx", import.meta.url), "utf8"),
    readFile(new URL("../app/globals.css", import.meta.url), "utf8"),
  ]);

  assert.match(pagina, /Dictar diagnóstico/);
  assert.match(pagina, /Evidencia fotográfica/);
  assert.match(pagina, /Hallazgos, piezas afectadas y evidencia del diagnóstico/);
  assert.match(pagina, /8 - \(diagnostico\?\.evidencias\.length \?\? 0\)/);
  assert.match(pagina, /const acumuladas = \[\.\.\.fotografias, \.\.\.seleccionadas\]/);
  assert.match(pagina, /sincronizarFotografiasEntrada\(evento\.target, acumuladas\)/);
  assert.match(pagina, /vistasPrevias\.map/);
  assert.match(pagina, /wa\.me/);
  assert.match(pagina, /normalizarTelefonoWhatsapp/);
  assert.match(pagina, /telefono\.replace\(\/\\D\/g, ""\)/);
  assert.match(pagina, /digitos\.length === 8/);
  assert.match(pagina, /Abrir WhatsApp/);
  assert.match(pagina, /Puede revisarlo y autorizar el trabajo/);
  assert.match(pagina, /Diagnóstico finalizado y preparado para aprobación del cliente/);
  assert.match(pagina, /cliente=\{clientes\.find\(\(cliente\) => cliente\.id === ordenDetalle\.clienteId\)\}/);
  assert.doesNotMatch(pagina, /https:\/\/wa\.me\/\?text=/);
  assert.match(pagina, /guardarDiagnosticoOrdenApi/);
  assert.match(paginaPublica, /Autorizar continuar/);
  assert.match(paginaPublica, /Estado actual:/);
  assert.match(paginaPublica, /etiquetaEstado\(orden\.estado\)/);
  assert.match(paginaPublica, /api\/publico\/ordenes-servicio/);
  assert.match(paginaPublica, /Inspección del vehículo/);
  assert.match(paginaPublica, /Fotografías de la inspección/);
  assert.match(paginaPublica, /const daniosInspeccion = orden\.daniosInspeccion \?\? \[\]/);
  assert.match(paginaPublica, /daniosInspeccion\.map/);
  assert.match(paginaPublica, /direccionEvidenciaInspeccion/);
  assert.match(paginaPublica, /const taller = orden\.taller \?\?/);
  assert.match(paginaPublica, /Logo de \$\{taller\.nombre\}/);
  assert.match(paginaPublica, /taller\.direccion/);
  assert.match(paginaPublica, /taller\.telefono/);
  assert.match(paginaPublica, /Sistema desarrollado por/);
  assert.match(paginaPublica, /https:\/\/www\.ksoftech\.com\//);
  assert.match(estilos, /\.formulario-diagnostico/);
  assert.match(estilos, /\.campo-diagnostico textarea\s*\{[^}]*background:\s*var\(--panel\)/s);
  assert.match(estilos, /\.campo-diagnostico textarea:focus/);
  assert.match(estilos, /\.pagina-publica-orden/);
  assert.match(estilos, /\.vehiculo-publico[^}]*background:\s*var\(--verde-profundo\)/s);
  assert.match(estilos, /\.tarjeta-publica[^}]*background:\s*var\(--panel\)/s);
  assert.match(estilos, /\.identidad-taller-publica[^}]*grid-template-columns:/s);
});

test("acumula selecciones consecutivas de fotografías en diagnóstico e inspección", async () => {
  const pagina = await readFile(new URL("../app/page.tsx", import.meta.url), "utf8");

  assert.equal(
    pagina.match(/const acumuladas = \[\.\.\.fotografias, \.\.\.seleccionadas\]/g)?.length,
    2,
  );
  assert.equal(
    pagina.match(/sincronizarFotografiasEntrada\(evento\.target, acumuladas\)/g)?.length,
    2,
  );
  assert.match(pagina, /function sincronizarFotografiasEntrada\(entrada: HTMLInputElement, fotografias: File\[\]\)/);
  assert.match(pagina, /entrada\.files = transferencia\.files/);
  assert.equal(
    pagina.match(/aria-label=\{`Quitar \$\{vista\.nombre\}`\}/g)?.length,
    2,
  );
  assert.equal(
    pagina.match(/fotografias\.filter\(\(_, posicion\) => posicion !== indice\)/g)?.length,
    2,
  );
});

test("usa un cargador global accesible para las operaciones críticas", async () => {
  const [pagina, disposicion, cargador, estilos] = await Promise.all([
    readFile(new URL("../app/page.tsx", import.meta.url), "utf8"),
    readFile(new URL("../app/layout.tsx", import.meta.url), "utf8"),
    readFile(new URL("../app/componentes/ProveedorCargadorPantalla.tsx", import.meta.url), "utf8"),
    readFile(new URL("../app/globals.css", import.meta.url), "utf8"),
  ]);

  assert.match(disposicion, /<ProveedorCargadorPantalla>\{children\}<\/ProveedorCargadorPantalla>/);
  assert.match(cargador, /role="status"/);
  assert.match(cargador, /aria-live="assertive"/);
  assert.match(cargador, /inert=\{cargando \? true : undefined\}/);
  assert.match(cargador, /finally\s*\{\s*finalizarCargaPantalla\(id\)/s);
  assert.match(estilos, /\.cargador-pantalla\s*\{[^}]*position:\s*fixed[^}]*inset:\s*0[^}]*z-index:\s*1000/s);
  assert.match(estilos, /env\(safe-area-inset-bottom\)/);
  assert.match(pagina, /ejecutarConCargadorPantalla\(\s*"Creando la orden de servicio…"/s);
  assert.match(pagina, /"Guardando la recepción e inspección…"/);
  assert.doesNotMatch(pagina, /<Cargador\b/);
});
