# Fase 7 — Subscription Allocations

## Objetivo
Aplicar los pagos recibidos a servicios específicos. Esta fase define cómo un ingreso en el ledger se distribuye entre los cargos pendientes de un cliente.

## Entidades

### SubscriptionAllocation
- `Id`
- `LedgerEntryId` (pago de tipo Income)
- `BillingItemId`
- `Amount`
- `AllocatedAt`
- `AllocatedBy`
- `IsAutomatic` (true si fue asignado por el motor)

## Casos de Uso

1. **Asignación automática** de un pago a cargos pendientes del cliente.
2. **Asignación manual** de un pago a cargos específicos.
3. **Pago parcial**: el pago cubre solo una parte de un cargo.
4. **Pago adelantado**: el pago excede los cargos pendientes y queda como saldo a favor.
5. **Pago múltiple**: un pago cubre varios cargos.
6. **Pago combinado**: varios pagos cubren un solo cargo.
7. **Desasignar** un pago de un cargo (antes de conciliación).

## Entregables UI/API

### API
- `POST /api/allocations/auto-allocate/{ledgerEntryId}`
- `POST /api/allocations/manual`
- `POST /api/allocations/{id}/unallocate`
- `GET /api/clients/{id}/allocations`
- `GET /api/billing-items/{id}/allocations`

### UI
- Pantalla de asignaciones por cliente.
- Modal de asignación manual con lista de cargos pendientes.
- Indicador de progreso de pago por cargo.
- Acción de auto-asignar.
- Resumen de saldo a favor.

## Reglas de Negocio

1. Solo pagos de tipo `Income` pueden asignarse a cargos.
2. La suma de asignaciones no puede superar el monto del pago.
3. La suma de asignaciones a un cargo no puede superar su monto.
4. Asignación automática prioriza cargos más antiguos (FIFO).
5. Un pago parcial deja el cargo en estado `Partial`.
6. Un pago completo deja el cargo en estado `Paid`.
7. El excedente queda como saldo a favor del cliente sin asignación.
8. No se puede desasignar una asignación de un movimiento reconciliado.

## Dependencias
- Fase 1: Foundation.
- Fase 2: Client Management.
- Fase 5: Billing Engine.
- Fase 6: Financial Ledger.

## Criterios de Aceptación

- [ ] Se puede auto-asignar un pago a cargos pendientes.
- [ ] Se puede asignar manualmente a cargos seleccionados.
- [ ] Se controla que no se exceda el monto del pago.
- [ ] Se controla que no se exceda el monto del cargo.
- [ ] Un pago parcial actualiza el estado del cargo a `Partial`.
- [ ] Un excedente queda sin asignar como saldo a favor.

## Notas Técnicas

- La asignación automática debe ejecutarse dentro de transacción.
- Considerar publicar evento de dominio `PaymentAllocated`.
- El estado del `BillingItem` se calcula a partir de sus asignaciones.
- Mantener un índice en `SubscriptionAllocation` por `LedgerEntryId` y `BillingItemId`.
