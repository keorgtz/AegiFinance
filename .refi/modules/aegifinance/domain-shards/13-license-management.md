# Fase 13 — License Management

## Objetivo
Administrar licencias de software, instalaciones y activaciones. Compatible con SHEndevour, AegiDocs y futuros productos.

## Entidades

### License
- `Id`
- `LicenseKey` (único, generado criptográficamente)
- `ClientId`
- `ProductName` (ej. SHEndevour, AegiDocs)
- `ProductVersion`
- `SubscriptionId` (nullable)
- `MaxActivations`
- `ActivationCount`
- `Status` (Active, Suspended, Expired, Revoked)
- `StartDate`
- `ExpiryDate`
- `CreatedAt`

### Installation
- `Id`
- `LicenseId`
- `MachineId` (identificador único del dispositivo)
- `MachineName`
- `InstalledAt`
- `LastSeenAt`
- `IsActive`

### Activation
- `Id`
- `InstallationId`
- `ActivatedAt`
- `DeactivatedAt`
- `ActivationToken`

### LicenseValidationLog
- `Id`
- `LicenseId`
- `InstallationId`
- `ValidationDate`
- `Result` (Success, Failed)
- `Reason`
- `IPAddress`

## Casos de Uso

1. Crear licencia para un cliente/producto.
2. Generar `LicenseKey` automáticamente.
3. Registrar instalación.
4. Activar/desactivar instalación.
5. Validar licencia desde producto cliente.
6. Suspender/revocar licencia.
7. Ver historial de validaciones.
8. Renovar licencia al renovar suscripción.

## Entregables UI/API

### API
- `GET/POST/PUT /api/licenses`
- `GET /api/licenses/{id}`
- `POST /api/licenses/{id}/installations`
- `POST /api/installations/{id}/activate`
- `POST /api/installations/{id}/deactivate`
- `POST /api/licenses/validate`
- `GET /api/licenses/{id}/validation-log`

### UI
- Pantalla de licencias.
- Detalle de licencia con instalaciones.
- Historial de validaciones.
- Botones de suspender/revocar.

## Reglas de Negocio

1. Cada `LicenseKey` es único y criptográficamente seguro.
2. No se puede activar más allá de `MaxActivations`.
3. Una licencia expirada o revocada no pasa validación.
4. Una instalación inactiva no cuenta para el límite.
5. La licencia puede vincularse a una suscripción para renovación automática.

## Dependencias
- Fase 1: Foundation.
- Fase 2: Client Management.
- Fase 4: Subscriptions (opcional).

## Criterios de Aceptación

- [ ] Se genera una licencia con clave única.
- [ ] Se registran instalaciones dentro del límite permitido.
- [ ] La validación desde API responde correctamente.
- [ ] Las revocaciones y suspensiones funcionan.
- [ ] El historial de validaciones se registra.

## Notas Técnicas

- Usar GUID + HMAC o similar para generar claves.
- Exponer endpoint público de validación con rate limiting.
- Encriptar `LicenseKey` en base de datos.
- Considerar firma de licencias para productos offline.
