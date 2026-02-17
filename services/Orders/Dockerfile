# ---------------------------------------------------------
# 1. ETAPA DE COMPILACIÓN (SDK 8.0)
# ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos los archivos de proyecto (.csproj) de cada capa.
# Hacemos esto primero para que Docker cachee las capas de dependencias (NuGet).
COPY ["Orders.API/Orders.API.csproj", "Orders.API/"]
COPY ["Orders.Application/Orders.Application.csproj", "Orders.Application/"]
COPY ["Orders.Domain/Orders.Domain.csproj", "Orders.Domain/"]
COPY ["Orders.Infrastructure/Orders.Infrastructure.csproj", "Orders.Infrastructure/"]

# Restauramos las dependencias de toda la solución a través del proyecto API
RUN dotnet restore "Orders.API/Orders.API.csproj"

# AHORA copiamos el resto del código fuente (Domain, Application, etc.)
COPY . .

# Nos movemos a la carpeta de la API y compilamos
WORKDIR "/src/Orders.API"
RUN dotnet build "Orders.API.csproj" -c Release -o /app/build

# ---------------------------------------------------------
# 2. ETAPA DE PUBLICACIÓN
# ---------------------------------------------------------
FROM build AS publish
RUN dotnet publish "Orders.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------------------------------------
# 3. ETAPA FINAL (RUNTIME 8.0)
# ---------------------------------------------------------
# Usamos 'alpine' para que la imagen sea súper ligera para tu demo
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# 1. Instalar dependencias para globalización
RUN apk add --no-cache icu-libs

# 2. Desactivar el modo invariante para que reconozca culturas
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Exponemos el puerto 8080 (el estándar de .NET 8 en Docker)
EXPOSE 8080

# Copiamos solo los archivos necesarios de la etapa de publicación
COPY --from=publish /app/publish .

# Comando para arrancar el microservicio
ENTRYPOINT ["dotnet", "Orders.API.dll"]