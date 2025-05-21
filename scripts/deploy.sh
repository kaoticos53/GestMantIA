#!/bin/bash
set -e

# Colores para la salida
echo_ok() { echo -e "\033[0;32m$1\033[0m"; }
echo_info() { echo -e "\033[0;34m$1\033[0m"; }
echo_error() { echo -e "\033[0;31m$1\033[0m"; }

# Verificar que se proporcione un entorno
if [ -z "$1" ]; then
    echo_error "Uso: $0 <entorno> [versión]"
    echo_info "Entornos disponibles: staging, production"
    exit 1
fi

ENTORNO=$1
VERSION=${2:-latest}
DEPLOY_DIR="/opt/gestmantia"
DOCKER_COMPOSE_FILE="docker-compose.yml"

# Validar entorno
case "$ENTORNO" in
    staging)
        DOCKER_COMPOSE_OVERRIDE="-f docker-compose.staging.yml"
        ;;
    production)
        DOCKER_COMPOSE_OVERRIDE="-f docker-compose.prod.yml"
        # En producción, asegurarse de que se proporcione una versión específica
        if [ "$VERSION" = "latest" ]; then
            echo_error "En producción, debes especificar una versión específica (ej: v1.0.0)"
            exit 1
        fi
        ;;
    *)
        echo_error "Entorno no válido: $ENTORNO"
        echo_info "Entornos disponibles: staging, production"
        exit 1
        ;;
esac

echo_info "=== Iniciando despliegue en $ENTORNO (versión: $VERSION) ==="

# Crear directorio de despliegue si no existe
if [ ! -d "$DEPLOY_DIR" ]; then
    echo_info "Creando directorio de despliegue en $DEPLOY_DIR..."
    sudo mkdir -p "$DEPLOY_DIR"
    sudo chown -R $(whoami) "$DEPLOY_DIR"
fi

# Copiar archivos de configuración si no existen
if [ ! -f "$DEPLOY_DIR/.env" ]; then
    echo_info "Copiando archivo .env de ejemplo..."
    cp .env.example "$DEPLOY_DIR/.env"
    echo_info "Por favor, configura las variables de entorno en $DEPLOY_DIR/.env"
    exit 1
fi

# Cargar variables de entorno
set -a
source "$DEPLOY_DIR/.env"
set +a

# Exportar la versión como variable de entorno
export TAG="$VERSION"

# Cambiar al directorio de despliegue
cd "$DEPLOY_DIR"

# Detener y eliminar contenedores existentes
echo_info "Deteniendo contenedores existentes..."
docker-compose $DOCKER_COMPOSE_OVERRIDE down --remove-orphans || true

# Limpiar recursos no utilizados
echo_info "Limpiando recursos de Docker..."
docker system prune -f

echo_info "Descargando imágenes actualizadas..."
docker-compose $DOCKER_COMPOSE_OVERRIDE pull

echo_info "Iniciando servicios..."
docker-compose $DOCKER_COMPOSE_OVERRIDE up -d --force-recreate

echo_info "Verificando el estado de los servicios..."
docker-compose $DOCKER_COMPOSE_OVERRIDE ps

echo_ok "=== Despliegue completado exitosamente en $ENTORNO (versión: $VERSION) ==="

# Mostrar logs de los últimos 20 segundos para verificar que todo está funcionando
echo_info "Mostrando logs recientes..."
sleep 10
docker-compose $DOCKER_COMPOSE_OVERRIDE logs --tail=50

echo_ok "=== ¡Listo! La aplicación está en línea ==="
