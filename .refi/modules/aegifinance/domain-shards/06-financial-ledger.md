# Fase 6 — Financial Ledger

## Objetivo
Crear el núcleo financiero del sistema. El ledger es la fuente de verdad de todos los saldos. Ningún saldo se almacena calculado; todo se obtiene agregando movimientos.

## Entidades

### BankAccount
- `Id`
- `Name`
- `BankName`
- `AccountNumber` (últimos 4 dígitos visibles, resto encriptado)
- `Currency`
- `OpeningBalance`
- `OpeningDate`
- `IsActive`

### LedgerEntryType
Enum / lookup:
- `Income`
- `Expense`
- `TransferIn`
- `TransferOut`
- `Adjustment`

### LedgerEntry
- `Id`
- `BankAccountId`
- `EntryType`
- `Amount` (decimal, siempre positivo; el tipo define el signo)
- `Currency`
- `Date`
- `Description`
- `Reference` (número de referencia externa)
- `ClientId` (nullable)
- `BillingItemId` (nullable)
- `IsReconciled`
- `ReconciledAt`
- `CreatedBy`
- `CreatedAt`

### LedgerAllocation
- `Id`
- `LedgerEntryId` (pago)
- `BillingItemId`
- `Amount`
- `CreatedAt`

### TransferGroup
- `Id`
- `FromEntryId`
- `ToEntryId`
- `Amount`
- `Date`
- `Description`

## Casos de Uso

1. **Registrar ingreso** (pago de cliente, depósito).
2. **Registrar egreso** (gasto, retiro).
3. **Registrar transferencia** entre cuentas bancarias.
4. **Registrar ajuste** con motivo.
5. **Ver movimientos** por cuenta bancaria.
6. **Consultar saldo** calculado de una cuenta.
7. **Reconciliar** movimientos con estado de cuenta bancario.

## Entregables UI/API

### API
- `GET/POST/PUT/DELETE /api/bank-accounts`
- `GET /api/bank-accounts/{id}/balance`
- `GET /api/bank-accounts/{id}/entries`
- `POST /api/ledger/income`
- `POST /api/ledger/expense`
- `POST /api/ledger/transfer`
- `POST /api/ledger/adjustment`
- `POST /api/ledger/{id}/reconcile`

### UI
- Pantalla de cuentas bancarias.
- Pantalla de movimientos con filtros.
- Formulario de ingreso/egreso.
- Formulario de transferencia.
- Vista de conciliación.
- Indicador de saldo calculado por cuenta.

## Reglas de Negocio

1. Todo saldo se calcula; no se almacena calculado.
2. Las transferencias generan dos movimientos vinculados por `TransferGroup`.
3. Los ajustes requieren motivo y aprobación según permiso.
4. Un movimiento reconciliado no puede editarse ni eliminarse.
5. El monto siempre es positivo; el tipo define el efecto en el saldo.
6. Un ingreso puede estar asociado a un cliente y a items de facturación (vía `LedgerAllocation`).

## Dependencias
- Fase 1: Foundation.
- Fase 2: Client Management (opcional para `ClientId`).
- Fase 5: Billing Engine (para `BillingItemId`).

## Criterios de Aceptación

- [ ] El saldo de una cuenta es la suma de opening balance + ingresos - egresos + transferencias + ajustes.
- [ ] Una transferencia actualiza dos cuentas.
- [ ] Los movimientos reconciliados están bloqueados para edición.
- [ ] Los ajustes quedan registrados con motivo.
- [ ] Se puede filtrar movimientos por tipo, fecha y cuenta.

## Notas Técnicas

- Usar una vista o query calculada para el saldo.
- Las transferencias deben usar transacción atómica.
- El `Amount` siempre positivo evita errores de signo.
- Encriptar número de cuenta completo; mostrar solo últimos 4 dígitos.
- El monto de cada movimiento se almacena siempre en MXN; la visualización en otra moneda usa `ICurrencyConverter`.
