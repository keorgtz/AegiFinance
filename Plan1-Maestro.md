# PLAN.md

# SHE Finance Platform V2

## Master Implementation Plan

---

# VISIÓN

Construir una plataforma financiera web capaz de operar en dos modos

## Modo Interno

Administración personal de clientes y mensualidades.

Ejemplos

 Clientes de software.
 Clientes de mantenimiento.
 Clientes de hosting.
 Clientes de soporte.

---

## Modo SaaS

Administración comercial completa.

Ejemplos

 Venta de licencias.
 Venta de suscripciones.
 Soporte.
 Portal cliente.
 Automatizaciones.

---

# PRINCIPIOS DE DISEÑO

## Regla #1

Todo movimiento financiero debe ser auditable.

Nunca modificar saldos directamente.

Todo debe generarse mediante movimientos.

---

## Regla #2

Ledger First Architecture

El ledger es la fuente de verdad.

Todo saldo se calcula.

Nada se almacena calculado.

---

## Regla #3

Subscriptions Driven

Todo ingreso recurrente proviene de una suscripción.

---

## Regla #4

Cliente ≠ Suscripción

Un cliente puede tener

 0 suscripciones
 1 suscripción
 N suscripciones

---

## Regla #5

La plataforma debe funcionar con

 5 clientes
 500 clientes
 5000 clientes

Sin cambios arquitectónicos.

---

# STACK TECNOLÓGICO

## Backend

.NET 9

ASP.NET Core Web API

Entity Framework Core

MediatR

FluentValidation

Serilog

---

## Frontend

Blazor Web App

MudBlazor

---

## Base de Datos

SQL Server 2022

---

## Infraestructura

Docker

Docker Compose

Ubuntu Server

Cloudflare Tunnel

---

## Reportes

AegiReports

---

# CLEAN ARCHITECTURE

src

SHE.Finance.Domain

SHE.Finance.Application

SHE.Finance.Infrastructure

SHE.Finance.Web

SHE.Finance.Worker

---

# PHASE 1

# FOUNDATION

Objetivo

Construir la base del sistema.

---

## Entidades

BaseEntity

AuditableEntity

User

Role

Permission

---

## Infraestructura

Authentication

Authorization

JWT

Refresh Tokens

---

## Entregables

Login

Logout

Roles

Usuarios

Permisos

Auditoría

---

# PHASE 2

# CLIENT MANAGEMENT

Objetivo

Administrar clientes.

---

## Entidades

Client

ClientTag

ClientCategory

ClientNote

ClientContact

---

## Funciones

CRUD Clientes

Etiquetas

Clasificaciones

Notas

Historial

---

## Entregables

Pantalla Clientes

Detalle Cliente

Historial Cliente

---

# PHASE 3

# SERVICE CATALOG

Objetivo

Administrar los servicios que vende la empresa.

---

## Entidades

Service

ServiceCategory

BillingType

ServicePriceHistory

---

## Billing Types

Monthly

Yearly

OneTime

Hourly

Custom

---

## Entregables

CRUD Servicios

Categorías

Historial de precios

---

# PHASE 4

# SUBSCRIPTIONS

Objetivo

Administrar contratos entre clientes y servicios.

---

## Entidades

Subscription

SubscriptionStatus

SubscriptionPriceHistory

SubscriptionChangeLog

---

## Estados

Active

Suspended

Cancelled

Expired

Pending

---

## Entregables

CRUD Suscripciones

Suspensión

Reactivación

Historial

---

# PHASE 5

# BILLING ENGINE

Objetivo

Generar automáticamente los cargos esperados.

---

## Entidades

BillingCycle

BillingItem

BillingGenerationLog

---

## Funciones

Generación mensual

Generación anual

Reprocesamiento

Correcciones

---

## Entregables

Motor de generación

Vista de cargos esperados

---

# PHASE 6

# FINANCIAL LEDGER

Objetivo

Crear el núcleo financiero.

---

## Entidades

BankAccount

LedgerEntry

LedgerEntryType

LedgerAllocation

TransferGroup

---

## Tipos

Income

Expense

TransferIn

TransferOut

Adjustment

---

## Entregables

Registro movimientos

Transferencias

Ajustes

Conciliación

---

# PHASE 7

# SUBSCRIPTION ALLOCATIONS

Objetivo

Aplicar pagos a servicios específicos.

---

## Entidades

SubscriptionAllocation

---

## Casos

Pago parcial

Pago adelantado

Pago múltiple

Pago combinado

---

## Entregables

Pantalla Asignaciones

Asignación automática

Asignación manual

---

# PHASE 8

# CUSTOMER ACCOUNT STATEMENTS

Objetivo

Estados de cuenta por cliente.

---

## Funciones

Cargos

Pagos

Ajustes

Saldo

---

## Entregables

Estado de Cuenta

Detalle de movimientos

Resumen financiero

---

# PHASE 9

# BANK RECONCILIATION

Objetivo

Conciliar bancos.

---

## Funciones

Estado de cuenta

Balance

Conciliación

Transferencias

---

## Entregables

Vista bancaria

Reporte conciliación

---

# PHASE 10

# DASHBOARD

Objetivo

Indicadores de negocio.

---

## KPIs

Clientes Activos

Clientes Morosos

MRR

ARR

Ingresos Mes

Ingresos Año

Pendiente Cobro

---

## Entregables

Dashboard Ejecutivo

Dashboard Financiero

---

# PHASE 11

# REPORTING

Objetivo

Reportes empresariales.

---

## Reportes

Clientes

Servicios

Suscripciones

Ingresos

Gastos

Morosidad

Conciliación

---

## Tecnología

AegiReports

---

# PHASE 12

# SUPPORT TICKETS

Objetivo

Administrar soporte.

---

## Entidades

Ticket

TicketComment

TicketAttachment

---

## Entregables

Mesa de ayuda

Historial

Seguimiento

---

# PHASE 13

# LICENSE MANAGEMENT

Objetivo

Administrar licencias de software.

---

## Entidades

License

Installation

Activation

LicenseValidationLog

---

## Compatible con

SHEndevour

AegiDocs

Futuros productos

---

# PHASE 14

# CUSTOMER PORTAL

Objetivo

Portal de autoservicio.

---

## Funciones

Ver suscripciones

Ver pagos

Ver facturación

Ver licencias

Crear tickets

---

# PHASE 15

# AUTOMATIONS

Objetivo

Reducir operación manual.

---

## Procesos

Recordatorios

Vencimientos

Morosidad

Renovaciones

Suspensiones

---

# MVP REAL

Para comenzar a usar el sistema internamente NO es necesario llegar a la fase 15.

La primera versión operativa queda terminada al finalizar

PHASE 1
PHASE 2
PHASE 3
PHASE 4
PHASE 5
PHASE 6
PHASE 7
PHASE 8

Con esas fases ya puedes administrar clientes, planes, mensualidades, pagos, estados de cuenta y conciliación financiera.

Todo lo posterior es crecimiento empresarial.
