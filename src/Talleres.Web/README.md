# Talleres.Web

Frontal web tablet-first del sistema Talleres. Está construido con React, TypeScript, vinext y Vite, y consume la API ASP.NET Core del repositorio.

## Requisitos

- Node.js 22.13 o superior.
- La API disponible en la dirección indicada por `NEXT_PUBLIC_API_URL` para trabajar con datos persistidos.

## Desarrollo local

```powershell
Copy-Item .env.example .env.local
npm ci
npm run dev
```

La dirección local predeterminada es `http://localhost:4173`. Sin una API configurada,
la interfaz muestra un error explícito y no sustituye la respuesta con datos simulados.

Para levantar el frontal junto con la API, use `docker compose up --build` desde la raíz del repositorio. La cadena de la base SQL Server remota se configura en el `.env` de la raíz y no forma parte de Docker. Dentro de los contenedores, el navegador consume `/backend` y el servidor web reenvía las solicitudes a la API mediante la red interna de Compose.

## Variables

| Variable | Uso |
|---|---|
| `NEXT_PUBLIC_API_URL` | Dirección pública de la API ASP.NET Core. |
| `NEXT_PUBLIC_EMPRESA_ID` | Empresa de desarrollo enviada mediante `X-Empresa-Id`; no reemplaza autenticación. |

No colocar secretos en variables `NEXT_PUBLIC_*`, porque sus valores quedan disponibles en el navegador.

## Verificación

```powershell
npm run lint
npm test
```

`npm test` ejecuta la compilación y las pruebas de la salida generada. `package-lock.json` se versiona y debe mantenerse sincronizado con `package.json`.

## Estructura que debe conservarse

- `app/`: interfaz y estilos del producto.
- `public/`: recursos estáticos usados por la interfaz y sus metadatos.
- `worker/`: entrada compatible con Cloudflare Workers requerida por vinext.
- `build/sites-vite-plugin.ts`: preparación de la salida de despliegue.
- `servidor.mjs`: servidor de producción liviano usado por la imagen Docker.
- `.openai/hosting.json`: declaración de recursos de Sites; no contiene credenciales.

Las decisiones obligatorias de arquitectura, seguridad, inspección visual y experiencia tablet se encuentran en el [`AGENTS.md`](../../AGENTS.md) de la raíz.
