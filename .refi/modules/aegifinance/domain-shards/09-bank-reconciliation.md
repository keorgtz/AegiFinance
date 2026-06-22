# Fase 9 — Bank Reconciliation

## Objetivo
Permitir conciliar los movimientos registrados en el ledger contra los estados de cuenta bancarios reales.

## Entidades

### BankStatement
- `Id`
- `BankAccountId`
- `StatementDate`
- `StartDate`
- `EndDate`
- `OpeningBalance`
- `ClosingBalance`
- `FileUrl`
- `UploadedBy`
- `UploadedAt`

### BankStatementLine
- `Id`
- `BankStatementId`
- `TransactionDate`
- `Description`
- `Reference`
- `Amount` (positivo ingreso, negativo egreso)
- `IsReconciled`
- `LedgerEntryId` (nullable)

### ReconciliationReport (vista / DTO)
- `BankAccountId`
- `StatementId`
- `LedgerBalance`
- `StatementBalance`
- `Difference`
- `MatchedCount`
- `UnmatchedCount`

## Casos de Uso

1. **Subir estado de cuenta bancario** (CSV/Excel).
2. **Listar líneas del estado de cuenta**.
3. **Conciliar automáticamente** por monto + referencia + fecha.
4. **Conciliar manualmente** una línea con un `LedgerEntry`.
5. **Desconciliar** una línea.
6. **Generar reporte de conciliación**.
7. **Marcar diferencias** no conciliadas.

## Entregables UI/API

### API
- `POST /api/bank-statements/upload`
- `GET /api/bank-statements`
- `GET /api/bank-statements/{id}/lines`
- `POST /api/bank-statements/{id}/auto-reconcile`
- `POST /api/bank-statement-lines/{id}/match`
- `POST /api/bank-statement-lines/{id}/unmatch`
- `GET /api/bank-statements/{id}/reconciliation-report`

### UI
- Pantalla de estados de cuenta bancarios.
- Vista de líneas con estados conciliado/pendiente.
- Botón de conciliación automática.
- Modal para emparejar manualmente.
- Reporte de diferencias.

## Reglas de Negocio

1. Una línea conciliada debe coincidir con un `LedgerEntry` no reconciliado.
2. El monto de la línea debe coincidir con el monto del movimiento.
3. Una conciliación automática debe ser revisable y deshacible.
4. El reporte muestra diferencia entre saldo ledger y saldo bancario.
5. No se puede eliminar un estado de cuenta con líneas conciliadas.

## Dependencias
- Fase 1: Foundation.
- Fase 6: Financial Ledger.

## Criterios de Aceptación

- [ ] Se puede subir un archivo de estado de cuenta.
- [ ] Se listan las líneas correctamente.
- [ ] La conciliación automática empareja por monto y referencia.
- [ ] La conciliación manual permite seleccionar un movimiento.
- [ ] El reporte muestra diferencia y cantidad de emparejamientos.
- [ ] Se puede deshacer una conciliación.

## Notas Técnicas

- Parser de CSV/Excel debe soportar formatos comunes de bancos.
- Almacenar archivo en disco/cloud según configuración.
- Usar transacción al marcar conciliaciones.
- Diferencia = `LedgerBalance - StatementBalance`.
