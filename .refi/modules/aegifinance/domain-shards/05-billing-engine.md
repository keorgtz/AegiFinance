# Fase 5 — Billing Engine

## Objetivo
Generar automáticamente los cargos esperados a partir de las suscripciones activas. El motor debe soportar generación mensual, anual, reprocesamiento y correcciones.

## Entidades

### BillingCycle
- `Id`
- `Year`
- `Month` (1-12; null para ciclos anuales personalizados)
- `StartDate`
- `EndDate`
- `Status` (Open, Closed, Reprocessing)
- `ClosedAt`
- `ClosedBy`

### BillingItem
- `Id`
- `SubscriptionId`
- `BillingCycleId`
- `ClientId`
- `Description`
- `Amount` (decimal)
- `Currency`
- `DueDate`
- `Status` (Pending, Partial, Paid, Cancelled)
- `PaidAmount`
- `GeneratedAt`
- `GeneratedBy` (sistema o usuario)

### BillingGenerationLog
- `Id`
- `BillingCycleId`
- `StartedAt`
- `FinishedAt`
- `Status` (Success, Partial, Failed)
- `ItemsGenerated`
- `Errors` (JSON)
- `TriggeredBy`

## Casos de Uso

1. **Generar cargos mensuales** para un mes/año específico.
2. **Generar cargos anuales** para un año específico.
3. **Reprocesar** un ciclo de facturación cerrado.
4. **Corregir un cargo** generado (sin modificar saldos directamente).
5. **Cancelar un cargo** pendiente con motivo.
6. **Cerrar un ciclo de facturación**.
7. **Ver log de generación**.

## Entregables UI/API

### API
- `POST /api/billing/generate`
- `POST /api/billing/reprocess/{cycleId}`
- `POST /api/billing/cancel-item/{itemId}`
- `POST /api/billing/close-cycle/{cycleId}`
- `GET /api/billing/cycles`
- `GET /api/billing/cycles/{id}/items`
- `GET /api/billing/generation-logs`

### Worker
- Job periódico (diario) que genera cargos para suscripciones cuyo `NextBillingDate` corresponde al día actual.

### UI
- Pantalla de ciclos de facturación.
- Pantalla de cargos esperados con filtros.
- Modal de generación manual.
- Vista de logs de generación.
- Acciones: reprocesar, cerrar ciclo, cancelar item.

## Reglas de Negocio

1. Solo suscripciones `Active` generan cargos.
2. No se genera un cargo duplicado para la misma suscripción y ciclo.
3. El monto del cargo proviene del precio vigente de la suscripción.
4. `DueDate` se calcula según `BillingDay` o política de vencimiento.
5. La cancelación de un cargo pendiente no afecta el ledger.
6. El reprocesamiento solo puede regenerar items no pagos (o todos con confirmación).
7. Todo cargo generado queda ligado a un `BillingCycle`.

## Dependencias
- Fase 1: Foundation.
- Fase 2: Client Management.
- Fase 3: Service Catalog.
- Fase 4: Subscriptions.

## Criterios de Aceptación

- [ ] El worker genera cargos automáticamente según `NextBillingDate`.
- [ ] Se puede generar manualmente un ciclo mensual/anual.
- [ ] No se generan cargos duplicados.
- [ ] Los cargos se listan por ciclo y cliente.
- [ ] Se puede cancelar un cargo pendiente.
- [ ] El log de generación registra éxito/fracaso y cantidad de items.

## Notas Técnicas

- Ejecutar la generación dentro de una transacción.
- Usar idempotency key basada en `SubscriptionId + BillingCycleId`.
- El worker debe ser resiliente: errores en una suscripción no detienen las demás.
- Considerar cola de background (Hangfire / Quartz) para reprocesamientos.
- El monto de cada cargo se almacena siempre en MXN; la visualización en otra moneda usa `ICurrencyConverter`.
