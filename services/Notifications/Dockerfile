FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Notifications.API/Notifications.API.csproj", "Notifications.API/"]
RUN dotnet restore "Notifications.API/Notifications.API.csproj"
COPY . .
RUN dotnet publish "Notifications.API/Notifications.API.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notifications.API.dll"]
