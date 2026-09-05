# Simplificación de conciliación bancaria

## Implementado

- Dashboard seguido de Conciliación bancaria en la navegación de escritorio y móvil.
- `/ledger` abre captura bancaria; los enlaces existentes con filtros conservan su vista de movimientos.
- Captura por filas en cuenta existente: fecha, concepto, cargo, abono y saldo del estado bancario. Enter avanza y agrega la siguiente fila; se pueden eliminar filas antes de confirmar.
- Revisión y confirmación mediante el importador CSV existente, con escape de comillas y separadores, detección de duplicados y errores del servidor. No se escriben saldos contables directamente.
- Las filas se conservan al cambiar de pestaña; no son persistentes al salir del módulo. El navegador avisa al cerrar o recargar con captura pendiente. Sin conexión se permite capturar, pero no confirmar.
- Importación CSV/XLSX accesible desde captura. Historial y reversión de lotes existentes conservados.
- Selección de cliente al registrar un ingreso. Asignación manual con periodo, suscripción y filtros de mes/año; admite múltiples mensualidades. Reversión de aplicaciones existente conservada.
- Catálogo de controles ampliado para incluir Combobox y campos de captura con claves dinámicas.

## Pendiente para completar el flujo solicitado

La captura bancaria aún no clasifica directamente una línea como pago, gasto o transferencia ni la vincula al cliente. Se conserva el recorrido de registrar el movimiento contable, aplicar mensualidades y conciliar. Implementar esa operación necesita un comando transaccional de backend que evite duplicar movimientos y conserve aislamiento de tenant, idempotencia y asientos balanceados.

La eliminación individual de líneas confirmadas tampoco está implementada. El historial permite revertir un lote no conciliado; la captura permite eliminar filas antes de confirmar. Debe diseñarse una corrección individual auditable que considere conciliaciones y pagos ya aplicados.

Los selectores conservan los límites de paginación existentes. No constituyen una búsqueda exhaustiva de todos los clientes o mensualidades.

## Verificación

- TypeScript, build de producción y catálogo de permisos verificados.
- Playwright: seis escenarios con API simulada, a 390, 768 y 1440 px, en ambos temas. Cubren navegación con Enter, eliminación, conservación entre pestañas, serialización CSV y confirmación. No sustituyen pruebas financieras contra API/SQL Server.
- Validación de backend completada el 2026-09-04 con .NET SDK 9.0.120 y runtime 9.0.19:
  - `dotnet build AegiFinance.sln -c Release --nologo`: pasó, sin advertencias ni errores.
  - `dotnet run --project tests/AegiFinance.LedgerTests -c Release --no-build`: pasó; verifica reglas de libro mayor, cuentas, importaciones, conciliación y aplicación de pagos, entre otras.
  - `dotnet run --project tests/AegiFinance.PermissionTests -c Release --no-build`: pasó; verifica decisiones de permisos, alcance de cliente/suscripción y contratos JSON.
- Ambas suites son ejecutables de consola, no proyectos de un test runner: `dotnet test` por sí solo no ejecuta sus comprobaciones.
- Estas suites comprueban reglas en memoria. No prueban transacciones, aislamiento ni migraciones contra SQL Server; no se ha ejecutado una prueba integral del backend y la base de datos.

## Despliegue

Los cambios reutilizan la imagen frontend, API, variables, volúmenes, redes, health checks y orden de inicio existentes. No requieren migraciones ni ajustes a Compose o Dockerfiles, ni pasos manuales nuevos de despliegue.

Validación completada el 2026-09-04 con Docker Engine (Moby) 29.7.2 y Docker Compose 5.5.0, instalados desde los repositorios de Fedora:

- Servicio Docker activo y habilitado al inicio.
- `docker run --rm hello-world`: pasó; verificó descarga y ejecución de un contenedor.
- `docker compose config --quiet`: pasó con las variables actuales. El contrato Compose permanece válido y no necesita ajustes por estos cambios.

No se desplegó la aplicación ni se modificaron sus bases de datos. La comprobación de Compose valida la configuración, no el arranque completo de los servicios.
