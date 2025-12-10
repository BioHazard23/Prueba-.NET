# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/TalentoPlus.API/TalentoPlus.API.csproj", "TalentoPlus.API/"]
COPY ["src/TalentoPlus.Application/TalentoPlus.Application.csproj", "TalentoPlus.Application/"]
COPY ["src/TalentoPlus.Infrastructure/TalentoPlus.Infrastructure.csproj", "TalentoPlus.Infrastructure/"]
COPY ["src/TalentoPlus.Domain/TalentoPlus.Domain.csproj", "TalentoPlus.Domain/"]

# Restaurar dependencias
RUN dotnet restore "TalentoPlus.API/TalentoPlus.API.csproj"

# Copiar todo el código fuente
COPY src/ .

# Compilar
WORKDIR "/src/TalentoPlus.API"
RUN dotnet build "TalentoPlus.API.csproj" -c Release -o /app/build

# Publicar
FROM build AS publish
RUN dotnet publish "TalentoPlus.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TalentoPlus.API.dll"]

