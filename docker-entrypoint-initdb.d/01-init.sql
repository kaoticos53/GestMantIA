-- Crear esquema si no existe
CREATE SCHEMA IF NOT EXISTS gestmantia;

-- Crear extensión para UUID si no existe
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Crear tabla de roles
CREATE TABLE IF NOT EXISTS gestmantia.roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Insertar roles por defecto si no existen
INSERT INTO gestmantia.roles (name, description)
SELECT 'Administrador', 'Administrador del sistema con acceso total'
WHERE NOT EXISTS (SELECT 1 FROM gestmantia.roles WHERE name = 'Administrador');

INSERT INTO gestmantia.roles (name, description)
SELECT 'Técnico', 'Técnico de mantenimiento con acceso limitado'
WHERE NOT EXISTS (SELECT 1 FROM gestmantia.roles WHERE name = 'Técnico');

INSERT INTO gestmantia.roles (name, description)
SELECT 'Usuario', 'Usuario estándar con acceso de solo lectura'
WHERE NOT EXISTS (SELECT 1 FROM gestmantia.roles WHERE name = 'Usuario');

-- Crear tabla de usuarios
CREATE TABLE IF NOT EXISTS gestmantia.users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    email_confirmed BOOLEAN DEFAULT false,
    last_login TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    role_id INTEGER REFERENCES gestmantia.roles(id)
);

-- Crear índice para búsquedas por nombre de usuario y correo
CREATE INDEX IF NOT EXISTS idx_users_username ON gestmantia.users (username);
CREATE INDEX IF NOT EXISTS idx_users_email ON gestmantia.users (email);

-- Crear tabla de dispositivos
CREATE TABLE IF NOT EXISTS gestmantia.devices (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    description TEXT,
    serial_number VARCHAR(100) UNIQUE,
    model VARCHAR(100),
    manufacturer VARCHAR(100),
    purchase_date DATE,
    warranty_expiration DATE,
    status VARCHAR(50) NOT NULL DEFAULT 'Disponible',
    location VARCHAR(255),
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES gestmantia.users(id)
);

-- Crear tabla de mantenimientos
CREATE TABLE IF NOT EXISTS gestmantia.maintenances (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    device_id UUID NOT NULL REFERENCES gestmantia.devices(id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    maintenance_type VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    priority VARCHAR(20) NOT NULL DEFAULT 'Media',
    scheduled_date TIMESTAMP WITH TIME ZONE,
    completed_date TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES gestmantia.users(id),
    assigned_to UUID REFERENCES gestmantia.users(id)
);

-- Crear tabla de partes de trabajo
CREATE TABLE IF NOT EXISTS gestmantia.work_orders (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    maintenance_id UUID NOT NULL REFERENCES gestmantia.maintenances(id) ON DELETE CASCADE,
    description TEXT NOT NULL,
    start_time TIMESTAMP WITH TIME ZONE,
    end_time TIMESTAMP WITH TIME ZONE,
    status VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES gestmantia.users(id),
    assigned_to UUID REFERENCES gestmantia.users(id)
);

-- Crear tabla de inventario
CREATE TABLE IF NOT EXISTS gestmantia.inventory (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    description TEXT,
    category VARCHAR(50) NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 0,
    min_quantity INTEGER DEFAULT 0,
    unit VARCHAR(20),
    location VARCHAR(100),
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES gestmantia.users(id)
);

-- Crear tabla de movimientos de inventario
CREATE TABLE IF NOT EXISTS gestmantia.inventory_movements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    inventory_id UUID NOT NULL REFERENCES gestmantia.inventory(id) ON DELETE CASCADE,
    movement_type VARCHAR(20) NOT NULL, -- ENTRADA, SALIDA, AJUSTE
    quantity INTEGER NOT NULL,
    reference_id UUID, -- ID de la referencia (orden de trabajo, factura, etc.)
    reference_type VARCHAR(50), -- Tipo de referencia
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    created_by UUID REFERENCES gestmantia.users(id)
);

-- Crear tabla de logs de actividad
CREATE TABLE IF NOT EXISTS gestmantia.activity_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES gestmantia.users(id) ON DELETE SET NULL,
    action VARCHAR(50) NOT NULL,
    entity_type VARCHAR(50),
    entity_id UUID,
    old_values JSONB,
    new_values JSONB,
    ip_address VARCHAR(45),
    user_agent TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Crear función para actualizar automáticamente los campos updated_at
CREATE OR REPLACE FUNCTION update_modified_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Crear triggers para actualizar automáticamente los campos updated_at
DO $$
DECLARE
    t record;
BEGIN
    FOR t IN 
        SELECT table_schema, table_name 
        FROM information_schema.tables 
        WHERE table_schema = 'gestmantia' 
        AND table_type = 'BASE TABLE'
        AND table_name NOT LIKE '%_view' -- Excluir vistas si las hay
    LOOP
        EXECUTE format('DROP TRIGGER IF EXISTS update_%s_modtime ON %I.%I', 
                      t.table_name, t.table_schema, t.table_name);
                      
        EXECUTE format('CREATE TRIGGER update_%s_modtime
                      BEFORE UPDATE ON %I.%I
                      FOR EACH ROW EXECUTE FUNCTION update_modified_column()',
                      t.table_name, t.table_schema, t.table_name);
    END LOOP;
END;
$$;
