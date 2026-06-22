# PLAN_UPDATE_PORTAL_CLIENTE.md

# AegiFinance

## Actualización de Arquitectura de Usuarios, Portal Cliente y Seguridad

---

# OBJETIVO

Agregar un Portal Cliente completamente integrado dentro de AegiFinance.

No existirá una aplicación separada.

No existirán vistas duplicadas.

Todo funcionará dentro del mismo sistema.

---

# REGLA ARQUITECTÓNICA OBLIGATORIA

## SINGLE VIEW ARCHITECTURE (SVA)

Cada módulo tendrá una única vista.

Ejemplos:

* Clientes
* Suscripciones
* Conciliación
* Dashboard
* Reportes
* Estado de Cuenta
* Usuarios

Existirá solamente una implementación.

---

## Prohibido

```text
DashboardAdmin.razor

DashboardClient.razor

DashboardManager.razor
```

---

## Correcto

```text
Dashboard.razor
```

La misma vista para todos.

---

# CONTROL DE VISIBILIDAD

Cada componente deberá verificar permisos.

Ejemplo:

```csharp
@if(Permissions.CanViewRevenue)
{
    ...
}
```

```csharp
@if(Permissions.CanManageUsers)
{
    ...
}
```

```csharp
@if(Permissions.CanViewReports)
{
    ...
}
```

---

# BENEFICIOS

Una sola vista.

Un solo mantenimiento.

Un solo flujo.

Menor deuda técnica.

Escalabilidad máxima.

---

# NUEVO MODELO DE USUARIOS

AegiFinance tendrá dos conceptos independientes:

## UserType

Define quién es.

## Role

Define qué puede hacer.

---

# USER TYPE

## Administrator

Usuarios internos de la empresa.

Ejemplos:

* Kevin
* Colaborador
* Contador
* Auxiliar

---

## Client

Usuarios pertenecientes a un cliente.

Ejemplos:

* Hotel Las Palmas
* Restaurante El Fogón
* Tortillería García

---

# ENTIDAD USER

```csharp
public class User
{
    Guid Id;

    string UserName;

    string PasswordHash;

    UserType Type;

    Guid? ClientId;

    bool Active;
}
```

---

# REGLA

## Administrator

ClientId = NULL

---

## Client

ClientId obligatorio

---

# ROLES

Los roles son independientes del tipo.

---

## Administradores

Admin

Supervisor

Conciliador

Ventas

Soporte

Consulta

---

## Clientes

Admin

Gerente

Consulta

Contador

Operador

---

# EJEMPLOS

Kevin

```text
Type = Administrator
Role = Admin
```

Acceso total.

---

Empleado de conciliación

```text
Type = Administrator
Role = Conciliador
```

Solo conciliación.

---

Cliente Principal

```text
Type = Client
Role = Admin
```

Administra su empresa.

---

Gerente Cliente

```text
Type = Client
Role = Gerente
```

Ve información limitada.

---

# NUEVO CONCEPTO

# CLIENT SUB USERS

Cada cliente podrá crear usuarios internos.

---

# EJEMPLO

Hotel Las Palmas

Usuario principal

```text
hotelpalmas
```

---

Subusuarios

```text
gerente1
contador1
recepcion1
```

---

# ENTIDAD

ClientUser

```csharp
public class ClientUser
{
    Guid Id;

    Guid ClientId;

    Guid UserId;

    string DisplayName;

    bool Active;
}
```

---

# PIN SIMPLE

Para facilitar uso.

---

## Usuario principal

Login normal.

```text
hotelpalmas
********
```

---

## Subusuario

Puede usar:

```text
1234
5678
9999
```

PIN configurable.

---

# ENTIDAD

ClientPinCredential

```csharp
public class ClientPinCredential
{
    Guid Id;

    Guid UserId;

    string PinHash;
}
```

---

# PERMISOS POR MÓDULO

No se controlará únicamente por rol.

También por permisos.

---

# PERMISSION SYSTEM

```csharp
Permission
{
    Code
    Name
}
```

---

# EJEMPLOS

ViewDashboard

ViewReports

ViewRevenue

ViewPayments

ViewSubscriptions

ViewLicenses

ViewTickets

ViewClients

ManageUsers

ManageBilling

ManageReconciliation

ManageRoles

---

# FILTRADO POR CLIENTE

Regla obligatoria.

---

## Cliente

Solo puede ver:

```text
ClientId = suyo
```

---

Nunca podrá consultar:

```text
Otros clientes
```

---

# FILTRADO POR SUSCRIPCIÓN

Subusuarios podrán ver solo ciertos planes.

---

# EJEMPLO

Cliente tiene:

Software Hotelero

Hosting

Mantenimiento

---

Gerente

Puede ver:

```text
Software Hotelero
```

---

No puede ver:

```text
Hosting
Mantenimiento
```

---

# NUEVA ENTIDAD

SubscriptionPermission

```csharp
public class SubscriptionPermission
{
    Guid UserId;

    Guid SubscriptionId;
}
```

---

# FILTRADO POR MÓDULO

Ejemplo:

---

Contador Cliente

Puede ver:

```text
Estado Cuenta
Pagos
Facturación
```

---

No puede ver:

```text
Usuarios
Tickets
Configuración
```

---

# DASHBOARD ADAPTATIVO

Misma vista.

Diferente contenido.

---

## Kevin

Ve:

```text
MRR

ARR

Todos los clientes

Conciliación

Reportes

Ingresos
```

---

## Cliente

Ve:

```text
Planes

Pagos

Adeudos

Estado Cuenta
```

---

## Gerente

Ve:

```text
Planes asignados

Pagos asignados
```

---

# PORTAL CLIENTE

No será una aplicación aparte.

Será un módulo de AegiFinance.

---

## Menú Dinámico

Construido por permisos.

---

Ejemplo:

```text
Dashboard

Mis Servicios

Estado Cuenta

Pagos

Tickets
```

---

Otro usuario:

```text
Dashboard

Conciliación

Reportes
```

---

# MATRIZ DE SEGURIDAD

Permiso

↓

Rol

↓

Usuario

↓

Cliente

↓

Suscripción

---

# REGLA FINAL

Toda funcionalidad nueva deberá cumplir:

1. Una sola vista.
2. Permisos dinámicos.
3. Menús dinámicos.
4. Filtrado por cliente.
5. Filtrado por suscripción.
6. Roles desacoplados de UserType.
7. Portal Cliente integrado.
8. Cero duplicación de pantallas.
9. Cero lógica duplicada.
10. Toda autorización centralizada mediante Permission Engine.

---

# RESULTADO

AegiFinance operará simultáneamente como:

* ERP financiero interno.
* Plataforma de cobranza.
* Gestor de suscripciones.
* Portal de clientes.
* Portal de colaboradores.

Todo dentro de una sola aplicación, una sola base de código, una sola UI y una sola arquitectura de permisos.
