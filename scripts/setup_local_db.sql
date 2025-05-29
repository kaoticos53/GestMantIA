-- Script para configurar la base de datos local

-- 1. Crear el rol (usuario) si no existe
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'gestmantia_user') THEN
        CREATE ROLE gestmantia_user WITH
            LOGIN
            PASSWORD 'YourSecurePassword123!';
    END IF;
END $$;

-- 2. Crear la base de datos si no existe
SELECT 'CREATE DATABASE gestmantia_dev'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'gestmantia_dev');

-- 3. Conceder todos los privilegios al usuario en la base de datos
GRANT ALL PRIVILEGES ON DATABASE gestmantia_dev TO gestmantia_user;

-- 4. Conectarse a la base de datos y crear la extensión si es necesario (ejecutar manualmente si es necesario)
-- \c gestmantia_dev
-- CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Mensaje de confirmación
\echo "Base de datos local configurada correctamente."
\echo "Usuario: gestmantia_user"
\echo "Contraseña: YourSecurePassword123!"
\echo "Base de datos: gestmantia_dev"
\echo "Puerto: 5432"
\echo ""
\echo "Puedes conectarte usando:"
\echo "psql -h localhost -U gestmantia_user -d gestmantia_dev"
