import { resolve } from "node:path";
import { startProdServer } from "vinext/server/prod-server";

const puerto = Number.parseInt(process.env.PORT ?? "3000", 10);
const anfitrion = process.env.HOST ?? "0.0.0.0";

if (!Number.isInteger(puerto) || puerto <= 0 || puerto > 65_535) {
  throw new Error("La variable PORT debe contener un puerto válido.");
}

await startProdServer({
  port: puerto,
  host: anfitrion,
  outDir: resolve("dist"),
});
