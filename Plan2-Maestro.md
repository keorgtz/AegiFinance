# AegiFinance — Plan Maestro V2

## Estado y propósito

Este documento reemplaza a `Plan1-Maestro.md`, `Plan1.1-Extension.md` y `Plan1.2-Extension.md` como fuente principal de ejecución. Los planes anteriores quedan como referencia histórica.

AegiFinance será una plataforma confiable para administrar clientes, planes, cuentas, cargos, pagos y conciliación bancaria desde teléfono o computadora. La fuente de verdad financiera será un Major Ledger auditable; toda cifra visible deberá poder explicarse hasta su movimiento de origen.

## Diagnóstico de la base actual

La solución ya contiene Clean Architecture, autenticación, roles, permisos, clientes, servicios, suscripciones, facturación, movimientos, asignaciones y estados de cuenta. Sin embargo:

- el dashboard era un placeholder;
- la interfaz dependía de colores literales y no tenía tema oscuro real;
- varios clientes HTTP construían rutas `/api/api/...`;
- los permisos actuales son demasiado amplios y algunas claves usadas por la UI no se sembraban;
- varias operaciones de escritura comparten permisos de lectura;
- `LedgerEntry` representa un registro de movimientos por cuenta bancaria, no un libro mayor de partida doble;
- falta la importación de estados bancarios y el flujo completo de conciliación.

## Principios no negociables

1. **Ledger first:** ningún saldo operativo se modifica directamente.
2. **Partida doble:** todo asiento contable debe cuadrar; débitos y créditos deben sumar lo mismo.
3. **Inmutabilidad financiera:** un movimiento contabilizado se revierte con otro movimiento, no se edita ni elimina.
4. **Idempotencia:** importar, facturar o aplicar un pago dos veces no puede duplicar efectos.
5. **Autorización en profundidad:** UI, API, alcance de cliente y base de datos se validan por separado.
6. **Single View Architecture:** una vista por módulo, adaptada por permisos y alcance.
7. **AegiPulse:** claro/oscuro, mobile recompuesto, estados completos, teclado y WCAG 2.2 AA.
8. **Auditoría explicable:** cada decisión automática conserva regla, actor, fecha, fuente y evidencia.
9. **Una fase a la vez:** ninguna fase siguiente comienza hasta revisar demostración, pruebas y deuda de la fase actual.

## Modelo de permisos objetivo

Se separan dos capas:

- **Permiso de negocio:** protege acciones reales de API, por ejemplo `clients.create`, `payments.allocate` o `ledger.post`.
- **Política de control UI:** identifica una ruta, botón, acción o campo mediante una clave estable, por ejemplo `clients.form.tax-id`.

Una política de UI admite `hidden`, `read-only`, `disabled` y `enabled`. Su acceso efectivo se calcula en este orden:

`Sistema → Rol → Excepción de usuario → Cliente → Suscripción → Contexto del registro`

Las claves se descubren desde policies de endpoints y desde un manifiesto generado al compilar el frontend. El catálogo se sincroniza automáticamente; crear una pantalla no exige insertar permisos manualmente en la base de datos. Ninguna política de UI sustituye la autorización de la API.

Para aislamiento multiempresa se usarán filtros globales de EF Core y SQL Server Row-Level Security como defensa adicional donde exista `ClientId`. RLS no se usará para decidir si un botón está visible.

---

## Fase 0 — Base visual y estabilización inicial

**Estado:** iniciada en este rediseño.

### Entregables

- tokens AegiPulse semánticos para claro y oscuro;
- shell responsive: sidebar desktop, navegación móvil y paleta de comandos;
- login, dashboard inicial y componentes base renovados;
- estados de carga, vacío, error y sin acceso;
- corrección de rutas HTTP duplicadas;
- descubrimiento automático de policies declaradas por endpoints;
- reglas obligatorias en `AGENTS.md` y `CLAUDE.md`.

### Gate de salida

- `npm run type-check` y `npm run build`;
- `dotnet build AegiFinance.sln`;
- revisión visual a 390, 768 y 1440 px en ambos temas;
- cero colores literales en componentes de producto, salvo colores de datos externos;
- inventario de controles sin clave y deuda registrada para Fase 1.

---

## Fase 1 — Permission Engine 2.0 y aislamiento

### Objetivo

Construir primero la seguridad dinámica para que todo módulo posterior nazca correctamente protegido.

### Entregables

- entidades `PermissionDefinition`, `RolePermission`, `UserPermissionOverride` y `UiControlPolicy`;
- acciones `view/create/update/delete/approve/reconcile/export` separadas;
- modos UI `hidden/read-only/disabled/enabled`;
- manifiesto TypeScript generado y sincronización automática con backend;
- primitivas `PermissionBoundary`, `PermissionButton`, `PermissionField` y navegación autorizada;
- migración de todos los controles existentes a claves estables;
- filtros por `ClientId` y `SubscriptionId`;
- RLS para tablas multiempresa;
- pantalla de simulación: “ver como este rol/usuario” sin suplantar sesión;
- auditoría de cambios de permisos.

### Gate de salida

- ningún endpoint de escritura protegido sólo por un permiso `View*`;
- ningún control interactivo sin clave;
- pruebas que demuestren denegación por rol, usuario, cliente y suscripción;
- acceso directo por URL no evita autorización;
- un usuario cliente jamás puede leer datos de otro cliente.

---

## Fase 2 — Dashboard operativo V1

**Estado:** implementada el 18 de agosto de 2026. TypeScript, build de producción, compilación .NET Release, catálogo de permisos, pruebas de resolución y migración SQL aprobados. La inspección visual interactiva queda pendiente por indisponibilidad del navegador de validación en esta sesión.

### Objetivo

Convertir el inicio en una cola de decisiones, no en una pared de métricas.

### Entregables

- resumen por permisos: clientes activos, cargos pendientes, vencidos y movimientos;
- cola “requiere atención” para pagos sin aplicar, diferencias y fallos de importación;
- filtros de periodo, cuenta, cliente y moneda;
- acciones contextuales hacia conciliación, cargos o estado de cuenta;
- resumen textual para cada gráfica;
- dashboard adaptado para administrador, conciliador y cliente sin duplicar la vista.

### Gate de salida

- cada cifra navega a su detalle filtrado;
- ninguna cifra se calcula de forma diferente entre dashboard y reporte;
- carga parcial no bloquea todo el dashboard;
- mobile permite completar la siguiente acción con una mano.

---

## Fase 3 — Roles, usuarios y administración de acceso

**Estado:** implementada el 18 de agosto de 2026. Incluye sesiones por dispositivo, revocación inmediata, bloqueo y recuperación, excepciones con expiración y alcance, comparación de roles, historial y contrato Docker configurable. Pendientes de la sesión: validación visual interactiva y `docker compose config` real por ausencia de ambos runtimes.

### Entregables

- CRUD de roles internos y de cliente;
- usuarios, subusuarios, activación, bloqueo y recuperación;
- asignación masiva de permisos por módulo y acción;
- excepciones por usuario con fecha de expiración;
- alcance por cliente y suscripción;
- sesiones activas, cierre remoto y políticas de contraseña/PIN;
- historial de cambios y comparación entre roles.

### Gate de salida

- no existen roles codificados en componentes o controladores;
- cambios de acceso se reflejan sin volver a desplegar;
- revocar acceso invalida sesiones dentro del tiempo acordado;
- permisos efectivos son explicables desde la UI.

---

## Fase 4 — Núcleo contable: Major Ledger

**Estado:** implementada el 18 de agosto de 2026. Incluye catálogo multimoneda, asientos y líneas de partida doble, periodos, contabilización automática de cargos y movimientos bancarios, inmutabilidad, reversión, idempotencia, migración reconciliable, balanza y auxiliares. Compilación .NET Release, pruebas contables, TypeScript, build frontend y catálogo de permisos aprobados. Pendientes de la sesión: inspección visual interactiva y `docker compose config` real por indisponibilidad de ambos runtimes.

### Objetivo

Evolucionar el registro actual de caja hacia un libro mayor real sin perder trazabilidad.

### Entregables

- catálogo de cuentas;
- `JournalEntry` y `JournalLine` con débito/crédito;
- periodos contables y estados borrador/contabilizado/revertido;
- cuentas de banco, clientes, ingresos, gastos, impuestos y diferencias;
- reglas de contabilización para cargos, pagos, transferencias, ajustes y reversiones;
- migración controlada desde `LedgerEntry`;
- balances derivados, balanza de comprobación y auxiliares.

### Gate de salida

- todo asiento contabilizado cuadra;
- movimientos contabilizados son inmutables;
- reversión deja rastro completo;
- saldos históricos se reproducen para cualquier fecha;
- migración reconciliada contra la suma del modelo anterior.

---

## Fase 5 — Cuentas bancarias y saldos

**Estado:** implementada el 18 de agosto de 2026. Incluye apertura transaccional contra el Major Ledger, saldos contable/bancario/diferencia separados, registro auditado de saldos bancarios verificados, transferencias neutrales vinculadas, bloqueo de cuentas inactivas, moneda inmutable, protección del número de cuenta y experiencia AegiPulse responsive con permisos declarativos. Gates aprobados: compilación .NET Release, reglas contables/bancarias, TypeScript, build frontend y catálogo UI. `docker compose config` e inspección visual autenticada quedan sujetos a disponer de Docker y SQL Server en el entorno de ejecución.

### Entregables

- múltiples cuentas por organización, banco y moneda;
- cuentas activas/inactivas, saldo inicial como asiento y fecha de apertura;
- transferencias entre cuentas con dos lados vinculados;
- protección de números de cuenta y últimos dígitos visibles;
- saldo contable, saldo bancario y diferencia sin mezclar conceptos.

### Gate de salida

- una transferencia nunca crea ingreso o gasto artificial;
- saldos coinciden con el Major Ledger;
- cuentas inactivas conservan historia pero no aceptan nuevos movimientos.

---

## Fase 6 — Clientes, contactos y alcance

**Estado:** implementada el 18 de agosto de 2026. Incluye organización y alcance multiempresa en EF y SQL RLS, ficha comercial completa, responsable, reglas configurables de duplicados, documentos persistentes, timeline de auditoría y experiencia AegiPulse responsive con permisos declarativos. Gates aprobados: compilación .NET Release, pruebas de identidad/contabilidad/permisos, migración sin cambios pendientes, script SQL idempotente, TypeScript, build frontend y catálogo de 341 controles. El contrato de despliegue Ubuntu fue endurecido contra secretos ausentes, credenciales iniciales inseguras, exposición de servicios internos, pérdida de claves/documentos, edición SQL no licenciada y restauraciones Docker incompletas; cuenta con guía y verificador propios. La inspección visual autenticada y `docker compose config` quedan pendientes porque el SQL Server local rechazó la conexión cifrada, el navegador integrado no pudo inicializarse y Docker no está instalado en este entorno.

### Entregables

- ficha única de cliente, contactos, etiquetas, notas y documentos;
- condiciones comerciales y moneda de presentación;
- límites de crédito, estado y responsable;
- timeline auditable;
- portal integrado con aislamiento por cliente.

### Gate de salida

- alta y edición completas por teclado y teléfono;
- duplicados detectados por reglas configurables;
- toda consulta respeta alcance multiempresa.

---

## Fase 7 — Planes, servicios y suscripciones

**Estado:** implementada el 18 de agosto de 2026. Los servicios quedaron convertidos en planes multiempresa versionados, con conceptos, vigencias, periodicidad mensual, anual, única, por hora y personalizada, precios base, descuentos, impuestos, prorrateo y condiciones contractuales. Las suscripciones conservan versiones propias de sus condiciones, permiten pausa, reactivación, renovación, cancelación, cambios de precio y cambios de plan con fecha efectiva. Cada cargo congela la versión aplicada y su desglose financiero; las renovaciones usan clave de idempotencia protegida por índice único. La administración de visibilidad usa la política dinámica `ManageSubscriptionAccess`, sin roles embebidos. La UI AegiPulse expone versiones y condiciones en layouts recompuestos para teléfono y escritorio, con temas semánticos y controles declarativos. Gates aprobados: compilación .NET Release, pruebas financieras y de permisos, migración sin cambios pendientes, TypeScript, build frontend, catálogo de 372 controles y verificador de despliegue. No fue necesario agregar servicios ni volúmenes a Docker Compose; `docker compose config` queda pendiente en un host con Docker instalado.

### Entregables

- planes versionados con conceptos y precios;
- periodicidad mensual, anual, única y personalizada;
- prorrateo, descuentos, impuestos y vigencias;
- suscripciones con pausa, renovación, cambio de plan y cancelación;
- historial de precio y condiciones sin reescribir el pasado.

### Gate de salida

- una renovación repetida es idempotente;
- cambios futuros no alteran cargos ya emitidos;
- permisos por suscripción funcionan de extremo a extremo.

---

## Fase 8 — Cargos y cuentas por cobrar

**Estado:** implementada el 27 de agosto de 2026. Gate automatizado: `scripts/verify-phase8.ps1`.

### Entregables

- generación programada y manual;
- cargos, notas de crédito, recargos y cancelaciones por reversión;
- vencimientos, antigüedad de saldo y promesas de pago;
- cierre y reproceso seguros;
- asiento contable automático por evento.

### Gate de salida

- no se generan cargos duplicados;
- total por cobrar coincide con auxiliares del ledger;
- cada cargo explica su plan, periodo y regla de cálculo.

---

## Fase 9 — Importación de estados bancarios

**Estado:** implementada el 27 de agosto de 2026. Gate automatizado: `scripts/verify-phase9.ps1`.

### Entregables

- importadores CSV/XLSX y adaptadores por banco;
- perfil de columnas, vista previa y validación antes de confirmar;
- hash de deduplicación, lote de importación y rollback controlado;
- fechas, referencias, descripción, importe, moneda y saldo bancario;
- estados válido, duplicado, incompleto y rechazado.

### Gate de salida

- reimportar el mismo archivo no duplica líneas;
- errores señalan fila, campo y corrección;
- ninguna línea inválida llega a conciliación silenciosamente.

---

## Fase 10 — Motor de conciliación

**Estado:** implementada el 27 de agosto de 2026. Incluye coincidencias explicables por importe, fecha, referencia, cliente y patrón; casos exactos, sugeridos, combinados y parciales; confirmación humana; clasificación de diferencias; reversión auditable; cierres de periodo protegidos; concurrencia optimista; aislamiento multiempresa en EF y SQL RLS; permisos declarativos y experiencia AegiPulse responsive. Gate automatizado: `scripts/verify-phase10.ps1`.

### Entregables

- coincidencia exacta y sugerida por importe, fecha, referencia, cliente y patrón;
- puntuación explicable y umbral configurable;
- uno a uno, uno a muchos, muchos a uno y pagos parciales;
- confirmación humana para sugerencias;
- diferencias, comisiones, devoluciones y transferencias;
- deshacer conciliación mediante eventos auditables.

### Gate de salida

- el motor nunca auto-confirma por debajo del umbral;
- cada sugerencia explica por qué coincide;
- no puede conciliarse dos veces la misma línea o movimiento;
- diferencia final de un periodo cerrado es cero o queda justificada.

---

## Fase 11 — Aplicación de pagos

### Entregables

- aplicación manual y automática a cargos;
- prioridades configurables: vencimiento, plan, referencia o selección;
- pagos parciales, anticipos, sobrantes y pagos combinados;
- reversión y reaplicación;
- recibo y asiento asociado.

### Gate de salida

- suma aplicada nunca excede el pago ni el saldo del cargo;
- una reversión restaura exactamente saldos anteriores;
- toda aplicación conserva actor, regla y origen.

---

## Fase 12 — Estados de cuenta y portal cliente

### Entregables

- saldo inicial, cargos, pagos, ajustes, saldo final y vencidos;
- filtros por periodo, plan y moneda;
- PDF/CSV accesible y verificable;
- portal cliente dentro de la misma aplicación;
- descarga, contacto y aclaración desde el movimiento.

### Gate de salida

- saldo inicial + movimientos = saldo final;
- el PDF coincide con la vista y el ledger;
- cliente sólo ve su información y suscripciones autorizadas.

---

## Fase 13 — Auditoría, periodos y cierre

### Entregables

- bitácora consultable de acceso y cambios;
- cierre por periodo con checklist;
- reapertura con aprobación y motivo;
- evidencia de importaciones, reglas automáticas y reversiones;
- alertas de integridad.

### Gate de salida

- un periodo cerrado no acepta asientos ordinarios;
- toda reapertura queda aprobada y auditada;
- pruebas de integridad detectan asientos descuadrados o huérfanos.

---

## Fase 14 — Reportes y analítica

### Entregables

- cartera, cobranza, ingresos, gastos, antigüedad, conciliación y flujo;
- balanza y auxiliares contables;
- filtros compartidos y exportación;
- definiciones visibles de cada métrica;
- reportes programados con control de acceso.

### Gate de salida

- reportes se reconcilian con el ledger;
- exportar respeta los mismos permisos y filtros que la pantalla;
- gráficas incluyen periodo, unidad y resumen textual.

---

## Fase 15 — Automatización, confiabilidad y operación

### Entregables

- recordatorios, renovaciones, vencimientos y reglas de conciliación;
- outbox para eventos, reintentos y trabajos idempotentes;
- backups verificados y restauración ensayada;
- observabilidad, alertas, métricas y trazas;
- pruebas E2E de login, cargo, importación, conciliación, aplicación y estado de cuenta;
- PWA/offline limitado a lectura segura y borradores explícitos.

### Gate de salida

- RPO/RTO definidos y restauración comprobada;
- fallos externos no duplican operaciones;
- flujos críticos pasan en desktop y mobile;
- checklist de producción, seguridad y soporte aprobado.

## Orden de revisión

La siguiente sesión debe comenzar por **Fase 1 — Permission Engine 2.0**. Después se revisarán Fase 2 (Dashboard) y Fase 3 (Roles y usuarios). Cada fase termina con demostración funcional, evidencia de pruebas y actualización de este documento.
