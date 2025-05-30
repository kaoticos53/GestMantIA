-- Script para configurar la base de datos local PostgreSQL 'gestmantia_dev'
-- Ejecutar este script conectado a una base de datos de mantenimiento (ej: 'postgres') como superusuario.

-- 1. Crear el rol (usuario) si no existe
DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_catalog.pg_roles
      WHERE  rolname = 'gestmantia_user') THEN

      CREATE ROLE gestmantia_user LOGIN PASSWORD 'YourSecurePassword123!';
      RAISE NOTICE 'Rol ''gestmantia_user'' creado.';
   ELSE
      RAISE NOTICE 'Rol ''gestmantia_user'' ya existe.';
   END IF;
END
$do$;

-- 2. Crear la base de datos.
-- Si la base de datos ya existe, este comando fallará. 
-- Puede ignorar el error o eliminarla manualmente primero (DROP DATABASE gestmantia_dev;).
CREATE DATABASE gestmantia_dev
    WITH
    OWNER = gestmantia_user
    ENCODING = 'UTF8'
    LC_COLLATE = 'C' -- Configuración de intercalación neutral. Cambiar si se necesita una específica (ej: 'es_ES.utf8').
    LC_CTYPE = 'C'   -- Configuración de tipo de caracteres neutral. Cambiar si se necesita una específica.
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1
    IS_TEMPLATE = False;
RAISE NOTICE 'Intento de creación de base de datos ''gestmantia_dev'' con propietario ''gestmantia_user''. Si falló, la BD podría ya existir.';

-- Nota: Si la base de datos ya existía y el propietario no es 'gestmantia_user', 
-- puede ejecutar lo siguiente para cambiar el propietario:
-- ALTER DATABASE gestmantia_dev OWNER TO gestmantia_user;
-- RAISE NOTICE 'Propietario de ''gestmantia_dev'' (si existía) ajustado a ''gestmantia_user''.';

-- 3. Conceder todos los privilegios al usuario en la base de datos
-- Esto es en parte redundante si el usuario es el propietario, pero asegura todos los permisos necesarios.
GRANT ALL PRIVILEGES ON DATABASE gestmantia_dev TO gestmantia_user;
RAISE NOTICE 'Todos los privilegios otorgados a ''gestmantia_user'' en ''gestmantia_dev''.';

-- IMPORTANTE: Pasos adicionales a realizar DESPUÉS de ejecutar este script:
--
-- A. Conéctese a la nueva base de datos 'gestmantia_dev'. 
--    Por ejemplo, usando psql desde la línea de comandos:
--    psql -U gestmantia_user -d gestmantia_dev -h localhost
--    (Se le pedirá la contraseña 'YourSecurePassword123!')
--
-- B. Una vez conectado a 'gestmantia_dev', ejecute el siguiente comando 
--    para habilitar el soporte para la generación de UUIDs (si es necesario):
--    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
--
--    Para confirmar que la extensión está instalada, puede ejecutar:
--    SELECT * FROM pg_extension WHERE extname = 'uuid-ossp';

-- Fin del script.
