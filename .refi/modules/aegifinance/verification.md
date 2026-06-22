# AegiFinance — Verification & Quality Gates

## Principios de Verificación

1. Cada fase debe superar su gate antes de avanzar a la siguiente.
2. Los gates incluyen pruebas funcionales, de seguridad, de arquitectura y de negocio.
3. Ningún saldo se almacena calculado; todos los saldos deben ser verificables mediante el ledger.
4. Toda acción crítica debe dejar traza en auditoría.

## Gates por Fase

### Gate 1 — Foundation

- [ ] Login con contraseña devuelve JWT válido y refresh token.
- [ ] Login con PIN funciona para subusuarios de cliente.
- [ ] Refresh token renovado correctamente.
- [ ] Logout invalida refresh token.
- [ ] Usuario sin permiso recibe 403.
- [ ] `UserType = Client` requiere `ClientId`.
- [ ] `UserType = Administrator` tiene `ClientId = NULL`.
- [ ] Auditoría registra cambios en usuarios, roles y permisos.
- [ ] Migraciones ejecutan sin errores.
- [ ] Cobertura de pruebas unitarias > 70% en handlers de autenticación.
- [ ] `CurrencyConfig` y `ExchangeRate` creados.
- [ ] Se puede registrar tipo de cambio manual.
- [ ] Se puede sincronizar tipo de cambio automático.
- [ ] `ICurrencyConverter` centraliza conversiones MXN ↔ moneda de visualización.

### Gate 2 — Client Management

- [ ] CRUD de clientes funcional.
- [ ] Código de cliente único y auto-generado.
- [ ] Contacto primario único por cliente.
- [ ] Etiquetas y categorías funcionan.
- [ ] Historial de auditoría visible.
- [ ] Permisos `clients:*` aplicados.

### Gate 3 — Service Catalog

- [ ] CRUD de servicios y categorías.
- [ ] Historial de precios registrado.
- [ ] No se elimina servicio con suscripciones.
- [ ] Precio vigente calculado correctamente.

### Gate 4 — Subscriptions

- [ ] Suscripción vincula cliente y servicio.
- [ ] Estados funcionan correctamente.
- [ ] Suspensión detiene generación de cargos.
- [ ] Historial de precios y cambios registrado.
- [ ] Cliente puede tener N suscripciones.
- [ ] `SubscriptionPermission` restringe visibilidad por usuario.

### Gate 5 — Billing Engine

- [ ] Worker genera cargos automáticamente.
- [ ] No hay duplicados.
- [ ] Ciclos de facturación funcionan.
- [ ] Reprocesamiento controlado.
- [ ] Logs de generación registrados.

### Gate 6 — Financial Ledger

- [ ] Saldo calculado correctamente.
- [ ] Transferencias actualizan dos cuentas.
- [ ] Movimientos reconciliados no editables.
- [ ] Ajustes con motivo.
- [ ] Ingresos vinculables a cliente.
- [ ] Los montos se almacenan siempre en MXN.
- [ ] La visualización en otra moneda usa `ICurrencyConverter`.

### Gate 7 — Subscription Allocations

- [ ] Auto-asignación funciona (FIFO).
- [ ] Asignación manual controla límites.
- [ ] Pago parcial deja cargo en `Partial`.
- [ ] Excedente queda sin asignar.
- [ ] No se desasigna movimiento reconciliado.

### Gate 8 — Customer Account Statements

- [ ] Estado de cuenta muestra cargos, pagos y ajustes.
- [ ] Saldo acumulado correcto.
- [ ] Filtros de fecha funcionan.
- [ ] Resumen financiero consistente.
- [ ] La visualización en otra moneda usa `ICurrencyConverter`.
- [ ] Se muestra la tasa de cambio aplicada.

### Gate 9 — Bank Reconciliation

- [ ] Upload de estado de cuenta funciona.
- [ ] Conciliación automática empareja por monto/referencia.
- [ ] Conciliación manual disponible.
- [ ] Reporte de diferencias correcto.

### Gate 10 — Dashboard

- [ ] Existe un único componente `Dashboard.razor`.
- [ ] El contenido se adapta según permisos del usuario.
- [ ] KPIs calculados correctamente.
- [ ] Gráficos muestran tendencias cuando aplica.
- [ ] Datos consistentes con ledger.
- [ ] Usuarios `Client` solo ven datos de su cliente.
- [ ] Los KPIs monetarios se pueden visualizar en moneda extranjera usando `ICurrencyConverter`.
- [ ] Se muestra la tasa de cambio aplicada.

### Gate 11 — Reporting

- [ ] 7 reportes generan correctamente en PDF.
- [ ] Se pueden exportar los reportes a Excel (.xlsx).
- [ ] Se puede generar vista previa HTML.
- [ ] Los PDFs/Excel/HTML usan estilo MeridianUI.
- [ ] Datos consistentes.
- [ ] Usuarios `Client` solo ven su información.
- [ ] La conversión de moneda funciona desde MXN con tasa automática o manual.

### Gate 12 — Support Tickets

- [ ] Tickets vinculados a cliente.
- [ ] Comentarios internos/públicos funcionan.
- [ ] Adjuntos limitados por tamaño/tipo.

### Gate 13 — License Management

- [ ] Claves únicas y seguras.
- [ ] Límite de activaciones respetado.
- [ ] Validación funciona desde API.

### Gate 14 — Customer Portal

- [ ] El portal es parte de la misma aplicación AegiFinance.
- [ ] No existen vistas duplicadas (SVA).
- [ ] Cliente inicia sesión y ve solo sus datos.
- [ ] Puede crear tickets.
- [ ] No accede a datos de otros clientes.
- [ ] El menú se construye dinámicamente por permisos.
- [ ] Los subusuarios con `SubscriptionPermission` solo ven suscripciones asignadas.
- [ ] Los montos se pueden visualizar en moneda extranjera usando `ICurrencyConverter`.
- [ ] Se muestra la tasa de cambio aplicada.

### Gate 15 — Automations

- [ ] Reglas configurables.
- [ ] Worker ejecuta según programación.
- [ ] Logs registrados.
- [ ] Recordatorios idempotentes.

## Gates Transversales

### Seguridad
- [ ] Todos los endpoints protegidos por autenticación salvo los explícitamente públicos.
- [ ] Autorización basada en permisos funciona.
- [ ] Filtrado por `ClientId` aplicado en todas las queries de clientes.
- [ ] Usuarios `Client` no pueden acceder a datos de otros clientes.
- [ ] No hay datos sensibles expuestos en logs o errores.
- [ ] Contraseñas y PINs hasheados con algoritmo seguro.

### Arquitectura
- [ ] Dependencias de Clean Architecture respetadas.
- [ ] Domain no depende de infraestructura.
- [ ] No hay lógica de negocio en controllers.
- [ ] Validaciones en FluentValidation.
- [ ] Single View Architecture (SVA) aplicada: una vista por módulo.
- [ ] Cero duplicación de componentes para administradores y clientes.

### UX/UI
- [ ] Toda la UI sigue MeridianUI (colores, tipografía, iconografía, spacing).
- [ ] El menú de navegación se construye dinámicamente por permisos.
- [ ] Fechas presentadas en formato `22/Jun/2026`.
- [ ] Importes con `font-variant-numeric: tabular-nums`.

### Moneda
- [ ] Todo monto se almacena en MXN.
- [ ] Las conversiones a otras monedas son visuales y usan `ICurrencyConverter`.
- [ ] Se soportan tasas automáticas (servicio externo) y manuales.
- [ ] Se registra la tasa de cambio aplicada en cada conversión.
- [ ] La moneda por defecto del sistema es MXN.

### Calidad
- [ ] Build sin warnings críticos.
- [ ] Pruebas unitarias pasan.
- [ ] Migraciones aplicables desde cero.

## Checklist Final de MVP (Fases 1-8)

- [ ] Gate 1 aprobado.
- [ ] Gate 2 aprobado.
- [ ] Gate 3 aprobado.
- [ ] Gate 4 aprobado.
- [ ] Gate 5 aprobado.
- [ ] Gate 6 aprobado.
- [ ] Gate 7 aprobado.
- [ ] Gate 8 aprobado.
- [ ] Revisión de seguridad aprobada.
- [ ] Revisión de arquitectura aprobada.

## Notas

- Un gate no aprobado bloquea el avance a la siguiente fase.
- Los gates se documentan en el progress.md al completarse.
- Ryou Orchestrator debe ejecutar las pruebas asociadas a cada gate antes de solicitar revisión.
