FROM mcr.microsoft.com/dotnet/sdk:9.0.301 AS compilacion
WORKDIR /codigo

COPY Directory.Build.props global.json ./
COPY src/Talleres.Dominio/Talleres.Dominio.csproj src/Talleres.Dominio/
COPY src/Talleres.Aplicacion/Talleres.Aplicacion.csproj src/Talleres.Aplicacion/
COPY src/Talleres.Infraestructura/Talleres.Infraestructura.csproj src/Talleres.Infraestructura/
COPY src/Talleres.Api/Talleres.Api.csproj src/Talleres.Api/
RUN dotnet restore src/Talleres.Api/Talleres.Api.csproj

COPY src/ src/
RUN dotnet publish src/Talleres.Api/Talleres.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /aplicacion/publicada \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /aplicacion

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=compilacion /aplicacion/publicada .
USER $APP_UID

ENTRYPOINT ["dotnet", "Talleres.Api.dll"]
