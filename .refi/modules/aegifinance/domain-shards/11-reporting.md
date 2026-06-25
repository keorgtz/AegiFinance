# Fase 11 — Reporting

## Objetivo
Proveer reportes empresariales listos para impresión y análisis. Por ahora los reportes serán **estáticos en PDF generados automáticamente con estilo AegisUI**. La integración con AegiReports queda reservada para el futuro, pero el contrato de datos se define desde esta fase.

## Reportes Requeridos

1. **Clientes**: listado de clientes con suscripción, categoría y saldo.
2. **Servicios**: catálogo de servicios con precio vigente y estado.
3. **Suscripciones**: suscripciones activas, suspendidas, canceladas.
4. **Ingresos**: ingresos por período, cliente y cuenta bancaria.
5. **Gastos**: egresos por período, cuenta y categoría.
6. **Morosidad**: clientes con saldo pendiente, días de mora y monto.
7. **Conciliación**: diferencias entre ledger y estados bancarios.

## Tecnología

- **Motor principal de PDF:** QuestPDF (generación nativa de PDF en .NET 9, API fluent, alto rendimiento).
- **Excel:** ClosedXML para exportación a `.xlsx`.
- **Vista previa / impresión:** HTML generado dinámicamente con estilo AegisUI.
- **Estilo visual:** AegisUI (tipografía Manrope/Sora, colores semánticos propios — Jade/Saffron/Periwinkle/Plum/Terracotta —, iconografía lucide, layout de tarjetas y tablas).
- **Extensibilidad:** el motor de reportes se diseñará para permitir agregar nuevos formatos (Word, CSV, etc.) sin reescribir la fuente de datos.
- **Futuro:** AegiReports se integrará cuando esté listo; por ahora se mantiene el mismo contrato de datos.

## Entidades

No hay entidades nuevas. Los reportes consumen datos existentes.

### ReportDefinition
- `Id`
- `Name`
- `Slug`
- `Parameters` (JSON)
- `TemplatePath`

> Registro de reportes disponibles y parámetros. No confundir con plantilla AegiReports.

### ReportOutput
- `Id`
- `ReportDefinitionId`
- `GeneratedAt`
- `GeneratedBy`
- `FileUrl`
- `Parameters` (JSON)

> Almacena referencia a archivos generados (PDF, XLSX, HTML) para descarga posterior.

## Casos de Uso

1. Generar reporte de clientes.
2. Generar reporte de servicios.
3. Generar reporte de suscripciones.
4. Generar reporte de ingresos.
5. Generar reporte de gastos.
6. Generar reporte de morosidad.
7. Generar reporte de conciliación.
8. Exportar reporte a PDF.
9. Exportar reporte a Excel (.xlsx).
10. Generar vista previa HTML.
11. Descargar reporte generado.
12. (Futuro) Integrar con AegiReports usando el mismo contrato de datos.

## Entregables UI/API

### API
- `POST /api/reports/clients/generate`
- `POST /api/reports/services/generate`
- `POST /api/reports/subscriptions/generate`
- `POST /api/reports/income/generate`
- `POST /api/reports/expenses/generate`
- `POST /api/reports/delinquency/generate`
- `POST /api/reports/reconciliation/generate`
- `POST /api/reports/{id}/export/pdf`
- `POST /api/reports/{id}/export/xlsx`
- `POST /api/reports/{id}/export/html`
- `GET /api/reports/{id}/download`

### UI
- Centro de reportes.
- Formulario de parámetros por reporte (fechas, cliente, estado, moneda de visualización).
- Botón "Exportar PDF".
- Botón "Exportar Excel".
- Botón "Vista previa HTML".
- Selector de moneda de visualización (conversiones desde MXN).
- Estilo visual AegisUI en encabezados, tablas, KPIs y chips de estado.
- Centro de reportes accesible desde la paleta de comandos (`Ctrl/Cmd+K`).

## Reglas de Negocio

1. Los reportes deben respetar permisos del usuario (`ViewReports`, `ViewRevenue`, etc.).
2. Los reportes financieros deben coincidir con el ledger.
3. Los usuarios `Client` solo ven datos de su cliente en los reportes.
4. Los filtros de fecha son obligatorios para reportes de ingresos/gastos.
5. Los reportes de morosidad calculan días desde la fecha de vencimiento.
6. Los PDFs, Excel y HTML deben usar tipografía Manrope/Sora, colores semánticos AegisUI y tabular nums para importes.
7. La moneda de visualización en reportes se convierte desde MXN usando la tasa automática o manual configurada.
8. No se usan emojis ni imágenes decorativas en reportes.

## Dependencias
- Fase 1: Foundation.
- Fase 2: Client Management.
- Fase 3: Service Catalog.
- Fase 4: Subscriptions.
- Fase 5: Billing Engine.
- Fase 6: Financial Ledger.
- Fase 8: Customer Account Statements.
- Fase 9: Bank Reconciliation.

## Criterios de Aceptación

- [ ] Se generan los 7 reportes requeridos en PDF.
- [ ] Se pueden exportar los reportes a Excel (.xlsx).
- [ ] Se puede generar vista previa HTML.
- [ ] Los datos coinciden con el ledger y suscripciones.
- [ ] Los PDFs/Excel/HTML usan estilo AegisUI (colores, tipografía, tablas).
- [ ] Los filtros aplican correctamente.
- [ ] Un usuario `Client` solo ve su información en reportes.
- [ ] La conversión de moneda funciona desde MXN con tasa automática o manual.
- [ ] El contrato de datos está documentado para futura integración con AegiReports.

## Notas Técnicas

- Usar QuestPDF para PDF; ClosedXML para Excel; generador de HTML del lado del servidor (plantillas .NET, no Razor Components) para la vista previa — independiente del frontend Next.js.
- Definir contrato de datos con AegiReports (JSON o DTOs) desde el inicio.
- Considerar generación asíncrona para reportes pesados.
- Almacenar archivos generados temporalmente si son muy grandes.
- Implementar parámetros tipados para evitar inyección.
- Usar UTC internamente y convertir a zona horaria de México para fechas en reportes.
- Formato de fecha en reportes: `22/Jun/2026`.
- La conversión de moneda debe usar `ICurrencyConverter` centralizado.
