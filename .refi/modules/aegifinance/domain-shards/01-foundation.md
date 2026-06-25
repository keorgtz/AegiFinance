# Fase 1 — Foundation

## Objetivo
Construir la base del sistema sobre la cual descansan todas las demás fases. Incluye identidad, seguridad, autorización basada en permisos y roles, modelos de usuario administrador/cliente, subusuarios de cliente, PIN simple y auditoría de cambios.

## Entidades

### BaseEntity
Campos comunes para todas las entidades del sistema:
- `Id`: GUID
- `CreatedAt`: DateTime UTC
- `CreatedBy`: GUID? (User.Id)
- `UpdatedAt`: DateTime UTC
- `UpdatedBy`: GUID?
- `IsDeleted`: bool (soft delete)
- `DeletedAt`: DateTime?
- `DeletedBy`: GUID?

### AuditableEntity
Extensión de `BaseEntity` que indica que la entidad debe generar entradas en la tabla de auditoría `AuditLog` ante cualquier cambio.

### UserType
Enum:
- `Administrator` — usuarios internos de la empresa. `ClientId` debe ser NULL.
- `Client` — usuarios de un cliente. `ClientId` es obligatorio.

### User
- `Id`
- `UserName` (único, ej. `kevin`, `hotelpalmas`)
- `Email` (único, requerido)
- `PasswordHash`
- `UserType`
- `ClientId` (nullable; obligatorio si `UserType = Client`)
- `Name`
- `IsActive`
- `EmailConfirmed`
- `RefreshToken` (string nullable)
- `RefreshTokenExpiry` (DateTime?)
- `LastLoginAt`
- Relación N:M con `Role`

### ClientUser
- `Id`
- `ClientId`
- `UserId`
- `DisplayName` (ej. `Gerente`, `Contador`, `Recepción`)
- `IsActive`

> Relaciona un usuario de tipo `Client` con el cliente al que pertenece. Permite que un cliente tenga múltiples subusuarios.

### ClientPinCredential
- `Id`
- `UserId`
- `PinHash` (hash del PIN numérico, ej. 4-6 dígitos)

> Usado por subusuarios de clientes para login rápido. El usuario principal del cliente usa contraseña normal.

### Role
- `Id`
- `Name` (único, ej. `Admin`, `Supervisor`, `Conciliador`, `Gerente`, `Consulta`)
- `Description`
- `UserType` (opcional: indica si el rol aplica a `Administrator`, `Client` o ambos)
- Relación N:M con `Permission`
- Relación N:M con `User`

### Permission
- `Id`
- `Code` (único, ej. `ViewDashboard`, `ViewReports`, `ViewRevenue`, `ViewPayments`, `ViewSubscriptions`, `ViewLicenses`, `ViewTickets`, `ViewClients`, `ManageUsers`, `ManageBilling`, `ManageReconciliation`, `ManageRoles`)
- `Name`
- `Description`

### UserPermission (opcional, para overrides directos)
- `Id`
- `UserId`
- `PermissionId`
- `IsGranted`

> Permite asignar/quitar permisos individuales sin depender exclusivamente del rol.

### SubscriptionPermission
- `Id`
- `UserId`
- `SubscriptionId`

> Define qué suscripciones específicas puede ver un usuario de cliente.

### AuditLog
- `Id`
- `EntityType`
- `EntityId`
- `Action` (Created, Updated, Deleted)
- `Changes` (JSON diff)
- `UserId`
- `Timestamp`
- `IPAddress`
- `UserAgent`

### CurrencyConfig
- `Id`
- `Code` (ej. `USD`, `EUR`, `MXN`)
- `Name`
- `Symbol`
- `IsActive`
- `IsDefault` (solo una; default MXN)

### ExchangeRate
- `Id`
- `CurrencyCode`
- `RateToMXN` (cuántos MXN equivalen a 1 unidad de moneda extranjera)
- `RateFromMXN` (cuántas unidades de moneda extranjera equivalen a 1 MXN)
- `EffectiveDate`
- `Source` (`Auto` / `Manual`)
- `CreatedBy`
- `CreatedAt`

## Casos de Uso

### Autenticación
1. **Login** con `UserName`/`Email` y contraseña.
2. **Login con PIN** para subusuarios de clientes.
3. **Logout** revocando refresh token.
4. **Refresh token** para renovar JWT sin pedir credenciales.
5. **Cambio de contraseña** con validación de contraseña actual.
6. **Recuperación de contraseña** (futuro; marcar como no MVP).

### Gestión de Usuarios
1. CRUD de usuarios internos (`Administrator`).
2. CRUD de usuarios de clientes (`Client`).
3. Activar / desactivar usuario.
4. Asignar/quitar roles a usuario.
5. Asignar/quitar permisos individuales.

### Gestión de Roles y Permisos
1. CRUD de roles.
2. CRUD de permisos (seed inicial).
3. Asignar permisos a roles.
4. Seed de roles por `UserType`.

### Subusuarios de Cliente
1. Crear subusuario dentro de un cliente.
2. Asignar PIN a subusuario.
3. Activar/desactivar subusuario.
4. Asignar suscripciones visibles (`SubscriptionPermission`).

### Configuración de Moneda
1. CRUD de monedas disponibles (`CurrencyConfig`).
2. Configurar moneda por defecto (siempre MXN en almacenamiento).
3. Registrar tipo de cambio manual (`ExchangeRate`).
4. Sincronizar tipo de cambio automático desde servicio externo.
5. Ver historial de tipos de cambio.

### Auditoría
1. Listado paginado de logs de auditoría.
2. Filtros por entidad, usuario, fecha y acción.
3. Vista de diff para un registro específico.

## Entregables UI/API

### API
- `POST /api/auth/login`
- `POST /api/auth/login-pin`
- `POST /api/auth/logout`
- `POST /api/auth/refresh`
- `POST /api/auth/change-password`
- `GET /api/auth/me` — usuario actual, `UserType`, `ClientId`, roles y permisos efectivos. Fuente de `usePermissions()` y del menú dinámico en el frontend (React/Next.js).
- `GET/POST/PUT/DELETE /api/users`
- `GET/POST/PUT/DELETE /api/roles`
- `GET/POST/PUT/DELETE /api/permissions`
- `GET/POST/PUT/DELETE /api/client-users`
- `POST /api/client-users/{id}/set-pin`
- `GET/POST/DELETE /api/subscription-permissions`
- `GET/POST/PUT/DELETE /api/currencies`
- `GET/POST/PUT/DELETE /api/exchange-rates`
- `POST /api/exchange-rates/sync`
- `GET /api/audit-logs`

### UI (React + TypeScript + Next.js + AegisUI)
- Pantalla de login (`app/(auth)/login/page.tsx`).
- Pantalla de gestión de usuarios.
- Pantalla de gestión de roles.
- Pantalla de permisos (solo lectura con asignación a roles).
- Pantalla de subusuarios por cliente.
- Pantalla de auditoría con tabla y detalle de cambios.
- Pantalla de configuración de monedas.
- Pantalla de tipos de cambio (manual y sincronización automática).
- Menú dinámico construido por permisos (`usePermissions()` contra `GET /api/auth/me`).

## Reglas de Negocio

1. El `UserName` y el `Email` deben ser únicos.
2. La contraseña debe cumplir política mínima de seguridad (8 caracteres, mayúscula, minúscula, número, símbolo).
3. Un usuario desactivado no puede iniciar sesión.
4. Un `UserType = Client` debe tener `ClientId` obligatorio.
5. Un `UserType = Administrator` debe tener `ClientId` NULL.
6. Los tokens JWT expiran en 15 minutos; los refresh tokens en 7 días.
7. Cada acción de escritura en entidades auditables genera un `AuditLog`.
8. No se pueden eliminar roles si tienen usuarios asignados (o se desasignan automáticamente según política definida).
9. El menú de navegación se construye dinámicamente según los permisos efectivos del usuario.
10. Los permisos efectivos de un usuario = permisos de sus roles + overrides de `UserPermission`.
11. La moneda por defecto del sistema es MXN.
12. Todo monto se almacena en MXN; las conversiones son solo visuales y se registran con la tasa usada.

## Dependencias
- Ninguna (es la fase base).

## Criterios de Aceptación

- [ ] Login con contraseña devuelve JWT y refresh token válidos.
- [ ] Login con PIN funciona para subusuarios de cliente.
- [ ] Logout invalida el refresh token.
- [ ] Un usuario sin permiso recibe 403 al intentar acceder a un recurso protegido.
- [ ] Usuario `Client` sin `ClientId` es rechazado en validación.
- [ ] Auditoría registra cambios en usuarios, roles y permisos.
- [ ] El menú dinámico refleja solo los permisos del usuario.
- [ ] Las migraciones iniciales se ejecutan correctamente en SQL Server.
- [ ] Se puede registrar un tipo de cambio manual.
- [ ] Se puede sincronizar tipo de cambio automático (con servicio externo configurable).

## Notas Técnicas

- Usar `Microsoft.AspNetCore.Authentication.JwtBearer` para JWT.
- Almacenar refresh tokens hasheados en base de datos.
- Entregar el refresh token como cookie `HttpOnly`/`Secure`/`SameSite=Lax` (no en el body de la respuesta), para que el frontend React lo consuma sin exponerlo a JavaScript. El access token JWT sí va en el body y el frontend lo mantiene solo en memoria.
- Configurar `AddCors()` con política nombrada para el origen de Next.js (desarrollo y producción), con `AllowCredentials()` habilitado.
- Implementar `IAuthorizationHandler` para evaluar permisos como claims.
- Configurar `SaveChangesInterceptor` de EF Core para generar auditoría automáticamente.
- Seed de permisos iniciales mediante migración o `IHostedService`.
- Centralizar filtrado por `ClientId` en repositorios base para evitar fugas de datos.
- Implementar `ICurrencyConverter` en Application layer para centralizar conversiones MXN ↔ moneda de visualización.
- Configurar servicio externo de tipo de cambio en `appsettings.json` (URL, API key, fallback manual).
