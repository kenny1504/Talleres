import assert from "node:assert/strict";
import { access, readFile } from "node:fs/promises";
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

test("representa el sistema Taller Uno en español", async () => {
  const respuesta = await renderizar();
  assert.equal(respuesta.status, 200);
  assert.match(respuesta.headers.get("content-type") ?? "", /^text\/html\b/i);

  const html = await respuesta.text();
  assert.match(html, /<html[^>]*\blang=["']es["']/i);
  assert.match(html, /<title>Taller Uno \| Operación del taller<\/title>/i);
  assert.match(html, /Buen día, Javier/i);
  assert.match(html, /En taller/i);
  assert.match(html, /href=["']\/favicon\.svg["']/i);
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
  assert.match(disposicion, /title:\s*"Taller Uno \| Operación del taller"/);
  assert.doesNotMatch(pagina, /_sites-preview|SkeletonPreview/);
  assert.doesNotMatch(paquete, /react-loading-skeleton|drizzle-(orm|kit)/);

  await access(new URL("../public/favicon.svg", import.meta.url));
  await access(new URL("../public/og.png", import.meta.url));
});

test("mantiene el proxy interno de Docker fuera del código cliente", async () => {
  const [proxy, servidor] = await Promise.all([
    readFile(new URL("../app/backend/[...ruta]/route.ts", import.meta.url), "utf8"),
    readFile(new URL("../servidor.mjs", import.meta.url), "utf8"),
  ]);

  assert.match(proxy, /process\.env\.TALLERES_API_INTERNA_URL/);
  assert.match(proxy, /"x-empresa-id"/);
  assert.match(proxy, /cache:\s*"no-store"/);
  assert.doesNotMatch(proxy, /http:\/\/api:8080/);
  assert.match(servidor, /startProdServer/);
  assert.match(servidor, /host:\s*anfitrion/);
});
