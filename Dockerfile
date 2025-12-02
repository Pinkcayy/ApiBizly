# ============================================
# STAGE 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivo de proyecto primero (para cache de dependencias)
COPY ["ApiBizly.csproj", "./"]

# Restaurar dependencias
RUN dotnet restore "ApiBizly.csproj" --verbosity normal

# Copiar todo el código fuente
COPY . .

# Compilar el proyecto
WORKDIR "/src"
RUN dotnet build "ApiBizly.csproj" -c Release -o /app/build

# Publicar la aplicación
RUN dotnet publish "ApiBizly.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# STAGE 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copiar archivos publicados
COPY --from=build /app/publish .

# Variables de entorno para Render
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Exponer puerto (Render lo maneja automáticamente via PORT)
EXPOSE 8080

# Punto de entrada
ENTRYPOINT ["dotnet", "ApiBizly.dll"]
