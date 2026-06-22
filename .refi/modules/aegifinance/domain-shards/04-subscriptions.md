# Fase 4 — Subscriptions

## Objetivo
Administrar los contratos entre clientes y servicios. Una suscripción vincula un cliente, un servicio, un precio negociado y un ciclo de facturación. Es la fuente de todo ingreso recurrente.

## Entidades

### SubscriptionStatus
Enum / lookup:
- `Pending`
- `Active`
- `Suspended`
- `Cancelled`
- `Expired`

### Subscription
- `Id`
- `Code` (único, ej. SUB-00001)
- `ClientId`
- `ServiceId`
- `BillingType` (copia del servicio o override)
- `Price` (precio negociado)
- `Currency`
- `StartDate` (DateTime)
- `EndDate` (DateTime?)
- `BillingDay` (día del mes para generación del cargo)
- `Status`
- `AutoRenew` (bool)
- `Notes`
- `LastBillingDate`
- `NextBillingDate`

### SubscriptionPriceHistory
- `Id`
- `SubscriptionId`
- `OldPrice`
- `NewPrice`
- `EffectiveDate`
- `Reason`
- `CreatedBy`
- `CreatedAt`

### SubscriptionChangeLog
- `Id`
- `SubscriptionId`
- `ChangeType` (Suspend, Reactivate, Cancel, Renew, PriceChange)
- `OldValue`
- `NewValue`
- `Reason`
- `CreatedBy`
- `CreatedAt`

### SubscriptionPermission
- `Id`
- `UserId`
- `SubscriptionId`

> Entidad definida en Fase 1. Permite restringir qué suscripciones específicas puede ver un subusuario de cliente.

## Casos de Uso

1. **Crear suscripción** vinculando cliente y servicio.
2. **Listar suscripciones** por cliente, estado y servicio.
3. **Suspender una suscripción** con motivo.
4. **Reactivar una suscripción**.
5. **Cancelar una suscripción**.
6. **Cambiar precio** de suscripción con historial.
7. **Renovar automática o manualmente** una suscripción.
8. **Ver historial** de cambios de la suscripción.
9. **Administrar visibilidad por suscripción** para subusuarios de cliente.

## Entregables UI/API

### API
- `GET/POST/PUT /api/subscriptions`
- `GET /api/subscriptions/{id}`
- `POST /api/subscriptions/{id}/suspend`
- `POST /api/subscriptions/{id}/reactivate`
- `POST /api/subscriptions/{id}/cancel`
- `POST /api/subscriptions/{id}/change-price`
- `GET /api/subscriptions/{id}/history`
- `GET /api/clients/{id}/subscriptions`
- `GET/POST/DELETE /api/subscriptions/{id}/permissions`

### UI
- Pantalla de listado de suscripciones con filtros.
- Formulario de creación/edición.
- Pantalla de detalle con pestañas:
  - Información general
  - Historial de precios
  - Historial de cambios
- Botones de acción: suspender, reactivar, cancelar.
- Vista de suscripciones por cliente.

## Reglas de Negocio

1. Un cliente puede tener 0, 1 o N suscripciones.
2. El precio de la suscripción puede diferir del precio por defecto del servicio.
3. Solo suscripciones `Active` generan cargos recurrentes.
4. La suspensión detiene la generación de cargos futuros (no anula cargos ya generados).
5. La cancelación requiere fecha efectiva.
6. Los cambios de precio deben registrar historial.
7. `NextBillingDate` se calcula según `BillingType` y `BillingDay`.
8. Los usuarios `Client` solo ven suscripciones de su `ClientId`.
9. Los subusuarios de cliente solo ven suscripciones autorizadas en `SubscriptionPermission`.
10. Un administrador siempre ve todas las suscripciones.

## Dependencias
- Fase 1: Foundation.
- Fase 2: Client Management.
- Fase 3: Service Catalog.

## Criterios de Aceptación

- [ ] Se crea una suscripción vinculando cliente y servicio.
- [ ] El precio negociado se almacena independiente del servicio.
- [ ] Se puede suspender y reactivar una suscripción.
- [ ] Al suspender, `NextBillingDate` ya no avanza hasta reactivar.
- [ ] Se registra historial de precios y cambios.
- [ ] Un cliente puede tener múltiples suscripciones activas.

## Notas Técnicas

- El `BillingDay` debe validarse entre 1 y 28 para evitar problemas con febrero.
- `NextBillingDate` se calcula sumando 1 mes o 1 año según `BillingType`.
- Considerar un índice compuesto en `Subscription` por `ClientId` + `Status`.
- La cancelación no elimina la suscripción; cambia su estado y fecha de fin.
- El precio se almacena siempre en MXN; la visualización en otra moneda usa `ICurrencyConverter`.
