# Fase 10 — Dashboard

## Objetivo
Proveer indicadores de negocio para la toma de decisiones. El dashboard se alimenta de los datos del ledger, suscripciones y facturación. Aplica **Single View Architecture (SVA)**: una misma vista que cambia su contenido según el usuario y sus permisos.

## Principio SVA

- Existe una única ruta `app/(app)/dashboard/page.tsx` (`<Dashboard />`).
- No existirán `dashboard/admin/page.tsx`, `dashboard/client/page.tsx` ni variantes por rol.
- El contenido se adapta evaluando permisos en tiempo de renderizado con `usePermissions()`:
  - `{can('ViewRevenue') && ...}`
  - `{can('ViewSubscriptions') && ...}`
  - `{can('ViewReports') && ...}`
  - `{can('ManageUsers') && ...}`

## KPIs por Tipo de Usuario

### Administrador (ej. Kevin)
- Clientes Activos
- Clientes Morosos
- MRR / ARR
- Ingresos Mes / Año
- Pendiente de Cobro
- Conciliación
- Reportes

### Cliente principal
- Mis Planes
- Mis Pagos
- Adeudos
- Estado de Cuenta

### Gerente / Subusuario de cliente
- Planes asignados (`SubscriptionPermission`)
- Pagos asignados
- Estado de cuenta filtrado

## Entidades

No hay entidades nuevas. Se usan vistas/DTOs:

### DashboardSummary
- `ActiveClients`
- `DelinquentClients`
- `Mrr`
- `Arr`
- `MonthlyIncome`
- `YearlyIncome`
- `PendingToCollect`

### DashboardTrendItem
- `Date`
- `Income`
- `Charges`

## Casos de Uso

1. Ver resumen ejecutivo de KPIs según permisos.
2. Ver gráfico de ingresos vs cargos por mes (si aplica).
3. Ver lista de clientes morosos (solo administradores).
4. Ver suscripciones próximas a vencer.
5. Ver planes asignados (subusuarios de cliente).
6. Filtrar dashboard por rango de fechas.

## Entregables UI/API

### API
- `GET /api/dashboard/summary?currency=MXN`
- `GET /api/dashboard/trends?currency=MXN`
- `GET /api/dashboard/delinquent-clients?currency=MXN`
- `GET /api/dashboard/upcoming-renewals`
- `GET /api/dashboard/assigned-subscriptions?currency=MXN`

> Todos los endpoints deben filtrar por `ClientId` cuando el usuario es de tipo `Client`. Los montos se convierten a la moneda solicitada usando `ICurrencyConverter`.

### UI
- Dashboard único (`<Dashboard />`) con tarjetas de KPIs (`KpiCard` de AegisUI).
- Selector de moneda de visualización (default MXN).
- Gráficos de tendencias con Recharts/visx (solo si el usuario tiene `ViewRevenue` o equivalente).
- Tabla de clientes morosos (solo administradores), con navegación de fila por teclado (`↑`/`↓`/`Enter`).
- Tabla de renovaciones próximas.
- Sección "Mis planes asignados" para subusuarios.
- Indicador de tasa de cambio usada cuando la visualización no sea MXN.
- Accesible desde la paleta de comandos (`Ctrl/Cmd+K`).

## Reglas de Negocio

1. El dashboard es una sola vista compartida.
2. Los KPIs se calculan en tiempo real o con cache de 5 minutos.
3. MRR solo incluye suscripciones activas.
4. Los ingresos se toman del ledger (`Income`).
5. El pendiente de cobro son `BillingItem` en estado `Pending` o `Partial`.
6. Un cliente es moroso si su saldo calculado > 0.
7. Los usuarios `Client` solo ven KPIs de su propio cliente.
8. Los subusuarios con `SubscriptionPermission` solo ven datos de suscripciones asignadas.
9. Los KPIs monetarios se calculan en MXN y se convierten a la moneda de visualización seleccionada.
10. Se muestra la tasa de cambio aplicada cuando la visualización no sea MXN.

## Dependencias
- Fase 1: Foundation (permisos, UserType, filtrado).
- Fase 2: Client Management.
- Fase 4: Subscriptions.
- Fase 5: Billing Engine.
- Fase 6: Financial Ledger.
- Fase 7: Subscription Allocations.
- Fase 8: Customer Account Statements.

## Criterios de Aceptación

- [ ] Existe una única ruta `app/(app)/dashboard/page.tsx` (`<Dashboard />`).
- [ ] El contenido cambia según los permisos del usuario.
- [ ] Los KPIs principales se muestran en tarjetas.
- [ ] Los gráficos muestran tendencias mensuales cuando aplica.
- [ ] La lista de morosos es correcta y solo visible para administradores.
- [ ] Las renovaciones próximas se calculan según `NextBillingDate`.
- [ ] Los datos son consistentes con el ledger.
- [ ] Un usuario `Client` no ve datos de otros clientes.
- [ ] Los KPIs se pueden visualizar en moneda extranjera usando `ICurrencyConverter`.
- [ ] Se muestra la tasa de cambio aplicada.

## Notas Técnicas

- Usar consultas SQL optimizadas o vistas materializadas si el volumen lo requiere.
- Considerar cache en memoria con expiración.
- Implementar gráficos con Recharts o visx, estilizados con tokens de AegisUI.
- No almacenar KPIs calculados permanentemente.
- Centralizar el filtro por `ClientId` en el repositorio base.
- Calcular KPIs en MXN y aplicar conversión al final para visualización.
- Cachear tasas de cambio del día para evitar llamadas repetidas.
