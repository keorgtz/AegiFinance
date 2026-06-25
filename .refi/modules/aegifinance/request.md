# Solicitud del Cliente

## Fuente
Archivo: `Plan1-Maestro.md` ubicado en la raíz del repositorio `AegiFinance`.

## Petición original
> "Vamos a generar mi Proyecto AegiFinance, en mi Plan1-Maestro.md tengo lo inicial para las fases, pero quiero que me generes todos los planes necesarios para todas las fases primero, y ya que terminemos de hacer todos los planes genera un resumen corto HTML de lo generado y lo que haga falta de hacer, asi que comienza a leer mi Plan1-Maestro.md y analizalo completamente y generame todos los planes y fases de mi AegiFinance que sera el nombre de mi software para mi app financiera de control de mis clientes y planes"

## Intención
El cliente desea:
1. Leer y analizar completamente el `Plan1-Maestro.md` existente.
2. Generar planes detallados para **todas** las fases descritas (1-15).
3. Entregar un resumen corto en formato HTML que muestre lo generado y lo que falta por hacer.
4. El producto final es **AegiFinance**, una aplicación financiera para control de clientes y planes/suscripciones.

## Alcance solicitado
- Producción de documentación de planificación (REFI packet) para todo el proyecto.
- No se solicita implementación de código en este paso; se busca planificación profunda primero.
- El resumen HTML debe ser breve y visual.

## Restricciones y directrices
- No inventar requisitos que no estén en el maestro.
- No crear APIs, tablas, servicios o UI especulativos.
- Respetar los principios de diseño del maestro: auditabilidad, *Ledger First*, *Subscription Driven*, separación Cliente/Suscripción y escalabilidad.
- Usar el stack tecnológico declarado: .NET 9, ASP.NET Core Web API, EF Core, MediatR, FluentValidation, Serilog, Blazor Web App, MudBlazor, SQL Server 2022, Docker, Docker Compose, Ubuntu Server, Cloudflare Tunnel y AegiReports.
- La arquitectura base es Clean Architecture con los proyectos: `SHE.Finance.Domain`, `SHE.Finance.Application`, `SHE.Finance.Infrastructure`, `SHE.Finance.Web`, `SHE.Finance.Worker`.

---

# ADDENDUM — Pivote de Stack de Frontend (24/Jun/2026)

## Fuente
Mensaje directo del cliente en sesión de trabajo, posterior a la solicitud original anterior.

## Petición (resumen del mensaje original en español)
El cliente solicitó refactorizar el plan maestro y todo su desglose (`Plan1-Maestro.md`, `Plan1.1-Extension.md` y el contenido completo de `.refi/modules/aegifinance/`) para reflejar un cambio de tecnología de frontend:

1. Mantener el backend en C# / ASP.NET Core (Clean Architecture intacta).
2. Mover la UI de **Blazor Web App + MudBlazor** a **React + TypeScript + Next.js**.
3. La UI/UX debe **inspirarse** en su sistema de diseño existente `.MeridianUI`, pero **no debe ser idéntica**: solo se adopta el estilo/filosofía, adaptado para que AegiFinance no se perciba como una copia visual de otro producto.
4. El sistema debe ser intuitivo, funcionar correctamente con el teclado y ser muy eficiente para dar de alta o editar registros, tanto en computadora como en teléfono.
5. Ajustar las reglas y estructura del proyecto AegiFinance en consecuencia (stack + Clean Architecture + reglas de diseño).
6. Modificar toda la planeación existente para incorporar esto correctamente, dado que parte de ella ya estaba parcialmente implementada.

## Intención
El cliente desea un pivote de presentación, no un cambio de producto: las reglas de negocio, Clean Architecture, Permission Engine y SVA ya definidas se conservan; solo cambia el canal de UI y se introduce un sistema de diseño propio (**AegisUI**) en lugar de MeridianUI.

## Alcance solicitado
- Refactorizar la planeación (`Plan1-Maestro.md`, `Plan1.1-Extension.md`, y todo `.refi/modules/aegifinance/` incluyendo `master-blueprint.md`, `progress.md`, `orchestration-map.md`, `verification.md` y los domain-shards aplicables) para reflejar el nuevo stack de frontend.
- Crear un nuevo documento `Plan1.2-Extension.md` con el detalle completo del pivote: stack, arquitectura de carpetas, tokens de AegisUI, reglas de teclado y de eficiencia de captura en mobile/desktop, y ajustes necesarios en el backend para servir a una SPA.
- No se solicitó en este paso escribir código de la aplicación React; el pivote de código queda documentado como plan de migración a ejecutar después.

## Restricciones y directrices
- No modificar la lógica de negocio, entidades de dominio ni handlers existentes en `AegiFinance.Domain`/`Application`/`Infrastructure`.
- AegisUI debe ser un sistema de diseño con valores propios (paleta, tipografía, iconografía, proporciones) — no debe reutilizar literalmente los tokens de MeridianUI.
- Preservar intacto el registro histórico de la solicitud original (sección anterior de este documento); este addendum se agrega, no reemplaza.
