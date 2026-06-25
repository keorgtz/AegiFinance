# AegiFinance — Reglas del Proyecto

Plataforma financiera (control de clientes, suscripciones, facturación y ledger) de Keorsoft. Operación interna + portal cliente integrado.

## Planeación

La fuente de verdad de planeación vive en, por orden de prioridad ante conflicto:

1. `Plan1.2-Extension.md` — stack de frontend (React/TS/Next.js), AegisUI, reglas de teclado y eficiencia de captura.
2. `Plan1.1-Extension.md` — modelo de usuarios, permisos, Portal Cliente y SVA.
3. `Plan1-Maestro.md` — visión, principios y fases (referencia histórica de visión y fases).
4. `.refi/modules/aegifinance/` — desglose operativo: `master-blueprint.md`, `progress.md`, `orchestration-map.md`, `verification.md`, `domain-shards/*.md`.

No dupliques estas reglas en código vía comentarios; consulta los documentos cuando falte contexto.

## Stack

- **Backend:** .NET 9, ASP.NET Core Web API, EF Core, MediatR, FluentValidation, Serilog. SQL Server 2022.
- **Frontend:** React + TypeScript + Next.js (App Router), Radix UI + Tailwind CSS, TanStack Query/Table, React Hook Form + Zod, cmdk, lucide-react.
- **Infra:** Docker Compose, Ubuntu Server, Cloudflare Tunnel.

`AegiFinance.Web` es API-only (controladores HTTP); no hospeda Razor Components/Blazor Server. El frontend vive en `web/`, proyecto hermano de `src/`, y consume la API solo vía HTTP/JSON.

## Clean Architecture (sin excepciones)

```
Domain        → no depende de nada
Application   → depende solo de Domain
Infrastructure→ depende de Application + Domain
Web / Worker  → dependen de Application + Infrastructure
web/ (Next.js)→ consume Web vía HTTP; nunca referencia proyectos .NET
```

No se coloca lógica de negocio en controllers ni en componentes de React. Las validaciones de negocio van en `Application` (FluentValidation); el frontend valida solo para UX (Zod), nunca como única fuente de autorización.

## Single View Architecture (SVA)

Una sola ruta de Next.js por módulo, compartida entre administradores y clientes. Prohibido crear variantes por rol (`dashboard/admin/page.tsx`, `DashboardClient.tsx`, etc.). El contenido se adapta con `usePermissions()` / `<Can permission="..." />`, nunca duplicando vistas.

Jerarquía de autorización: `Permission → Role → User → Client → Subscription`. `UserType` (Administrator/Client) es independiente de `Role`. Toda autorización real ocurre en el backend; el frontend solo refleja lo ya autorizado vía `GET /api/auth/me`.

## Dinero y auditoría

- Todo monto se almacena en MXN. Conversión a otra moneda es solo visual, vía `ICurrencyConverter`, con la tasa usada siempre registrada.
- Ledger First: ningún saldo se almacena calculado; todo se deriva del ledger.
- Toda acción crítica queda en `AuditLog`.

## AegisUI (sistema de diseño)

Sistema de diseño propio de AegiFinance. Inspirado en la filosofía de MeridianUI (densidad controlada, color semántico, elevación sutil, interacción silenciosa) pero con valores propios e independientes: paleta de 5 roles (Jade/Saffron/Periwinkle/Plum/Terracotta + primario petróleo), tipografía Manrope + Sora, iconografía lucide-react, radios y layout propios. **Nunca reutilizar hex, fuentes o iconos de MeridianUI.** Tokens completos en `Plan1.2-Extension.md`.

Regla añadida sobre MeridianUI: **teclado primero** — todo flujo de alta/edición debe completarse sin tocar el mouse.

## Teclado y eficiencia de captura (obligatorio en todo formulario/listado nuevo)

- `Ctrl/Cmd+K` paleta de comandos global; `N` nuevo registro; `/` enfoca buscador; `↑`/`↓` navegan filas; `Enter` abre/edita o envía formulario; `Esc` cancela con confirmación si hay cambios sin guardar.
- Foco atrapado en modales/drawers; primer campo autofocado al abrir.
- Mobile: una columna, barra de acción inferior fija, `inputMode` correcto en numéricos, objetivos táctiles ≥ 44×44px.
- Alta rápida inline en listados de alto volumen (Clientes, Suscripciones, Ledger); UI optimista con TanStack Query y rollback en error.

Ver gates verificables en `.refi/modules/aegifinance/verification.md` (secciones "Teclado" y "Eficiencia de Captura").

## Auth

Access token JWT en memoria en el cliente (nunca `localStorage`). Refresh token en cookie `HttpOnly`/`Secure`/`SameSite=Lax`. `GET /api/auth/me` es la fuente de permisos efectivos para el frontend.

## Git

- No crear ni usar otras branches. Todo se unifica en `Master`; operar exclusivamente sobre `Master`.
