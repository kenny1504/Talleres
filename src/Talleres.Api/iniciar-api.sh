#!/bin/sh
set -eu

: "${APP_UID:?La imagen no definió APP_UID}"

directorio_claves=/home/app/.aspnet/DataProtection-Keys
mkdir -p "$directorio_claves"
chown -R "$APP_UID:$APP_UID" "$directorio_claves"

exec gosu "$APP_UID:$APP_UID" dotnet Talleres.Api.dll
