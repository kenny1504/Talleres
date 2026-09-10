import type { Metadata } from "next";
import "./globals.css";
import { ProveedorCargadorPantalla } from "./componentes/ProveedorCargadorPantalla";

export const metadata: Metadata = {
  title: "Taller Uno | Operación del taller",
  description: "Sistema de gestión de taller diseñado para trabajar cómodamente desde tablet.",
  openGraph: {
    title: "Taller Uno",
    description: "El taller, bajo control.",
    images: [{ url: "/og.png", width: 1680, height: 941, alt: "Taller Uno en una tablet" }],
  },
  twitter: {
    card: "summary_large_image",
    title: "Taller Uno",
    description: "El taller, bajo control.",
    images: ["/og.png"],
  },
  icons: {
    icon: "/favicon.svg",
    shortcut: "/favicon.svg",
  },
};

export default function DisposicionRaiz({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="es">
      <body><ProveedorCargadorPantalla>{children}</ProveedorCargadorPantalla></body>
    </html>
  );
}
