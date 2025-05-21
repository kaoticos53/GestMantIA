# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0.100-preview.4 AS build
WORKDIR /src

# Copiar los archivos de solución y restaurar dependencias
COPY ["GestMantIA.sln", "./"]
COPY ["src/GestMantIA.Core/GestMantIA.Core.csproj", "src/GestMantIA.Core/"]
COPY ["src/GestMantIA.Web/GestMantIA.Web.csproj", "src/GestMantIA.Web/"]

# Restaurar dependencias
RUN dotnet restore "src/GestMantIA.Web/GestMantIA.Web.csproj"

# Copiar el resto del código fuente
COPY . .

# Publicar la aplicación
WORKDIR "/src/src/GestMantIA.Web"
RUN dotnet publish "GestMantIA.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa de tiempo de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:9.0.0-preview.4 AS runtime
WORKDIR /app

# Configuración de la zona horaria
ENV TZ=Europe/Madrid
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Instalar herramientas necesarias para la generación de certificados
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copiar la aplicación publicada
COPY --from=build /app/publish .

# Puerto expuesto
EXPOSE 80
EXPOSE 443

# Comando de inicio
ENTRYPOINT ["dotnet", "GestMantIA.Web.dll"]
