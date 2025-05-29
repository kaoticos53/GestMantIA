# Carpeta de Entidades del Dominio (Core.Entities)

Esta carpeta está destinada a contener las entidades principales del dominio de la aplicación GestMantIA.

Las entidades aquí definidas representan los conceptos fundamentales del negocio y encapsulan tanto los datos como el comportamiento asociado a dichos conceptos.

Siga los principios de Domain-Driven Design (DDD) al definir sus entidades:

- **Ricas en comportamiento**: Las entidades no deben ser solo contenedores de datos (anemic domain model), sino que deben incluir la lógica de negocio que opera sobre sus datos.
- **Identidad única**: Cada entidad tiene una identidad única que persiste a lo largo de su ciclo de vida.
- **Validez**: Las entidades deben esforzarse por mantener su estado válido en todo momento.

Ejemplos de entidades podrían ser: `Equipo`, `OrdenDeTrabajo`, `Cliente`, etc.
