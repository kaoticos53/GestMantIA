# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0.100-preview.4 AS build
WORKDIR /src

# Copiar los archivos de solución y restaurar dependencias
COPY ["GestMantIA.sln", "./"]
COPY ["src/GestMantIA.Core/GestMantIA.Core.csproj", "src/GestMantIA.Core/"]
COPY ["src/GestMantIA.Infrastructure/GestMantIA.Infrastructure.csproj", "src/GestMantIA.Infrastructure/"]
COPY ["src/GestMantIA.API/GestMantIA.API.csproj", "src/GestMantIA.API/"]

# Restaurar dependencias
RUN dotnet restore "src/GestMantIA.API/GestMantIA.API.csproj"

# Copiar el resto del código fuente
COPY . .

# Publicar la aplicación
WORKDIR "/src/src/GestMantIA.API"
RUN dotnet publish "GestMantIA.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa de tiempo de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:9.0.0-preview.4
WORKDIR /app

# Configuración de la zona horaria
ENV TZ=Europe/Madrid
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Copiar la aplicación publicada
COPY --from=build /app/publish .

# Puerto expuesto
EXPOSE 80
EXPOSE 443

# Comando de inicio
ENTRYPOINT ["dotnet", "GestMantIA.API.dll"]
