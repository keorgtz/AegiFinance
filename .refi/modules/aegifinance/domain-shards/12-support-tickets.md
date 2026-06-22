# Fase 12 — Support Tickets

## Objetivo
Administrar el soporte a clientes mediante tickets, comentarios y adjuntos.

## Entidades

### Ticket
- `Id`
- `Code` (único, ej. TKT-00001)
- `ClientId`
- `Title`
- `Description`
- `Status` (Open, InProgress, Waiting, Resolved, Closed)
- `Priority` (Low, Medium, High, Critical)
- `AssignedTo` (User.Id)
- `CreatedBy`
- `CreatedAt`
- `ResolvedAt`
- `ClosedAt`

### TicketComment
- `Id`
- `TicketId`
- `Content`
- `IsInternal` (visible solo internos)
- `CreatedBy`
- `CreatedAt`

### TicketAttachment
- `Id`
- `TicketId` o `TicketCommentId`
- `FileName`
- `FileUrl`
- `FileSize`
- `UploadedBy`
- `UploadedAt`

## Casos de Uso

1. Crear ticket desde portal interno.
2. Asignar ticket a un usuario.
3. Cambiar estado y prioridad.
4. Agregar comentarios internos y públicos.
5. Adjuntar archivos a tickets y comentarios.
6. Ver historial del ticket.
7. Cerrar ticket.
8. Listar tickets por cliente, estado y asignado.

## Entregables UI/API

### API
- `GET/POST/PUT /api/tickets`
- `GET /api/tickets/{id}`
- `POST /api/tickets/{id}/comments`
- `POST /api/tickets/{id}/attachments`
- `POST /api/tickets/{id}/assign`
- `POST /api/tickets/{id}/change-status`

### UI
- Mesa de ayuda con listado de tickets.
- Pantalla de detalle de ticket.
- Panel de comentarios.
- Subida de archivos.
- Filtros por estado, prioridad y asignado.

## Reglas de Negocio

1. Un ticket siempre pertenece a un cliente.
2. Los comentarios internos no son visibles para el cliente en el portal.
3. Los adjuntos deben limitarse por tamaño y tipo.
4. Solo usuarios con permiso pueden cambiar estado.
5. Un ticket cerrado no puede recibir comentarios (reabrir primero).

## Dependencias
- Fase 1: Foundation.
- Fase 2: Client Management.

## Criterios de Aceptación

- [ ] Se crea un ticket vinculado a cliente.
- [ ] Se pueden agregar comentarios internos y públicos.
- [ ] Se pueden adjuntar archivos.
- [ ] Los estados y prioridades se actualizan.
- [ ] Los comentarios internos no se muestran al cliente.

## Notas Técnicas

- Almacenar adjuntos en disco o storage externo.
- Limitar tamaño de archivo (ej. 10 MB) y tipos permitidos.
- Notificar al asignado cuando se crea o actualiza ticket (fase 15).
