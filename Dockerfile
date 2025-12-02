# Usar la imagen base de .NET 9.0 SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el archivo del proyecto y restaurar dependencias
COPY ["ApiBizly.csproj", "./"]
RUN dotnet restore "ApiBizly.csproj"

# Copiar el resto de los archivos y compilar
COPY . .
WORKDIR "/src"
RUN dotnet build "ApiBizly.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "ApiBizly.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final de runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copiar los archivos publicados
COPY --from=publish /app/publish .

# Establecer el punto de entrada
ENTRYPOINT ["dotnet", "ApiBizly.dll"]

