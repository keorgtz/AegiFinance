# PLAN_UPDATE_FRONTEND_STACK.md

# AegiFinance

## Migración de Frontend a React + TypeScript + Next.js y Nuevo Sistema de Diseño AegisUI

---

# OBJETIVO

Sustituir el frontend de **Blazor Web App + MudBlazor** por una aplicación **React + TypeScript + Next.js** independiente, consumiendo el mismo backend **C# / ASP.NET Core Web API** (Clean Architecture) ya existente.

El backend NO cambia de tecnología. Cambia únicamente el canal de presentación.

La identidad visual deja de ser MeridianUI literal y pasa a ser **AegisUI**: un sistema de diseño propio, inspirado en MeridianUI (misma filosofía "enterprise-soft", misma disciplina de tokens y densidad), pero con paleta, tipografía, iconografía y proporciones propias, para que AegiFinance no se perciba como una copia visual de otro producto Keorsoft.

---

# POR QUÉ

1. Mantener Blazor Server obligaba a renderizado en el mismo proceso .NET; React + Next.js permite una SPA/SSR moderna, más eficiente para formularios densos y atajos de teclado.
2. MeridianUI es el lenguaje visual de SHEndevour (WPF/Blazor). AegiFinance necesita su propia identidad de marca dentro de la familia Keorsoft, sin perder la disciplina de diseño.
3. El objetivo de uso real (alta y edición de registros rápida, en computadora o teléfono) exige una capa de UI con primitivas accesibles y control total del comportamiento de teclado, algo más directo de lograr en React que en MudBlazor.

---

# REGLA NO NEGOCIABLE

El backend (`AegiFinance.Domain`, `AegiFinance.Application`, `AegiFinance.Infrastructure`, controladores de `AegiFinance.Web`) se mantiene. No se reescribe lógica de negocio, handlers de MediatR, entidades ni reglas de Clean Architecture. Solo se reemplaza la capa de presentación Blazor/Razor por consumo HTTP desde Next.js.

---

# STACK ACTUALIZADO

## Backend (sin cambios)

.NET 9 · ASP.NET Core Web API · Entity Framework Core · MediatR · FluentValidation · Serilog

## Frontend (NUEVO)

| Capa | Elección | Motivo |
|------|----------|--------|
| Framework | **Next.js** (App Router) | SSR/SSG híbrido, rutas por carpeta, óptimo para SEO del portal cliente y carga inicial rápida |
| Lenguaje | **TypeScript** (strict mode) | Contratos de datos seguros contra los DTOs del backend |
| UI primitives | **Radix UI** (unstyled, accesible) | Diálogos, menús, popovers, combobox con accesibilidad y manejo de teclado correctos de fábrica |
| Estilos | **Tailwind CSS** + tokens propios (`styles/tokens.css`) | Velocidad de implementación + control total de la identidad visual AegisUI |
| Server state / cache | **TanStack Query** | Cache, invalidación y estados de carga contra la Web API existente |
| Formularios | **React Hook Form** + **Zod** | Validación tipada, rendimiento en formularios densos, mensajes de error consistentes |
| Tablas | **TanStack Table** | Tablas densas con orden, filtros y navegación por teclado |
| Paleta de comandos | **cmdk** | `Ctrl/Cmd+K` para navegación y acciones rápidas (ver sección Teclado) |
| Iconografía | **lucide-react** | Set de líneas, deliberadamente distinto a Material Symbols Rounded de MeridianUI |
| Tipografía | **Manrope** (UI/cuerpo) + **Sora** (títulos/display) | Distinta a Segoe UI/Inter + Montserrat de MeridianUI |
| Gráficos | **Recharts** o **visx** | Tendencias de Dashboard/Reporting |
| Fechas | **date-fns** (locale `es`) | Formato `dd/MMM/yyyy`, zona horaria México |

## Calidad / Tooling

- ESLint + Prettier con reglas estrictas de accesibilidad (`eslint-plugin-jsx-a11y`).
- Vitest + React Testing Library para pruebas unitarias de componentes y hooks.
- Playwright (opcional, post-MVP) para flujos críticos: login, alta de cliente, registro de pago.
- Generación de cliente HTTP tipado desde el Swagger/OpenAPI del backend (`openapi-typescript` u `orval`) para evitar drift entre DTOs de C# y tipos de TypeScript.

---

# ARQUITECTURA DE CARPETAS (NUEVO PROYECTO `web/`)

El frontend vive como proyecto hermano de `src/`, no dentro de él:

```
AegiFinance/
├── src/
│   ├── AegiFinance.Domain
│   ├── AegiFinance.Application
│   ├── AegiFinance.Infrastructure
│   ├── AegiFinance.Web          # pasa a ser API-only (ver "Ajustes de Backend")
│   └── AegiFinance.Worker
├── web/                          # NUEVO — Next.js
│   ├── app/
│   │   ├── (auth)/
│   │   │   └── login/page.tsx
│   │   ├── (app)/
│   │   │   ├── layout.tsx        # Topbar + Sidebar + área de contenido
│   │   │   ├── dashboard/page.tsx
│   │   │   ├── clients/
│   │   │   │   ├── page.tsx
│   │   │   │   └── [id]/page.tsx
│   │   │   ├── services/page.tsx
│   │   │   ├── subscriptions/page.tsx
│   │   │   ├── billing/page.tsx
│   │   │   ├── ledger/page.tsx
│   │   │   ├── allocations/page.tsx
│   │   │   ├── account-statement/page.tsx
│   │   │   ├── reconciliation/page.tsx
│   │   │   ├── reports/page.tsx
│   │   │   ├── tickets/page.tsx
│   │   │   ├── licenses/page.tsx
│   │   │   └── settings/page.tsx
│   │   └── layout.tsx             # raíz: fuentes, providers globales
│   ├── components/
│   │   ├── ui/                    # primitivas AegisUI (Button, Input, Dialog, Table, Command, KpiCard...)
│   │   ├── layout/                 # Topbar, Sidebar, MobileNav, StatusStrip
│   │   └── modules/                 # componentes compuestos por dominio (ClientForm, LedgerEntryRow...)
│   ├── lib/
│   │   ├── api/                    # cliente HTTP tipado + hooks TanStack Query por dominio
│   │   ├── auth/                    # contexto de sesión, usePermissions(), <Can />
│   │   └── utils/
│   ├── hooks/
│   ├── styles/
│   │   └── tokens.css              # variables CSS AegisUI (ver sección Diseño)
│   └── types/                      # tipos generados desde OpenAPI
```

No existe `Pages` ni `Components/Pages` de Blazor en el estado final; `src/AegiFinance.Web/Components/*` se elimina una vez migrado cada módulo (ver "Plan de Migración").

---

# SINGLE VIEW ARCHITECTURE (SVA) EN REACT

La regla definida en `Plan1.1-Extension.md` se mantiene íntegra; solo cambia el lenguaje de los ejemplos.

## Prohibido

```text
components/modules/DashboardAdmin.tsx
components/modules/DashboardClient.tsx
components/modules/DashboardManager.tsx
```

## Correcto

```text
app/(app)/dashboard/page.tsx   →  <Dashboard />
```

Una sola ruta, un solo componente, contenido condicionado por permisos.

## Control de visibilidad

```tsx
const { can } = usePermissions();

{can('ViewRevenue') && <RevenueCard />}
{can('ManageUsers') && <UsersSection />}
{can('ViewReports') && <ReportsLink />}
```

O mediante el componente guard equivalente:

```tsx
<Can permission="ViewRevenue">
  <RevenueCard />
</Can>
```

`usePermissions()` lee los permisos efectivos devueltos por `GET /api/auth/me` (ver "Ajustes de Backend"). El frontend **nunca** decide autorización por sí mismo: solo refleja lo que el backend ya autorizó. Toda validación real ocurre en la API.

---

# SISTEMA DE DISEÑO — AEGISUI

> Inspirado en la filosofía de MeridianUI (densidad controlada, color semántico, elevación sutil, interacción silenciosa). Paleta, tipografía, iconografía y proporciones son propias de AegiFinance — no se reutilizan los valores de MeridianUI.

## Filosofía

Las mismas cuatro reglas de MeridianUI, igual de válidas aquí:

- Densidad controlada — mucha información, nunca abrumadora.
- Color semántico — cada color carga un significado y se usa siempre igual.
- Elevación sutil — sombras ligeras, nunca bordes de color para jerarquía.
- Interacción silenciosa — transiciones ≤ 200 ms, sin bounce, sin escala en hover.

A esto se agrega una quinta regla propia de AegisUI, por el objetivo de eficiencia de captura:

- **Teclado primero** — todo flujo de alta/edición debe ser completable sin tocar el mouse.

## Paleta semántica (5 roles, valores propios)

| Rol | Uso | Strong | Mid | Light | Pale | Bg |
|-----|-----|--------|-----|-------|------|-----|
| **Jade** — Éxito / Ingreso | pagos, altas exitosas | `#0E9F6E` | `#34C295` | `#8FE3C4` | `#DFFBEF` | `#F3FFFA` |
| **Saffron** — Alerta / Pendiente | cargos pendientes, vencimientos | `#B7791F` | `#D99A2B` | `#F0C36D` | `#FBEACB` | `#FFF8EC` |
| **Periwinkle** — Referencia / Cargos | montos neutros, info | `#5469D4` | `#7B8FE8` | `#B6C2F5` | `#E4E9FC` | `#F5F7FF` |
| **Plum** — Total / Resumen | KPIs de totales, abonos | `#A1336B` | `#C2528A` | `#E2A0C3` | `#F8E1EE` | `#FDF3F8` |
| **Terracotta** — Gasto / Salida | egresos, advertencias | `#B6452C` | `#D06A4A` | `#EBAA92` | `#FBE6DC` | `#FFF6F1` |

## Acción primaria (CTA)

| Token | Valor |
|-------|-------|
| `--primary` (petróleo) | `#0F5C6B` |
| `--primary-mid` | `#16798C` |
| `--primary-light` | `#5BAEBC` |
| `--primary-pale` | `#C9E8ED` |

> El primario de AegisUI es un **azul-petróleo**, deliberadamente distinto del Material Blue (`#1976D2`) de MeridianUI.

## Neutros

| Token | Valor | Uso |
|-------|-------|-----|
| `--ink` | `#16181D` | Texto principal |
| `--slate-700` | `#3A3F4B` | Texto secundario / celdas |
| `--slate-muted` | `#5B6472` | Labels, metadatos |
| `--line` | `#E3E6EC` | Bordes, divisores |
| `--foot` | `#F7F8FA` | Fondos de footer de tarjeta |
| `--page-bg` | `#EFF1F7` | Fondo de página |
| `--surface-dark` | `#12141A` | Sidebar / topbar en variante oscura |
| `--white` | `#FFFFFF` | Superficies de tarjeta |

## Tipografía

```
--font-ui: 'Manrope', system-ui, sans-serif;       /* cuerpo, formularios, tablas */
--font-display: 'Sora', 'Manrope', sans-serif;     /* títulos de página/sección, KPIs */
```

| Rol | Tamaño | Peso | Notas |
|-----|--------|------|-------|
| Page title | 22 px | 600 | Sora |
| Section title | 16–18 px | 700 | Sora |
| KPI label | 11 px | 600 | UPPERCASE, letter-spacing 0.6 px |
| KPI value | 28–40 px | 700 | color semántico, `tabular-nums` |
| Body / tabla | 13 px | 400–500 | Manrope |
| Meta / sub-label | 10–11 px | 600 | UPPERCASE, `--slate-muted` |
| Importes | 13–15 px | 700 | `font-variant-numeric: tabular-nums` siempre |

## Espaciado y radios

Base 4 px, igual disciplina que MeridianUI, escala propia:

| Token | Valor |
|-------|-------|
| `--s1`…`--s6` | 4 / 8 / 12 / 16 / 20 / 24 px |
| `--r-input` | 7 px |
| `--r-button` | 9 px |
| `--r-table` | 12 px |
| `--r-card` | 18 px |

## Elevación

| Nivel | Sombra | Uso |
|-------|--------|-----|
| Dp0 | ninguna | elementos en `--page-bg` |
| Dp1 | `0 1px 2px rgba(15,23,42,.06), 0 1px 1px rgba(15,23,42,.04)` | tarjetas, filas hover |
| Dp2 | `0 4px 10px rgba(15,23,42,.10)` | dropdowns, popovers, command palette |
| Dp3 | `0 12px 28px rgba(15,23,42,.16)` | modales, drawers |

## Iconografía

**lucide-react** (líneas, 1.5 px de grosor) en vez de Material Symbols Rounded. Tamaño estándar 18 px en sidebar/inputs, 16 px en chips/tablas.

## Layout de aplicación (desktop ≥ 1280 px)

```
┌─────────────────────────────────────────────────┐
│  TOPBAR  (56 px, sticky) — logo, breadcrumb,    │
│           command palette trigger, avatar       │
├──────────┬──────────────────────────────────────┤
│ SIDEBAR  │  CONTENT HEADER (52 px, sticky)      │
│ 248 px   ├──────────────────────────────────────┤
│ (76 px   │  MAIN CONTENT (scroll interno)       │
│ colapsado)│                                      │
├──────────┴──────────────────────────────────────┤
│  STATUS STRIP (32 px) — estado de guardado,     │
│  conexión, versión                              │
└─────────────────────────────────────────────────┘
```

La Status Strip no es decorativa: muestra feedback de autosave (`Guardado` / `Guardando…` / `Sin conexión`), clave para la eficiencia de captura.

## Layout mobile (< 768 px)

- Sidebar se sustituye por **bottom tab bar** (máx. 5 accesos: Dashboard, Clientes, Suscripciones, Ledger, Más).
- Topbar se reduce a 52 px con patrón back-button en pantallas de detalle.
- Formularios a pantalla completa, una columna, con **barra de acción inferior fija** (`Guardar` siempre alcanzable con el pulgar).
- Objetivo táctil mínimo 44×44 px.

---

# REGLAS DE TECLADO (obligatorias, verificables en Gate transversal)

| Atajo | Acción | Contexto |
|-------|--------|----------|
| `Ctrl/Cmd + K` | Abre la paleta de comandos (navegación + acciones: "Nuevo cliente", "Registrar ingreso"...) | Global |
| `N` | Nuevo registro | Listado con foco, ningún input activo |
| `/` | Enfoca el buscador/filtro | Listado |
| `↑` / `↓` | Mueve selección de fila | Tabla/listado |
| `Enter` | Abre/edita la fila seleccionada · envía el formulario enfocado | Tabla / formulario |
| `Ctrl/Cmd + Enter` | Envía formulario desde un `textarea` multilínea | Formulario |
| `Esc` | Cancela y cierra modal/drawer sin guardar (confirma si hay cambios sin guardar) | Modal / drawer |
| `Delete` | Elimina el registro seleccionado (con confirmación) | Tabla, si el usuario tiene permiso |
| `Tab` / `Shift+Tab` | Orden lógico de foco, primer campo autofocado al abrir formulario/modal | Global |
| `?` | Muestra hoja de atajos disponibles | Global |

Reglas adicionales:
- El foco queda atrapado (focus trap) dentro de modales y drawers.
- Todo botón de solo-ícono tiene `aria-label` y anillo de foco visible.
- Ninguna acción crítica depende exclusivamente del mouse (drag-and-drop siempre tiene alternativa por teclado/menú).

---

# REGLAS DE EFICIENCIA DE CAPTURA (desktop + mobile)

1. **Alta rápida inline**: en listados de alto volumen (Clientes, Suscripciones, Ledger) ofrecer un drawer lateral o fila inline de alta, sin navegar a una página nueva.
2. **Optimistic UI**: las mutaciones (TanStack Query) reflejan el cambio de inmediato en la UI; si la API falla, se revierte y se notifica con un toast.
3. **Autosave de borradores** en formularios largos (notas, tickets) para no perder texto al cambiar de app en el teléfono.
4. **Inputs numéricos** usan `inputMode="decimal"`/`"numeric"` y teclado nativo apropiado en mobile; fechas usan selector nativo con entrada manual de respaldo.
5. **Defaults inteligentes**: fecha de hoy, moneda por defecto MXN, último banco/cuenta usado, para minimizar campos a llenar.
6. **Validación en línea no bloqueante**: errores se muestran por campo sin impedir seguir llenando el formulario; el bloqueo solo ocurre al intentar enviar.
7. **Confirmaciones mínimas**: solo se confirma con modal en acciones destructivas o irreversibles (eliminar, cancelar suscripción, desconciliar); guardar nunca pide confirmación adicional.

---

# AJUSTES NECESARIOS EN EL BACKEND (`AegiFinance.Web`)

El proyecto actual mezcla hosting de Blazor Server (`AddRazorComponents().AddInteractiveServerComponents()` + `MapRazorComponents<App>()`) con controladores de API (`AddControllers()` + `MapControllers()`) en el mismo `Program.cs`. Para servir a una SPA externa:

1. Quitar el hosting de Razor Components/Blazor Server de `Program.cs` (`AddRazorComponents`, `AddInteractiveServerComponents`, `MapRazorComponents<App>`).
2. Eliminar `src/AegiFinance.Web/Components/*` (vistas Razor, layouts, `Login.razor`, etc.) una vez su equivalente en Next.js esté migrado — ver "Plan de Migración".
3. Eliminar `BlazorPermissionService` (acoplado a `AuthenticationStateProvider` de Blazor); conservar `PermissionService` como evaluador canónico de permisos del lado API.
4. Agregar `AddCors()` con política nombrada para el origen de Next.js (`http://localhost:3000` en desarrollo; dominio de producción detrás de Cloudflare Tunnel), permitiendo credenciales si el refresh token se mueve a cookie.
5. Endurecer el flujo de autenticación para SPA:
   - El access token JWT se devuelve en el body de `POST /api/auth/login` / `/api/auth/login-pin` y se guarda **solo en memoria** en el frontend (nunca en `localStorage`).
   - El refresh token se entrega como cookie `HttpOnly`, `Secure`, `SameSite=Lax`, no en el body de la respuesta.
   - `POST /api/auth/refresh` lee la cookie en vez de recibir el token por parámetro.
   - `POST /api/auth/logout` limpia la cookie y revoca el refresh token en base de datos.
6. Agregar `GET /api/auth/me`: devuelve usuario actual, `UserType`, `ClientId`, roles y permisos efectivos. Es la fuente de `usePermissions()` y del menú dinámico en el frontend.
7. (Recomendado, no urgente) Renombrar `AegiFinance.Web` → `AegiFinance.Api` para reflejar que ya es API-only. Se documenta como tarea pendiente; no se ejecuta en este pase de planeación.

Ninguno de estos cambios toca `Domain`, `Application` ni `Infrastructure`.

---

# PLAN DE MIGRACIÓN (orden recomendado)

## Sprint F0 — Cimientos del frontend
- Crear proyecto `web/` (Next.js + TypeScript + Tailwind).
- Implementar tokens AegisUI (`styles/tokens.css`) y primitivas base en `components/ui/` (Button, Input, Select, Dialog, Drawer, Table, Command, KpiCard, Toast).
- Implementar capa de autenticación (`lib/auth/`), `usePermissions()`, `<Can />`, layout `Topbar + Sidebar + StatusStrip` con versión mobile (bottom tabs).
- Implementar cliente HTTP tipado contra el Swagger existente.
- Ajustar backend según la sección anterior (CORS, cookie de refresh, `GET /api/auth/me`).

## Sprints F1-F8 — Paridad del MVP (Fases 1-8)
Migrar módulo por módulo, en el mismo orden de dependencia que las fases del master blueprint, retirando la página Razor equivalente al terminar cada módulo:

| Sprint | Módulo | Razor a retirar |
|--------|--------|------------------|
| F1 | Login + gestión de usuarios/roles/permisos | `Login.razor` |
| F2 | Clientes | `Clients.razor` |
| F3 | Catálogo de servicios | `Services.razor` |
| F4 | Suscripciones | `Subscriptions.razor` |
| F5 | Facturación | `Billing.razor` |
| F6 | Ledger / cuentas bancarias | `Ledger.razor` |
| F7 | Asignaciones de pago | `Allocations.razor` |
| F8 | Estado de cuenta | `AccountStatement.razor` |

## Sprints F9-F15 — Post-MVP
Migrar/completar Conciliación, Dashboard, Reporting, Tickets, Licencias, Portal Cliente y Automatizaciones siguiendo el mismo patrón. `MonthlyTracker.razor` y `BankReconciliation.razor` se evalúan junto con sus fases correspondientes (ver nota de inconsistencia en `progress.md`).

## Criterio de cierre de la migración
La migración se considera terminada cuando `src/AegiFinance.Web/Components/` queda vacío y `Program.cs` ya no referencia Razor Components.

---

# RESULTADO

AegiFinance mantiene exactamente las mismas reglas de negocio, Clean Architecture y arquitectura de permisos (SVA, UserType, Role, Permission, SubscriptionPermission) ya definidas. Lo único que cambia es el canal de presentación: de Blazor Server + MudBlazor + MeridianUI a **React + TypeScript + Next.js + AegisUI**, con foco explícito en eficiencia de captura por teclado y uso cómodo en escritorio y teléfono.
