# Fase 15 — Automations

## Objetivo
Reducir la operación manual mediante procesos automáticos: recordatorios, vencimientos, morosidad, renovaciones y suspensiones.

## Procesos Automatizados

1. **Recordatorios**: notificación antes del vencimiento de un cargo.
2. **Vencimientos**: cambio de estado cuando un cargo vence.
3. **Morosidad**: detección de clientes morosos y envío de notificaciones.
4. **Renovaciones**: generación automática de cargos por renovación.
5. **Suspensiones**: suspensión automática de suscripciones por morosidad crónica.

## Entidades

### AutomationRule
- `Id`
- `Name`
- `TriggerType` (DueDate, Delinquency, Renewal, Inactivity)
- `Condition` (JSON)
- `ActionType` (SendEmail, Suspend, GenerateCharge, Notify)
- `ActionConfig` (JSON)
- `IsActive`
- `RunFrequency`

### AutomationLog
- `Id`
- `AutomationRuleId`
- `RunAt`
- `Status` (Success, Failed)
- `AffectedEntityType`
- `AffectedEntityId`
- `Message`

### ScheduledJob
- `Id`
- `Name`
- `CronExpression`
- `LastRun`
- `NextRun`
- `IsActive`

## Casos de Uso

1. Configurar reglas de recordatorio por días antes de vencimiento.
2. Configurar reglas de morosidad por días de atraso.
3. Configurar regla de suspensión automática.
4. Configurar regla de renovación automática.
5. Ver log de ejecución de automatizaciones.
6. Habilitar/deshabilitar reglas.

## Entregables UI/API

### API
- `GET/POST/PUT/DELETE /api/automation-rules`
- `GET /api/automation-logs`
- `POST /api/automation-rules/{id}/run-now`

### Worker
- Jobs programados que ejecutan las reglas según frecuencia.
- Servicio de notificaciones (email).

### UI
- Pantalla de reglas de automatización.
- Pantalla de logs de ejecución.
- Editor de condiciones y acciones.

## Reglas de Negocio

1. Una regla solo se ejecuta si está activa.
2. Las suspensiones automáticas requieren confirmación manual o política estricta.
3. Los recordatorios no deben enviarse más de una vez por cargo.
4. Las renovaciones automáticas respetan `AutoRenew` de la suscripción.
5. Las reglas de morosidad evalúan saldo calculado del cliente.

## Dependencias
- Fase 1: Foundation.
- Fase 2: Client Management.
- Fase 4: Subscriptions.
- Fase 5: Billing Engine.
- Fase 6: Financial Ledger.
- Fase 8: Customer Account Statements.

## Criterios de Aceptación

- [ ] Se pueden crear reglas de automatización.
- [ ] El worker ejecuta reglas según programación.
- [ ] Los recordatorios se envían una sola vez por cargo.
- [ ] Las suspensiones automáticas respetan configuración.
- [ ] El log registra cada ejecución.

## Notas Técnicas

- Usar Quartz.NET o Hangfire para programación.
- Implementar servicio de notificación abstracto (`INotificationService`).
- Las condiciones y acciones pueden almacenarse como JSON con validación.
- Considerar idempotencia en envío de recordatorios.
