# Fase 8 — Customer Account Statements

## Objetivo
Generar estados de cuenta por cliente consolidando cargos, pagos, ajustes y saldo. Es la cara visible para el usuario del sistema financiero.

## Entidades

### AccountStatement (vista / DTO, no tabla)
- `ClientId`
- `StatementDate`
- `StartDate`
- `EndDate`
- `DisplayCurrency` (moneda de visualización; MXN por defecto)
- `ExchangeRateUsed` (tasa aplicada si la visualización no es MXN)
- `InitialBalance`
- `TotalCharges`
- `TotalPayments`
- `TotalAdjustments`
- `FinalBalance`
- `Items` (lista de `AccountStatementItem`)

### AccountStatementItem
- `Date`
- `Type` (Charge, Payment, Adjustment)
- `Description`
- `ReferenceId`
- `Debit` (en moneda de visualización)
- `Credit` (en moneda de visualización)
- `Balance` (en moneda de visualización)
- `OriginalAmountMXN` (monto original almacenado en MXN)
- `ExchangeRateUsed`

## Casos de Uso

1. **Generar estado de cuenta** por rango de fechas.
2. **Ver detalle de movimientos** de un cliente.
3. **Resumen financiero** de un cliente (total adeudado, pagado, saldo a favor).
4. **Ver estado de cuenta en moneda seleccionada** (MXN u otra moneda configurada).
5. **Exportar** estado de cuenta a PDF/Excel (futuro; opcional para MVP).
6. **Ver saldo actual** de un cliente.

## Entregables UI/API

### API
- `GET /api/clients/{id}/statement?from=&to=&currency=MXN`
- `GET /api/clients/{id}/financial-summary?currency=MXN`
- `GET /api/clients/{id}/movements?currency=MXN`

> Si se solicita una moneda distinta a MXN, el sistema usa `ICurrencyConverter` con la tasa automática o manual vigente.

### UI
- Pestaña "Estado de cuenta" en detalle de cliente.
- Filtros de rango de fechas.
- Selector de moneda de visualización (default MXN).
- Tabla de movimientos con saldo acumulado.
- Resumen: cargos, pagos, ajustes, saldo.
- Indicadores visuales de saldo negativo/positivo.
- Indicador de tasa de cambio usada cuando aplica.

## Reglas de Negocio

1. El estado de cuenta se calcula a partir de `BillingItem` y `LedgerEntry`.
2. Cargos aumentan el saldo adeudado.
3. Pagos y ajustes a favor disminuyen el saldo adeudado.
4. El saldo inicial corresponde al cierre anterior o a la suma antes del rango seleccionado.
5. No se almacena el saldo; se calcula en tiempo de consulta.
6. Solo movimientos de clientes vinculados aparecen en su estado de cuenta.
7. Los montos mostrados en moneda extranjera son conversiones visuales; el valor real permanece en MXN.
8. La conversión usa la tasa automática del día o la tasa manual configurada, según configuración del sistema.
9. Se debe mostrar la tasa de cambio aplicada cuando la visualización no sea MXN.

## Dependencias
- Fase 1: Foundation.
- Fase 2: Client Management.
- Fase 5: Billing Engine.
- Fase 6: Financial Ledger.
- Fase 7: Subscription Allocations.

## Criterios de Aceptación

- [ ] El estado de cuenta muestra cargos, pagos y ajustes.
- [ ] El saldo acumulado es correcto para cada fila.
- [ ] Los filtros de fecha funcionan correctamente.
- [ ] El resumen financiero coincide con el estado de cuenta.
- [ ] Un cliente sin movimientos muestra saldo 0.
- [ ] La visualización en otra moneda usa `ICurrencyConverter`.
- [ ] Se muestra la tasa de cambio aplicada.

## Notas Técnicas

- Implementar como query optimizada (raw SQL o LINQ proyectado).
- Considerar una vista SQL para el estado de cuenta si el volumen crece.
- Ordenar items por fecha ascendente.
- Paginar resultados para no sobrecargar la UI.
- Calcular valores en MXN primero; aplicar conversión al final para visualización.
- Cachear tasas de cambio del día para evitar llamadas repetidas.
