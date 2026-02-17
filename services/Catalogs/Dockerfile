# ---------------------------------------------------------
# 1. ETAPA DE COMPILACIÓN (SDK 8.0)
# ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos los archivos de proyecto (.csproj) de cada capa de Catalogs
COPY ["Catalogs.API/Catalogs.API.csproj", "Catalogs.API/"]
COPY ["Catalogs.Application/Catalogs.Application.csproj", "Catalogs.Application/"]
COPY ["Catalogs.Domain/Catalogs.Domain.csproj", "Catalogs.Domain/"]
COPY ["Catalogs.Infrastructure/Catalogs.Infrastructure.csproj", "Catalogs.Infrastructure/"]

# Restauramos las dependencias
RUN dotnet restore "Catalogs.API/Catalogs.API.csproj"

# Copiamos el resto del código fuente de Catalogs
COPY . .

# Compilamos el proyecto de la API
WORKDIR "/src/Catalogs.API"
RUN dotnet build "Catalogs.API.csproj" -c Release -o /app/build

# ---------------------------------------------------------
# 2. ETAPA DE PUBLICACIÓN
# ---------------------------------------------------------
FROM build AS publish
RUN dotnet publish "Catalogs.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------------------------------------
# 3. ETAPA FINAL (RUNTIME 8.0 ALPINE)
# ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Instalamos soporte para globalización (necesario para SQL Client en Alpine)
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Exponemos el puerto interno de .NET 8
EXPOSE 8080

# Copiamos los archivos publicados
COPY --from=publish /app/publish .

# Comando de arranque
ENTRYPOINT ["dotnet", "Catalogs.API.dll"]