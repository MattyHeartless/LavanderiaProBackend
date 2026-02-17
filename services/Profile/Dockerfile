# ---------------------------------------------------------
# 1. ETAPA DE COMPILACIÓN (SDK 8.0)
# ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos los archivos de proyecto (.csproj) de cada capa de Profile
COPY ["Profile.API/Profile.API.csproj", "Profile.API/"]
COPY ["Profile.Application/Profile.Application.csproj", "Profile.Application/"]
COPY ["Profile.Domain/Profile.Domain.csproj", "Profile.Domain/"]
COPY ["Profile.Infrastructure/Profile.Infrastructure.csproj", "Profile.Infrastructure/"]

# Restauramos las dependencias
RUN dotnet restore "Profile.API/Profile.API.csproj"

# Copiamos el resto del código fuente del microservicio Profile
COPY . .

# Compilamos el proyecto de la API
WORKDIR "/src/Profile.API"
RUN dotnet build "Profile.API.csproj" -c Release -o /app/build

# ---------------------------------------------------------
# 2. ETAPA DE PUBLICACIÓN
# ---------------------------------------------------------
FROM build AS publish
RUN dotnet publish "Profile.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------------------------------------
# 3. ETAPA FINAL (RUNTIME 8.0 ALPINE)
# ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Instalamos soporte para globalización (necesario para SQL Client en Alpine)
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Puerto estándar de .NET 8
EXPOSE 8080

# Copiamos los archivos publicados desde la etapa anterior
COPY --from=publish /app/publish .

# Punto de entrada para ejecutar la API
ENTRYPOINT ["dotnet", "Profile.API.dll"]