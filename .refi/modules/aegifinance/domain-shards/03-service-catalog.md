# Fase 3 — Service Catalog

## Objetivo
Administrar los servicios que la empresa vende. Cada servicio define cómo se factura y puede tener un historial de precios. Es el catálogo sobre el cual se crean las suscripciones.

## Entidades

### ServiceCategory
- `Id`
- `Name` (único, ej. Desarrollo, Hosting, Soporte)
- `Description`

### BillingType
Enum / lookup:
- `Monthly`
- `Yearly`
- `OneTime`
- `Hourly`
- `Custom`

### Service
- `Id`
- `Code` (único, ej. SRV-00001)
- `Name`
- `Description`
- `CategoryId`
- `BillingType`
- `DefaultPrice` (decimal, puede ser 0)
- `Currency` (default "MXN")
- `IsActive`
- `IsPublic` (visible en catálogo SaaS)

### ServicePriceHistory
- `Id`
- `ServiceId`
- `Price` (decimal)
- `Currency`
- `EffectiveDate` (DateTime)
- `CreatedBy`
- `CreatedAt`

## Casos de Uso

1. **CRUD de categorías de servicio**.
2. **CRUD de servicios** con selección de tipo de facturación y categoría.
3. **Registrar cambio de precio** en `ServicePriceHistory`.
4. **Consultar historial de precios** de un servicio.
5. **Activar/desactivar** servicio.
6. **Filtrar servicios** por categoría y tipo de facturación.

## Entregables UI/API

### API
- `GET/POST/PUT/DELETE /api/service-categories`
- `GET/POST/PUT/DELETE /api/services`
- `GET /api/services/{id}/price-history`
- `POST /api/services/{id}/price-history`

### UI
- Pantalla de listado de servicios con filtros.
- Formulario de creación/edición de servicio.
- Pantalla de categorías.
- Modal o pestaña de historial de precios.
- Indicador visual del tipo de facturación.

## Reglas de Negocio

1. El código de servicio es único y se genera automáticamente.
2. No se puede eliminar una categoría si tiene servicios asociados.
3. No se puede eliminar un servicio si tiene suscripciones asociadas (solo desactivar).
4. Cada cambio de precio debe registrarse en `ServicePriceHistory` con fecha efectiva.
5. El precio vigente es el más reciente por fecha efectiva.
6. Un servicio de tipo `OneTime` no genera cargos recurrentes.

## Dependencias
- Fase 1: Foundation.

## Criterios de Aceptación

- [ ] Se crea un servicio con código único.
- [ ] Se asigna categoría y tipo de facturación.
- [ ] Al cambiar el precio se registra en historial.
- [ ] No se permite eliminar servicio con suscripciones (validar en backend).
- [ ] El listado filtra por categoría y tipo de facturación.
- [ ] Los cambios quedan en auditoría.

## Notas Técnicas

- `BillingType` puede ser un enum en dominio y un lookup en base de datos.
- El precio vigente debe calcularse con `EffectiveDate <= hoy` ordenado descendente.
- Preparar query optimizada para obtener precio vigente junto al servicio.
- Los servicios `OneTime` no requieren generación periódica de cargos.
- El precio se almacena siempre en MXN; la visualización en otra moneda usa `ICurrencyConverter`.
