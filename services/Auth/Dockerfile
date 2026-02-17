# ---------------------------------------------------------
# 1. ETAPA DE COMPILACIÓN (SDK 8.0)
# ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos los archivos de proyecto (.csproj) de cada capa de Auth
COPY ["Auth.API/Auth.API.csproj", "Auth.API/"]
COPY ["Auth.Application/Auth.Application.csproj", "Auth.Application/"]
COPY ["Auth.Domain/Auth.Domain.csproj", "Auth.Domain/"]
COPY ["Auth.Infrastructure/Auth.Infrastructure.csproj", "Auth.Infrastructure/"]

# Restauramos las dependencias
RUN dotnet restore "Auth.API/Auth.API.csproj"

# Copiamos el resto del código fuente del microservicio Auth
COPY . .

# Compilamos el proyecto de la API
WORKDIR "/src/Auth.API"
RUN dotnet build "Auth.API.csproj" -c Release -o /app/build

# ---------------------------------------------------------
# 2. ETAPA DE PUBLICACIÓN
# ---------------------------------------------------------
FROM build AS publish
RUN dotnet publish "Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------------------------------------
# 3. ETAPA FINAL (RUNTIME 8.0 ALPINE)
# ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Instalamos soporte para globalización (indispensable para validaciones de tokens y DB)
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Puerto estándar de .NET 8
EXPOSE 8080

# Copiamos los archivos publicados
COPY --from=publish /app/publish .

# Comando de arranque
ENTRYPOINT ["dotnet", "Auth.API.dll"]