# AegiFinance — Master Blueprint

## 1. Resumen Ejecutivo

**AegiFinance** es una plataforma financiera web diseñada para administrar clientes, servicios, suscripciones, cargos recurrentes, pagos, estados de cuenta y conciliación bancaria. Opera en dos modalidades:

- **Modo Interno:** gestión personal de clientes y mensualidades (software, mantenimiento, hosting, soporte).
- **Modo SaaS:** administración comercial completa con licencias, suscripciones, portal cliente y automatizaciones.

El producto se construye en **15 fases**. Las fases 1-8 conforman el **MVP Real** operativo interno. Las fases 9-15 añaden escalabilidad empresarial, soporte, licenciamiento, portal cliente y automatizaciones.

## 2. Visión

Construir una plataforma financiera web capaz de operar en dos modos sin cambios arquitectónicos entre 5, 500 o 5,000 clientes.

### Modo Interno
- Administración personal de clientes y mensualidades.
- Ejemplos: clientes de software, mantenimiento, hosting y soporte.

### Modo SaaS
- Administración comercial completa.
- Ejemplos: venta de licencias, suscripciones, soporte, portal cliente y automatizaciones.

## 3. Principios de Diseño (No negociables)

| # | Principio | Descripción |
|---|-----------|-------------|
| 1 | **Auditabilidad total** | Todo movimiento financiero debe ser auditable. Nunca modificar saldos directamente; todo se genera mediante movimientos. |
| 2 | **Ledger First Architecture** | El ledger es la fuente de verdad. Todo saldo se calcula; nada se almacena calculado. |
| 3 | **Subscriptions Driven** | Todo ingreso recurrente proviene de una suscripción. |
| 4 | **Cliente ≠ Suscripción** | Un cliente puede tener 0, 1 o N suscripciones. |
| 5 | **Escalabilidad horizontal** | La plataforma debe funcionar con 5, 500 o 5,000 clientes sin cambios arquitectónicos. |

## 4. Stack Tecnológico

### Backend
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- MediatR
- FluentValidation
- Serilog

### Frontend
- React + TypeScript
- Next.js (App Router)
- AegisUI — sistema de diseño propio de AegiFinance, inspirado en MeridianUI (misma filosofía enterprise-soft) pero con paleta, tipografía e iconografía propias, sin copiar sus valores
- Radix UI (primitivas accesibles) + Tailwind CSS (estilos)
- TanStack Query (estado de servidor) + React Hook Form/Zod (formularios) + TanStack Table (tablas)
- lucide-react (iconografía)

> Detalle completo del stack frontend, arquitectura de carpetas, tokens AegisUI y reglas de teclado/mobile en `Plan1.2-Extension.md`.

### Base de Datos
- SQL Server 2022

### Infraestructura
- Docker
- Docker Compose
- Ubuntu Server
- Cloudflare Tunnel

### Reportes
- **Motor principal de PDF:** QuestPDF (generación nativa de PDF en .NET, API fluent, alto rendimiento).
- **Otros formatos:** ClosedXML para Excel (.xlsx) y generación de HTML para vista previa/impresión.
- Estilo visual AegisUI aplicado en todos los formatos.
- AegiReports (integración futura; por ahora se reserva el contrato de datos).

## 5. Clean Architecture

```
src/
├── AegiFinance.Domain       # Entidades, value objects, interfaces de dominio
├── AegiFinance.Application  # Casos de uso, DTOs, validaciones, handlers MediatR
├── AegiFinance.Infrastructure # EF Core, repositorios, servicios externos, email, jobs
├── AegiFinance.Web          # ASP.NET Core Web API (API-only; sin Razor Components)
└── AegiFinance.Worker       # Background services, generación de cargos, automatizaciones

web/                         # Next.js — React + TypeScript (frontend, proyecto hermano de src/)
```

### Reglas de dependencia
- `Domain` no depende de nadie.
- `Application` solo depende de `Domain`.
- `Infrastructure` depende de `Application` y `Domain`.
- `Web` y `Worker` dependen de `Application` e `Infrastructure`.
- `web/` (Next.js) consume `AegiFinance.Web` exclusivamente vía HTTP/JSON; no referencia ni compila contra los proyectos .NET.

> `AegiFinance.Web` deja de hospedar Blazor Server. Pasa a ser API-only (controladores existentes + CORS para el origen de Next.js). Ver ajustes detallados en `Plan1.2-Extension.md`.

## 6. Resumen de Fases

| Fase | Nombre | Objetivo | MVP |
|------|--------|----------|-----|
| 1 | Foundation | Base del sistema: autenticación, autorización, usuarios, roles, permisos, auditoría | Sí |
| 2 | Client Management | Administrar clientes, etiquetas, categorías, notas, contactos e historial | Sí |
| 3 | Service Catalog | Catálogo de servicios, categorías, tipos de facturación e historial de precios | Sí |
| 4 | Subscriptions | Contratos entre clientes y servicios con estados e historial | Sí |
| 5 | Billing Engine | Generación automática de cargos esperados mensuales/anuales | Sí |
| 6 | Financial Ledger | Núcleo financiero: cuentas bancarias, movimientos, transferencias, ajustes | Sí |
| 7 | Subscription Allocations | Aplicación de pagos a servicios específicos (automática/manual) | Sí |
| 8 | Customer Account Statements | Estados de cuenta por cliente con cargos, pagos, ajustes y saldo | Sí |
| 9 | Bank Reconciliation | Conciliación de bancos con estados de cuenta y reportes | No |
| 10 | Dashboard | Indicadores de negocio (MRR, ARR, morosidad, ingresos) | No |
| 11 | Reporting | Reportes empresariales con AegiReports | No |
| 12 | Support Tickets | Mesa de ayuda con tickets, comentarios y adjuntos | No |
| 13 | License Management | Administración de licencias de software e instalaciones | No |
| 14 | Customer Portal | Portal de autoservicio para clientes | No |
| 15 | Automations | Reducción de operación manual: recordatorios, vencimientos, suspensiones | No |

## 7. MVP Real

Para comenzar a usar el sistema internamente no es necesario llegar a la fase 15. La primera versión operativa queda terminada al finalizar las fases 1-8. Con esas fases ya se puede administrar:

- Clientes
- Planes / servicios
- Mensualidades
- Pagos
- Estados de cuenta
- Conciliación financiera básica

Todo lo posterior es crecimiento empresarial.

## 8. Entidades Globales (transversales)

### BaseEntity
- `Id` (GUID / int)
- `CreatedAt`
- `CreatedBy`
- `UpdatedAt`
- `UpdatedBy`
- `IsDeleted` (soft delete)

### AuditableEntity (hereda de BaseEntity)
- Traza completa de cambios por tabla de auditoría.

### UserType
Enum:
- `Administrator` — usuarios internos de la empresa (ClientId = NULL).
- `Client` — usuarios pertenecientes a un cliente (ClientId obligatorio).

### User
- `Id`
- `UserName`
- `Email`
- `PasswordHash`
- `UserType`
- `ClientId` (nullable; obligatorio cuando UserType = Client)
- `Name`
- `IsActive`
- `RefreshToken`
- `RefreshTokenExpiry`

### ClientUser
- `Id`
- `ClientId`
- `UserId`
- `DisplayName`
- `IsActive`

> Permite que cada cliente tenga subusuarios internos (ej. gerente1, contador1).

### ClientPinCredential
- `Id`
- `UserId`
- `PinHash`

> Credencial PIN simple para subusuarios de clientes. El usuario principal usa login normal con contraseña.

### Role
- `Id`
- `Name` (único)
- `Description`
- `UserType` (opcional: Admin roles / Client roles)

### Permission
- `Id`
- `Code` (ej. `ViewDashboard`, `ViewReports`, `ManageUsers`, `ViewSubscriptions`)
- `Name`
- `Description`

### SubscriptionPermission
- `Id`
- `UserId`
- `SubscriptionId`

> Controla qué suscripciones específicas puede ver un usuario de cliente.

## 9. Arquitectura de UI — Single View Architecture (SVA)

### Regla obligatoria
- Cada módulo tendrá **una única vista** (una sola ruta de Next.js) compartida entre administradores y clientes.
- Está prohibido crear componentes/rutas duplicadas como `dashboard/admin/page.tsx` / `dashboard/client/page.tsx`.
- El contenido se adapta mediante permisos: `{can('ViewRevenue') && ...}`, `{can('ManageUsers') && ...}`, usando el hook `usePermissions()` (ver sección 15).

### Ejemplos de vistas únicas (rutas Next.js)
- `app/(app)/dashboard/page.tsx`
- `app/(app)/clients/page.tsx`
- `app/(app)/subscriptions/page.tsx`
- `app/(app)/account-statement/page.tsx`
- `app/(app)/reports/page.tsx`

### Beneficios
- Un solo mantenimiento.
- Un solo flujo.
- Menor deuda técnica.
- Escalabilidad máxima.

## 10. Matriz de Seguridad

La autorización sigue la jerarquía:

```
Permission → Role → User → Client → Subscription
```

### Reglas
1. Los permisos definen qué puede hacer un usuario (`ViewDashboard`, `ManageUsers`, etc.).
2. Los roles agrupan permisos y son independientes del `UserType`.
3. Un `UserType = Client` siempre debe tener `ClientId` obligatorio.
4. Un `UserType = Administrator` siempre tiene `ClientId = NULL`.
5. Los usuarios `Client` solo ven datos donde `ClientId = suyo`.
6. Los `ClientSubUsers` pueden tener restricción adicional por suscripción (`SubscriptionPermission`).
7. El menú de navegación se construye dinámicamente a partir de los permisos del usuario.

## 11. Configuración Regional y Monetaria

### Moneda
- **Moneda de almacenamiento única:** Peso Mexicano (`MXN`).
  - Todos los montos de `Service`, `Subscription`, `BillingItem`, `LedgerEntry`, `BankAccount`, etc. se guardan siempre en MXN.
- **Visualización multi-moneda:** la UI y los reportes pueden mostrar los montos en la moneda que elija el usuario.
- **Conversión:** siempre se realiza desde MXN hacia la moneda de visualización.
- **Fuentes de tipo de cambio:**
  1. **Automática:** servicio externo de tipo de cambio (ej. ExchangeRate-API, Open Exchange Rates) con fallback manual si falla.
  2. **Manual:** tipo de cambio configurado por el usuario (unidades de moneda extranjera por 1 MXN o MXN por 1 unidad extranjera; se definirá en implementación).
- La tasa usada se registra junto con cada conversión para trazabilidad.

### Entidades de soporte de moneda

#### CurrencyConfig
- `Id`
- `Code` (ej. `USD`, `EUR`, `MXN`)
- `Name`
- `Symbol`
- `IsActive`
- `IsDefault` (solo una; default MXN)

#### ExchangeRate
- `Id`
- `CurrencyCode`
- `RateToMXN` (cuántos MXN equivalen a 1 unidad de moneda extranjera)
- `RateFromMXN` (cuántas unidades de moneda extranjera equivalen a 1 MXN)
- `EffectiveDate`
- `Source` (`Auto` / `Manual`)
- `CreatedBy`
- `CreatedAt`

### Fechas
- **Formato de presentación:** `22/Jun/2026` (ej. `dd/MMM/yyyy` en español México).
- **Zona horaria:** América/Ciudad de México (Jalisco) — `Central Standard Time (Mexico)`.
- **Almacenamiento:** UTC en base de datos; conversión a zona horaria local en UI y reportes.
- El `BillingDay` de suscripciones se valida entre 1 y 28.

## 12. Reglas de Negocio Transversales

1. Todo cambio en saldos debe realizarse mediante `LedgerEntry`.
2. Toda suscripción activa genera `BillingItem` en su ciclo correspondiente.
3. Un `BillingItem` puede estar pendiente, pagado parcialmente, pagado o anulado.
4. Un pago (`LedgerEntry` de tipo `Income`) se puede asignar a uno o varios `BillingItem`.
5. Un cliente siempre tiene un estado de cuenta calculado a partir de sus cargos y pagos.
6. Los usuarios solo pueden ver/modificar recursos autorizados por roles y permisos.
7. Los usuarios `Client` solo ven recursos filtrados por `ClientId`.
8. Los subusuarios de cliente pueden estar restringidos por `SubscriptionPermission`.
9. Toda funcionalidad nueva cumple SVA: una sola vista, permisos dinámicos, menús dinámicos.
10. Todo monto se almacena en MXN; las conversiones a otras monedas son solo visuales.
11. Las conversiones usan la tasa automática del día o la tasa manual configurada, registrando la fuente.
12. Toda acción crítica debe quedar registrada en auditoría.

## 13. Riesgos Identificados

| Riesgo | Impacto | Mitigación |
|--------|---------|------------|
| Ledger mal diseñado desde el inicio | Alto | Fase 6 con pruebas de conciliación forzadas antes de avanzar |
| Asignaciones de pagos complejas | Alto | Fase 7 con casos de uso explícitos y pruebas unitarias |
| Cálculo de cargos recurrentes con zonas horarias | Medio | Usar UTC para fechas de ciclo y conversión solo en UI |
| Escalabilidad prematura | Medio | Mantener queries proyectadas; no almacenar saldos calculados |
| Reportes estáticos PDF fuera de estilo AegisUI | Medio | Definir plantilla base AegisUI antes de implementar fase 11 |
| Frontend React termina pareciéndose visualmente a MeridianUI | Medio | Usar exclusivamente los tokens propios de AegisUI (`Plan1.2-Extension.md`); prohibido reutilizar hex/tipografía de MeridianUI |
| Filtrado por cliente mal implementado (fuga de datos) | Alto | Centralizar filtro `ClientId` en repositorios y validar en cada query |
| Conversión de moneda mal calculada | Medio | Centralizar servicio `ICurrencyConverter` con traza de tasa usada |
| Dependencia de servicio externo de tipo de cambio | Bajo | Soportar fallback a tasa manual y cache local
| Permisos y roles acoplados a UserType | Medio | Mantener `Role` independiente de `UserType`; validar en seed |
| Licenciamiento fuera de alcance inicial | Bajo | Postergar a fase 13; no modelar tablas antes de tiempo |

## 14. Sistema de Diseño — AegisUI

AegiFinance usa **AegisUI**, sistema de diseño propio inspirado en la filosofía de MeridianUI (densidad controlada, color semántico, elevación sutil, interacción silenciosa, más una quinta regla propia: **teclado primero**), pero con valores visuales independientes para no percibirse como una copia de otro producto Keorsoft.

| Elemento | MeridianUI (SHEndevour) | AegisUI (AegiFinance) |
|----------|--------------------------|------------------------|
| Primario | Material Blue `#1976D2` | Azul-petróleo `#0F5C6B` |
| Semánticos | Emerald / Amber / Indigo / Violet / Orange | Jade / Saffron / Periwinkle / Plum / Terracotta |
| Tipografía UI | Segoe UI (→ Inter en web) | Manrope |
| Tipografía display | Montserrat | Sora |
| Iconografía | Material Symbols Rounded | lucide-react |
| Radios | 6 / 10 / 14 / 20 px | 7 / 9 / 12 / 18 px |
| Layout desktop | Titlebar 48 + Sidebar 220 + Statusbar 28 | Topbar 56 + Sidebar 248 (76 colapsado) + Status strip 32 |

Especificación completa de tokens (paleta de 5 niveles, tipografía, espaciado, elevación, layout mobile) en `Plan1.2-Extension.md`. Ningún valor hexadecimal ni proporción de MeridianUI se reutiliza literalmente en AegisUI.

## 15. Arquitectura Frontend (React + Next.js)

- Proyecto `web/` hermano de `src/`, consumiendo `AegiFinance.Web` solo vía HTTP/JSON.
- Next.js App Router, TypeScript estricto, Radix UI + Tailwind CSS, TanStack Query/Table, React Hook Form + Zod, cmdk (paleta de comandos).
- SVA aplicada como una ruta por módulo (`app/(app)/<modulo>/page.tsx`); contenido condicionado por `usePermissions()` / `<Can permission="..." />`.
- Autenticación: access token JWT en memoria (nunca `localStorage`); refresh token en cookie `HttpOnly`/`Secure`; `GET /api/auth/me` como fuente de permisos efectivos y menú dinámico.
- Reglas obligatorias de teclado (paleta de comandos, navegación de tablas, atajos de alta/edición) y de eficiencia de captura en desktop/mobile (alta inline, optimistic UI, autosave de borradores, inputs nativos en mobile).
- Detalle completo en `Plan1.2-Extension.md`.

## 16. Handoff para Ryou Orchestrator

**Próximo trabajo a ejecutar:**
1. Implementar la Fase 1 (Foundation) completa: autenticación JWT, refresh tokens, `UserType`, `ClientUser`, `ClientPinCredential`, `Role`, `Permission`, `SubscriptionPermission`, seed de permisos y auditoría. *(Backend de Fase 1 ya implementado — ver `progress.md`; pendiente exponer `GET /api/auth/me` y mover refresh token a cookie `HttpOnly`.)*
2. Configurar la solución como `AegiFinance.*` (renombrar desde `SHE.Finance.*`). *(Completado.)*
3. Ejecutar Sprint F0 de `Plan1.2-Extension.md`: crear proyecto `web/` (Next.js), tokens AegisUI, primitivas base, capa de autenticación y ajustes de CORS/cookies en `AegiFinance.Web`.
4. Migrar el frontend módulo por módulo siguiendo el orden de Sprints F1-F8 (MVP) y F9-F15 (post-MVP) de `Plan1.2-Extension.md`, retirando cada vista Razor equivalente al completar su réplica en React.
5. No avanzar a una fase nueva de backend sin superar su gate de verificación; no retirar una vista Razor sin que su equivalente en Next.js cumpla los gates de UX/UI y teclado de `verification.md`.

**Lo que este blueprint NO incluye (queda fuera del alcance):**
- Código fuente de implementación.
- Configuración de CI/CD.
- Diseño detallado de UX/UI más allá de los tokens AegisUI definidos en `Plan1.2-Extension.md`.
- Contratos comerciales con terceros.
- Despliegue en producción.
