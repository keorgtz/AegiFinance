# AegiFinance — Progress Tracker

## Estado General

| Fase | Nombre | Estado | Inicio | Fin | Owner |
|------|--------|--------|--------|-----|-------|
| 1 | Foundation | 🟢 Completado | 22/Jun/2026 | 22/Jun/2026 | Ryou Orchestrator |
| 2 | Client Management | 🟢 Completado | 22/Jun/2026 | 22/Jun/2026 | Ryou Orchestrator |
| 3 | Service Catalog | 🟢 Completado | 22/Jun/2026 | 22/Jun/2026 | Ryou Orchestrator |
| 4 | Subscriptions | 🟢 Completado | 22/Jun/2026 | 22/Jun/2026 | Ryou Orchestrator |
| 5 | Billing Engine | 🟢 Completado | 22/Jun/2026 | 22/Jun/2026 | Ryou Orchestrator |
| 6 | Financial Ledger | 🟢 Completado | 22/Jun/2026 | 22/Jun/2026 | Ryou Orchestrator |
| 7 | Subscription Allocations | 🟢 Completado | 22/Jun/2026 | 22/Jun/2026 | Ryou Orchestrator |
| 8 | Customer Account Statements | 🟢 Completado | 22/Jun/2026 | 22/Jun/2026 | Ryou Orchestrator |
| 9 | Bank Reconciliation | ⚪ Pendiente | — | — | — |
| 10 | Dashboard | ⚪ Pendiente | — | — | — |
| 11 | Reporting | ⚪ Pendiente | — | — | — |
| 12 | Support Tickets | ⚪ Pendiente | — | — | — |
| 13 | License Management | ⚪ Pendiente | — | — | — |
| 14 | Customer Portal | ⚪ Pendiente | — | — | — |
| 15 | Automations | ⚪ Pendiente | — | — | — |

### Leyenda
- 🔵 Planificado
- 🟡 En progreso
- 🟢 Completado
- ⚪ Pendiente
- 🔴 Bloqueado

## Tareas Completadas

- [x] Análisis completo de `Plan1-Maestro.md`.
- [x] Lectura y análisis de `ExtensionPlan1.md` (portal integrado, SVA, permisos).
- [x] Creación de REFI packet en `.refi/modules/aegifinance/`.
- [x] Definición de master blueprint.
- [x] Definición de 15 domain shards.
- [x] Creación de orchestration map.
- [x] Definición de criterios de verificación.
- [x] Actualización de planes con: `UserType`, `ClientUser`, `ClientPinCredential`, `SubscriptionPermission`, SVA, MeridianUI, moneda MXN, formato de fechas y zona horaria México.

## Decisiones Confirmadas

- [x] Nombre oficial: `AegiFinance`.
- [x] Moneda de almacenamiento: Peso Mexicano (`MXN`).
- [x] Visualización multi-moneda: sí, con conversión desde MXN.
- [x] Fuentes de tipo de cambio: automática (servicio externo) y manual.
- [x] Formato de fechas: `22/Jun/2026`.
- [x] Zona horaria: América/Ciudad de México (Jalisco).
- [x] Motor de reportes: QuestPDF (PDF), ClosedXML (Excel), HTML (vista previa).
- [x] Reportes: estáticos con estilo MeridianUI; AegiReports en el futuro.
- [x] UX/UI: MeridianUI.
- [x] Portal cliente: integrado dentro de AegiFinance, sin app separada, con SVA.

## Puntos de Decisión Pendientes

- [ ] Seleccionar servicio externo de tipo de cambio (ExchangeRate-API, Open Exchange Rates, etc.).
- [ ] Confirmar canales de notificación futuros (email / WhatsApp / SMS).
- [ ] Estrategia de cache de tasas de cambio.

## Próximo Trabajo de Implementación

**Fase 1 — Foundation**
1. Crear solución .NET 9 con Clean Architecture bajo namespace `AegiFinance.*`.
2. Configurar Docker Compose y SQL Server.
3. Configurar cultura `es-MX` y zona horaria México.
4. Implementar autenticación JWT y refresh tokens.
5. Implementar `UserType`, `ClientUser`, `ClientPinCredential`.
6. Implementar `Role`, `Permission`, `UserPermission`, `SubscriptionPermission`.
7. Implementar Permission Engine y filtrado por `ClientId`.
8. Implementar auditoría automática.
9. Crear migraciones iniciales.
10. Validar endpoints de login/logout y login con PIN.

## Métricas de Avance

- Planificación: **100%**
- Implementación: **53%** (Fases 1-8 completadas de 15)
- Pruebas: **0%**
- Documentación: **100%** (planificación)

## Notas

- Fases 1, 2, 3, 4, 5, 6, 7 y 8 implementadas y compilando correctamente.
- Fase 1: Foundation (auth, permisos, moneda, auditoría).
- Fase 2: Client Management (clientes, etiquetas, categorías, notas, contactos).
- Fase 3: Service Catalog (servicios, categorías, historial de precios, tipos de facturación).
- Fase 4: Subscriptions (suscripciones, estados, historial de precios, logs de cambios).
- Fase 5: Billing Engine (ciclos, items, logs, motor de generación, worker diario).
- Fase 6: Financial Ledger (cuentas bancarias, ledger entries, allocations, transferencias, conciliación y cálculo de saldos).
- Fase 7: Subscription Allocations (asignación automática FIFO, manual, desasignación y actualización de estados de billing items).
- Fase 8: Customer Account Statements (estado de cuenta, movimientos y resumen financiero por cliente con conversión visual de moneda).
- Migraciones: `InitialCreate`, `AddClientManagement`, `AddServiceCatalog`, `AddSubscriptions`, `AddBillingEngine`, `AddFinancialLedger` y `AddSubscriptionAllocations` generadas.
- Próximo paso: continuar con Fase 9 — Bank Reconciliation.
