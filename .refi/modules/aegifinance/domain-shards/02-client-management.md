# Fase 2 — Client Management

## Objetivo
Administrar el catálogo de clientes del negocio. Un cliente es la entidad central que puede tener suscripciones, pagos, notas y contactos. Esta fase no incluye finanzas; solo el maestro de clientes.

## Entidades

### Client
- `Id`
- `Code` (código único interno, ej. CLI-00001)
- `Name` (nombre fiscal o comercial)
- `TradeName` (nombre comercial opcional)
- `TaxId` (RFC/NIT/CUIT según país; opcional)
- `BillingEmail`
- `BillingAddress`
- `Phone`
- `Status` (Active, Inactive, Prospective)
- `Notes`
- `CreatedAt`, `UpdatedAt`, etc.

### ClientTag
- `Id`
- `Name` (único)
- `Color` (hex para UI)
- Relación N:M con `Client`

### ClientCategory
- `Id`
- `Name` (único, ej. Software, Hosting, Soporte)
- `Description`
- Relación 1:N con `Client`

### ClientNote
- `Id`
- `ClientId`
- `Content`
- `CreatedBy`
- `CreatedAt`
- `IsPinned`

### ClientContact
- `Id`
- `ClientId`
- `Name`
- `Email`
- `Phone`
- `Position`
- `IsPrimary`

## Casos de Uso

1. **CRUD de clientes** con búsqueda, filtros y paginación.
2. **Asignar/quitar etiquetas** a un cliente.
3. **Asignar categoría** a un cliente.
4. **Agregar, editar y eliminar contactos** de un cliente (uno puede ser primario).
5. **Agregar notas** con opción de fijar.
6. **Ver historial** de cambios del cliente (desde auditoría).
7. **Activar/desactivar** cliente.

## Entregables UI/API

### API
- `GET/POST/PUT/DELETE /api/clients`
- `GET /api/clients/{id}`
- `GET /api/clients/{id}/contacts`
- `GET /api/clients/{id}/notes`
- `GET/POST/PUT/DELETE /api/client-tags`
- `GET/POST/PUT/DELETE /api/client-categories`

### UI
- Pantalla de listado de clientes con búsqueda y filtros.
- Formulario de creación/edición de cliente.
- Pantalla de detalle de cliente con pestañas:
  - Información general
  - Contactos
  - Notas
  - Historial de cambios
- Selector de etiquetas con colores.
- Selector de categoría.

## Reglas de Negocio

1. El código de cliente debe ser único y generado automáticamente.
2. Un cliente puede tener varios contactos pero solo uno puede ser primario.
3. Un cliente desactivado no puede tener nuevas suscripciones activas.
4. Las etiquetas deben tener nombres únicos y un color asociado.
5. Las categorías deben tener nombres únicos.
6. Las notas fijadas aparecen primero en la lista.

## Dependencias
- Fase 1: Foundation (usuarios, roles, permisos, auditoría).

## Criterios de Aceptación

- [ ] Se puede crear un cliente con código único auto-generado.
- [ ] Se puede asignar una categoría y múltiples etiquetas.
- [ ] Se pueden agregar contactos y marcar uno como primario.
- [ ] Se pueden agregar notas y fijarlas.
- [ ] Los cambios quedan registrados en auditoría.
- [ ] Un usuario sin permiso `clients:read` no puede ver el listado.

## Notas Técnicas

- Considerar índice único en `Client.Code`.
- Usar `Owned` o tabla separada para dirección de facturación si se requiere estructurar campos.
- El historial se obtiene filtrando `AuditLog` por `EntityType = "Client"` y `EntityId`.
- Preparar DTOs para listado (`ClientListDto`) y detalle (`ClientDetailDto`).
