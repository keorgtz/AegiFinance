# AegiFinance — Orchestration Map

## Secuencia de Ejecución

Las fases deben ejecutarse en orden numérico. Cada fase depende de las anteriores dentro del MVP (1-8). Las fases 9-15 son de crecimiento y dependen del MVP.

```
FASE 1: Foundation
   │
   ▼
FASE 2: Client Management
   │
   ▼
FASE 3: Service Catalog
   │
   ▼
FASE 4: Subscriptions
   │
   ▼
FASE 5: Billing Engine
   │
   ▼
FASE 6: Financial Ledger
   │
   ▼
FASE 7: Subscription Allocations
   │
   ▼
FASE 8: Customer Account Statements
   │
   ├──▶ FASE 9: Bank Reconciliation
   │
   ├──▶ FASE 10: Dashboard
   │
   ├──▶ FASE 11: Reporting
   │
   ├──▶ FASE 12: Support Tickets
   │
   ├──▶ FASE 13: License Management
   │
   ├──▶ FASE 14: Customer Portal
   │
   └──▶ FASE 15: Automations
```

## Mapa de Dependencias

| Fase | Depende de |
|------|------------|
| 1 Foundation | — |
| 2 Client Management | 1 |
| 3 Service Catalog | 1 |
| 4 Subscriptions | 1, 2, 3 |
| 5 Billing Engine | 1, 2, 3, 4 |
| 6 Financial Ledger | 1, 2 (opcional), 5 (opcional) |
| 7 Subscription Allocations | 1, 2, 5, 6 |
| 8 Customer Account Statements | 1, 2, 5, 6, 7 |
| 9 Bank Reconciliation | 1, 6 |
| 10 Dashboard | 1, 2, 4, 5, 6, 7, 8 |
| 11 Reporting | 1, 2, 3, 4, 5, 6, 8, 9 |
| 12 Support Tickets | 1, 2 |
| 13 License Management | 1, 2, 4 (opcional) |
| 14 Customer Portal | 1, 2, 4, 5, 6, 8, 12, 13 |
| 15 Automations | 1, 2, 4, 5, 6, 8 |

## MVP Real (Fases 1-8)

El producto ya es operativo internamente al terminar la fase 8. Esto permite:

- Administrar clientes, servicios y suscripciones.
- Generar cargos recurrentes automáticamente.
- Registrar ingresos, egresos, transferencias y ajustes.
- Asignar pagos a cargos.
- Generar estados de cuenta por cliente.

## Agrupación por Dominios

| Dominio | Fases |
|---------|-------|
| Identidad y Seguridad | 1 |
| CRM (Clientes) | 2 |
| Catálogo de Productos | 3 |
| Suscripciones | 4 |
| Facturación | 5, 7, 8 |
| Finanzas | 6, 9 |
| Business Intelligence | 10, 11 |
| Soporte | 12 |
| Licenciamiento | 13 |
| Portal Cliente | 14 |
| Operaciones Automáticas | 15 |

## Plan de Implementación Recomendado

### Sprint 0 — Preparación
- Crear solución .NET 9 con proyectos Clean Architecture bajo el namespace `AegiFinance.*`.
- Configurar Docker Compose con SQL Server.
- Configurar Serilog y base de logging.
- Configurar zona horaria México (Central Standard Time) y cultura `es-MX`.
- Definir tokens de MeridianUI para Blazor/MudBlazor.

### Sprints 1-3 — Fase 1 (Foundation)
- Autenticación JWT + refresh tokens.
- `UserType`, `ClientUser`, `ClientPinCredential`.
- `Role`, `Permission`, `UserPermission`, `SubscriptionPermission`.
- Permission Engine y filtrado por `ClientId`.
- Auditoría automática.

### Sprints 4-5 — Fase 2 (Client Management)
- CRUD de clientes, etiquetas, categorías, contactos y notas.

### Sprint 6 — Fase 3 (Service Catalog)
- Catálogo de servicios, categorías e historial de precios.

### Sprints 7-8 — Fase 4 (Subscriptions)
- Suscripciones con estados, historial de precios y cambios.

### Sprints 9-10 — Fase 5 (Billing Engine)
- Generación de cargos, ciclos y worker.

### Sprints 11-12 — Fase 6 (Financial Ledger)
- Cuentas bancarias, movimientos, transferencias y ajustes.

### Sprint 13 — Fase 7 (Subscription Allocations)
- Asignación automática y manual de pagos.

### Sprint 14 — Fase 8 (Customer Account Statements)
- Estados de cuenta y resumen financiero.

### Post-MVP
- Fases 9-15 según prioridad de negocio.

## Decisiones Confirmadas

1. **Nombre oficial:** `AegiFinance` (proyectos `AegiFinance.*`).
2. **Moneda de almacenamiento:** Peso Mexicano (`MXN`) obligatorio.
3. **Visualización multi-moneda:** sí, mediante conversión desde MXN.
4. **Fuentes de tipo de cambio:** automática (servicio externo configurable) y manual configurable.
5. **Formato de fechas:** `22/Jun/2026` (`dd/MMM/yyyy` en `es-MX`).
6. **Zona horaria:** América/Ciudad de México (Jalisco) — `Central Standard Time (Mexico)`.
7. **Motor de reportes:** QuestPDF para PDF, ClosedXML para Excel (.xlsx), HTML para vista previa.
8. **Reportes:** estáticos generados automáticamente con estilo MeridianUI; AegiReports se integra en el futuro.
9. **UX/UI:** MeridianUI (design system oficial de Keorsoft).
10. **Portal cliente:** Integrado dentro de AegiFinance, sin aplicación separada, siguiendo Single View Architecture (SVA).

## Puntos de Decisión Pendientes

1. **Servicio externo de tipo de cambio:** ¿ExchangeRate-API, Open Exchange Rates, otro?
2. **Notificaciones:** ¿solo email o también WhatsApp/SMS en el futuro?
3. **Motor de conversión multi-moneda:** ¿conversiones automáticas con cache o siempre consulta en tiempo real?
