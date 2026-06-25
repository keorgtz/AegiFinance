# Fase 14 — Customer Portal (Integrado)

## Objetivo
Habilitar el acceso de clientes y sus subusuarios dentro de la misma aplicación AegiFinance. **No será una aplicación aparte.** El portal es un módulo más del sistema, compartiendo las mismas vistas, componentes y arquitectura de permisos que los usuarios internos.

## Regla Arquitectónica Obligatoria: Single View Architecture (SVA)

- Cada módulo tendrá **una única vista** (una sola ruta de Next.js).
- Está prohibido crear vistas/rutas duplicadas como `dashboard/admin/page.tsx` / `dashboard/client/page.tsx`.
- El contenido se adapta evaluando permisos en cada componente con `usePermissions()`.

### Ejemplos correctos

```tsx
{can('ViewRevenue') && (
  ...
)}

{can('ManageUsers') && (
  ...
)}

{can('ViewReports') && (
  ...
)}
```

### Beneficios
- Una sola vista.
- Un solo mantenimiento.
- Un solo flujo.
- Menor deuda técnica.
- Escalabilidad máxima.

## Modelo de Usuarios

### UserType
Define quién es el usuario:
- `Administrator` — usuarios internos de la empresa. `ClientId = NULL`.
- `Client` — usuarios pertenecientes a un cliente. `ClientId` obligatorio.

### User
```csharp
public class User
{
    Guid Id;
    string UserName;
    string PasswordHash;
    UserType Type;
    Guid? ClientId;
    bool Active;
}
```

### ClientUser
Relaciona subusuarios con el cliente principal:
```csharp
public class ClientUser
{
    Guid Id;
    Guid ClientId;
    Guid UserId;
    string DisplayName;
    bool Active;
}
```

### ClientPinCredential
Credencial PIN simple para subusuarios:
```csharp
public class ClientPinCredential
{
    Guid Id;
    Guid UserId;
    string PinHash;
}
```

### Roles
- **Administradores:** Admin, Supervisor, Conciliador, Ventas, Soporte, Consulta.
- **Clientes:** Admin, Gerente, Consulta, Contador, Operador.

Los roles son independientes del `UserType`.

## Matriz de Seguridad

```
Permission → Role → User → Client → Subscription
```

1. El permiso define qué puede hacer el usuario.
2. El rol agrupa permisos.
3. El `UserType` determina si es interno o cliente.
4. El `ClientId` filtra todos los datos visibles.
5. `SubscriptionPermission` restringe qué suscripciones puede ver un subusuario.

## Control de Visibilidad

### Por cliente
- Un usuario `Client` solo puede ver datos donde `ClientId = suyo`.
- Nunca podrá consultar datos de otros clientes.

### Por suscripción
- Los subusuarios de un cliente solo ven suscripciones autorizadas en `SubscriptionPermission`.
- Ejemplo: un gerente puede ver solo "Software Hotelero", no "Hosting" ni "Mantenimiento".

### Por módulo
- Un contador cliente puede ver Estado de Cuenta, Pagos y Facturación.
- No puede ver Usuarios, Tickets ni Configuración.

## Dashboard Adaptativo

Misma vista (`app/(app)/dashboard/page.tsx` → `<Dashboard />`), diferente contenido:

| Usuario | Ve |
|---------|-----|
| Kevin (Admin) | MRR, ARR, todos los clientes, conciliación, reportes, ingresos |
| Cliente principal | Planes, pagos, adeudos, estado de cuenta |
| Gerente cliente | Planes asignados, pagos asignados |

## Menú Dinámico

El menú de navegación se construye a partir de los permisos del usuario.

### Ejemplo cliente
```text
Dashboard
Mis Servicios
Estado Cuenta
Pagos
Tickets
```

### Ejemplo administrador
```text
Dashboard
Conciliación
Reportes
Clientes
Suscripciones
```

## Funciones del Portal

1. Ver suscripciones (filtradas por cliente y permisos).
2. Ver pagos (filtrados por cliente).
3. Ver facturación / estado de cuenta (filtrado por cliente).
4. Ver licencias (filtradas por cliente).
5. Crear tickets de soporte (vinculados al cliente).
6. Ver respuestas de tickets.

## Entidades Reutilizadas

- Client
- Subscription
- BillingItem
- LedgerEntry
- License
- Ticket
- User
- ClientUser
- ClientPinCredential
- SubscriptionPermission

## Casos de Uso

1. Login de cliente con `UserName` + contraseña.
2. Login de subusuario con `UserName` + PIN.
3. Dashboard adaptativo según permisos.
4. Ver detalle de suscripción.
5. Ver estado de cuenta.
6. Descargar comprobante de pago (PDF estático con estilo AegisUI).
7. Ver pagos y estado de cuenta en moneda seleccionada (conversión visual desde MXN).
8. Crear ticket de soporte.
9. Ver respuestas de tickets.

## Entregables UI/API

### API
Los endpoints son los mismos que usa la UI interna; la autorización filtra por permisos y `ClientId`:
- `POST /api/auth/login`
- `POST /api/auth/login-pin`
- `GET /api/auth/me`
- `GET /api/dashboard/summary`
- `GET /api/subscriptions` (filtrado por cliente)
- `GET /api/ledger/income` (filtrado por cliente)
- `GET /api/clients/{id}/statement`
- `GET /api/licenses` (filtrado por cliente)
- `GET/POST /api/tickets`

### UI (React + TypeScript + Next.js + AegisUI)
- Login único compartido (`app/(auth)/login/page.tsx`, detecta `UserType` y redirige).
- Dashboard adaptativo (`app/(app)/dashboard/page.tsx`).
- Pantalla de suscripciones (`app/(app)/subscriptions/page.tsx`).
- Pantalla de pagos (`app/(app)/ledger/page.tsx` filtrado).
- Pantalla de estado de cuenta (`app/(app)/account-statement/page.tsx`).
- Selector de moneda de visualización en pantallas financieras.
- Pantalla de licencias (`app/(app)/licenses/page.tsx`).
- Mesa de ayuda del cliente (`app/(app)/tickets/page.tsx`).
- En mobile, el portal usa bottom tab bar y formularios de alta/edición a pantalla completa (ver `Plan1.2-Extension.md`).

## Reglas de Negocio

1. El portal no es una aplicación separada; es un módulo de AegiFinance.
2. Un usuario de portal solo ve información de su cliente.
3. No se permite crear, editar ni eliminar suscripciones desde el portal.
4. Los tickets creados desde el portal son visibles para el equipo interno.
5. El portal no expone datos de otros clientes (multitenancy lógica por cliente).
6. Toda nueva funcionalidad debe cumplir SVA.
7. Toda autorización pasa por el Permission Engine.
8. Cero duplicación de pantallas.
9. Cero lógica duplicada.
10. Los montos mostrados a clientes se almacenan siempre en MXN; la visualización en otra moneda es una conversión usando `ICurrencyConverter`.
11. Se muestra la tasa de cambio aplicada cuando la visualización no sea MXN.

## Dependencias
- Fase 1: Foundation (UserType, permisos, PIN, SubscriptionPermission).
- Fase 2: Client Management.
- Fase 4: Subscriptions.
- Fase 5: Billing Engine.
- Fase 6: Financial Ledger.
- Fase 8: Customer Account Statements.
- Fase 12: Support Tickets.
- Fase 13: License Management.

## Criterios de Aceptación

- [ ] Un cliente puede iniciar sesión en el mismo login que los administradores.
- [ ] Ve solo sus suscripciones, pagos y estados de cuenta.
- [ ] Puede crear un ticket de soporte.
- [ ] Puede ver sus licencias.
- [ ] No accede a información de otros clientes.
- [ ] No existen componentes duplicados para portal.
- [ ] El menú se construye dinámicamente según permisos.
- [ ] Los subusuarios con `SubscriptionPermission` solo ven sus suscripciones asignadas.
- [ ] Los montos se pueden visualizar en moneda extranjera usando `ICurrencyConverter`.
- [ ] Se muestra la tasa de cambio aplicada.

## Notas Técnicas

- Usar el mismo esquema de autenticación; diferenciar por claims `UserType` y `ClientId`.
- Aplicar autorización basada en `ClientId` para todas las operaciones de clientes.
- Implementar `IPermissionService` centralizado para evaluar permisos efectivos en UI y API.
- El filtro por `ClientId` debe estar en el repositorio base, no depender de controllers.
- Considerar rate limiting en endpoints públicos.
