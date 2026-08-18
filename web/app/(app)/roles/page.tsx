"use client";

import { useEffect, useRef, useState } from "react";
import { type ColumnDef } from "@tanstack/react-table";
import { useRoles, useDeleteRole } from "@/hooks/use-roles";
import {
  usePermissionsData,
  useCreatePermission,
  useDeletePermission,
} from "@/hooks/use-permissions-data";
import { DataTable } from "@/components/ui/data-table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { RoleForm } from "@/components/modules/roles/role-form";
import { UiPermissionManager } from "@/components/modules/roles/ui-permission-manager";
import { AssignPermissionsDialog } from "@/components/modules/roles/assign-permissions-dialog";
import { Can } from "@/lib/auth/can";
import type { PermissionDto, RoleDto } from "@/types/api";
import { MoreHorizontal, Plus, Shield, Trash2 } from "lucide-react";
import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useQuery } from "@tanstack/react-query";
import { RoleComparisonDialog } from "@/components/modules/roles/role-comparison-dialog";
import { auditLogsApi } from "@/lib/api/audit-logs";
import { formatDateTime } from "@/lib/utils/format";
import { GitCompareArrows, History } from "lucide-react";

export default function RolesPage() {
  const { data: roles = [], isLoading: rolesLoading } = useRoles();
  const { data: permissions = [], isLoading: permsLoading } = usePermissionsData();
  const deleteRole = useDeleteRole();
  const deletePermission = useDeletePermission();

  const [roleFormOpen, setRoleFormOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<RoleDto | null>(null);
  const [permDialogOpen, setPermDialogOpen] = useState(false);
  const [permissionsDialogOpen, setPermissionsDialogOpen] = useState(false);
  const [selectedRole, setSelectedRole] = useState<RoleDto | null>(null);
  const [compareOpen, setCompareOpen] = useState(false);
  const roleHistory = useQuery({ queryKey: ["audit-logs", "roles"], queryFn: () => auditLogsApi.list({ entityType: "RolePermission", pageSize: 12 }) });

  // Keyboard shortcuts
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      const tag = document.activeElement?.tagName ?? "";
      const isInput = tag === "INPUT" || tag === "TEXTAREA";
      if (!isInput && (e.key === "n" || e.key === "N")) {
        e.preventDefault();
        setEditingRole(null);
        setRoleFormOpen(true);
      }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, []);

  const roleColumns: ColumnDef<RoleDto, unknown>[] = [
    {
      accessorKey: "name",
      header: "Nombre",
      cell: ({ getValue }) => (
        <span className="font-semibold text-foreground">{getValue() as string}</span>
      ),
    },
    {
      accessorKey: "description",
      header: "Descripción",
      cell: ({ getValue }) => (
        <span className="text-muted">{(getValue() as string) || "—"}</span>
      ),
    },
    {
      accessorKey: "userType",
      header: "Tipo",
      size: 130,
      cell: ({ getValue }) => {
        const v = getValue() as string | null;
        if (!v) return <Badge variant="muted">General</Badge>;
        return <Badge variant={v === "Administrator" ? "primary" : "periwinkle"}>{v === "Administrator" ? "Administrador" : "Cliente"}</Badge>;
      },
    },
    {
      id: "actions",
      size: 48,
      cell: ({ row }) => {
        const role = row.original;
        return (
          <Can permission="ManageRoles">
            <DropdownMenu.Root>
              <DropdownMenu.Trigger asChild>
                <Button controlKey="ui.app.app.roles.page.button.1" variant="ghost" size="icon" aria-label="Más acciones" onClick={(e) => e.stopPropagation()}>
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenu.Trigger>
              <DropdownMenu.Portal>
                <DropdownMenu.Content className="z-50 min-w-[160px] overflow-hidden rounded-table border border-border bg-surface shadow-dp2" align="end" sideOffset={4}>
                  <DropdownMenu.Item onSelect={() => { setEditingRole(role); setRoleFormOpen(true); }} className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle cursor-pointer focus:outline-none">
                    Editar
                  </DropdownMenu.Item>
                  <DropdownMenu.Item onSelect={() => { setSelectedRole(role); setPermissionsDialogOpen(true); }} className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle cursor-pointer focus:outline-none">
                    <Shield className="h-3.5 w-3.5" />
                    Permisos
                  </DropdownMenu.Item>
                  <DropdownMenu.Separator className="my-1 h-px bg-surface-subtle" />
                  <DropdownMenu.Item
                    onSelect={() => { if (confirm(`¿Eliminar el rol "${role.name}"?`)) deleteRole.mutate(role.id); }}
                    className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-danger hover:bg-danger-soft cursor-pointer focus:outline-none"
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                    Eliminar
                  </DropdownMenu.Item>
                </DropdownMenu.Content>
              </DropdownMenu.Portal>
            </DropdownMenu.Root>
          </Can>
        );
      },
    },
  ];

  const permColumns: ColumnDef<PermissionDto, unknown>[] = [
    {
      accessorKey: "code",
      header: "Código",
      cell: ({ getValue }) => (
        <code className="rounded bg-surface-subtle px-1.5 py-0.5 text-[11px] text-action">
          {getValue() as string}
        </code>
      ),
    },
    {
      accessorKey: "name",
      header: "Nombre",
      cell: ({ getValue }) => <span className="font-medium text-foreground">{getValue() as string}</span>,
    },
    {
      accessorKey: "description",
      header: "Descripción",
      cell: ({ getValue }) => (
        <span className="text-muted">{(getValue() as string) || "—"}</span>
      ),
    },
    {
      id: "actions",
      size: 48,
      cell: ({ row }) => {
        const perm = row.original;
        return (
          <Can permission="ManageRoles">
            <Button controlKey="ui.app.app.roles.page.button.2"
              variant="ghost"
              size="icon"
              aria-label="Eliminar permiso"
              onClick={(e) => {
                e.stopPropagation();
                if (confirm(`¿Eliminar el permiso "${perm.code}"?`)) {
                  deletePermission.mutate(perm.id);
                }
              }}
            >
              <Trash2 className="h-3.5 w-3.5 text-danger" />
            </Button>
          </Can>
        );
      },
    },
  ];

  return (
    <div className="space-y-8">
      {/* ROLES */}
      <section className="rounded-hero border border-border bg-gradient-to-br from-action-soft via-surface to-accent-soft p-5 sm:p-6">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <p className="text-xs font-bold uppercase tracking-[0.14em] text-action">Modelo de acceso</p>
            <h1 className="mt-2 font-display text-[24px] font-bold text-foreground">Roles y permisos</h1>
            <p className="mt-0.5 text-[13px] text-muted">Gestión de roles del sistema</p>
          </div>
          <Can permission="ManageRoles">
            <div className="flex flex-wrap gap-2"><Button controlKey="roles.compare.open" permission="ManageRoles" variant="outline" onClick={() => setCompareOpen(true)}><GitCompareArrows className="h-4 w-4" />Comparar</Button><Button controlKey="ui.app.app.roles.page.button.4" onClick={() => { setEditingRole(null); setRoleFormOpen(true); }}><Plus className="h-4 w-4" />Nuevo rol<kbd className="ml-1 rounded bg-surface/20 px-1 text-[10px]">N</kbd></Button></div>
          </Can>
        </div>
      </section>
      <section>
        <DataTable
          columns={roleColumns}
          data={roles}
          isLoading={rolesLoading}
          emptyMessage="No hay roles configurados."
        />
      </section>

      {/* PERMISOS */}
      <section>
        <div className="mb-4 flex items-center justify-between">
          <div>
            <h2 className="font-display text-[16px] font-bold text-foreground">Permisos</h2>
            <p className="mt-0.5 text-[12px] text-muted">Catálogo de permisos del sistema</p>
          </div>
          <Can permission="ManageRoles">
            <Button controlKey="ui.app.app.roles.page.button.5"
              variant="outline"
              size="sm"
              onClick={() => setPermDialogOpen(true)}
            >
              <Plus className="h-3.5 w-3.5" />
              Nuevo permiso
            </Button>
          </Can>
        </div>
        <DataTable
          columns={permColumns}
          data={permissions}
          isLoading={permsLoading}
          emptyMessage="No hay permisos registrados."
        />
      </section>

      <UiPermissionManager />

      <section className="rounded-panel border border-border bg-surface p-5 shadow-dp1" aria-labelledby="access-history-title">
        <div className="flex items-center gap-3"><span className="grid h-10 w-10 place-items-center rounded-input bg-action-soft text-action"><History className="h-4 w-4" /></span><div><h2 id="access-history-title" className="font-display text-base font-bold text-foreground">Historial de permisos</h2><p className="text-xs text-muted">Últimas concesiones, denegaciones y revocaciones registradas.</p></div></div>
        <div className="mt-4 space-y-2">{roleHistory.isLoading ? <p className="text-sm text-muted">Cargando historial…</p> : roleHistory.isError ? <p className="text-sm text-danger">No se pudo cargar el historial.</p> : roleHistory.data?.items.length ? roleHistory.data.items.map((item) => <div key={item.id} className="flex min-h-12 items-center justify-between gap-3 rounded-input bg-surface-subtle px-3 py-2"><div><p className="text-sm font-semibold text-foreground">{item.action}</p><p className="text-xs text-muted">{item.userName || "Sistema"}</p></div><time className="text-xs text-muted">{formatDateTime(item.timestamp)}</time></div>) : <p className="rounded-input bg-surface-subtle p-4 text-sm text-muted">Todavía no hay cambios registrados.</p>}</div>
      </section>

      {/* Dialogs */}
      <RoleForm open={roleFormOpen} onOpenChange={setRoleFormOpen} editingRole={editingRole} />
      <AssignPermissionsDialog open={permissionsDialogOpen} onOpenChange={setPermissionsDialogOpen} role={selectedRole} />
      <NewPermissionDialog open={permDialogOpen} onOpenChange={setPermDialogOpen} />
      <RoleComparisonDialog open={compareOpen} onOpenChange={setCompareOpen} roles={roles} />
    </div>
  );
}

// Dialog inline para crear permiso
const permSchema = z.object({
  code: z.string().min(1, "Requerido").regex(/^[A-Za-z]+$/, "Solo letras, sin espacios"),
  name: z.string().min(1, "Requerido"),
  description: z.string().optional(),
});

function NewPermissionDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (v: boolean) => void }) {
  const createPermission = useCreatePermission();
  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(permSchema),
    defaultValues: { code: "", name: "", description: "" },
  });

  useEffect(() => {
    if (open) reset({ code: "", name: "", description: "" });
  }, [open, reset]);

  const onSubmit = async (values: { code: string; name: string; description?: string }) => {
    await createPermission.mutateAsync({
      code: values.code,
      name: values.name,
      description: values.description || null,
      module: values.code.replace(/^(View|Create|Update|Delete|Manage)/, "") || "System",
      action: values.code.match(/^(View|Create|Update|Delete|Manage)/)?.[0] || "Execute",
    });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Nuevo permiso" size="sm">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input controlKey="ui.app.app.roles.page.input.1"
            {...register("code")}
            label="Código"
            placeholder="Ej. ManageClients"
            autoFocus
            error={errors.code?.message}
            hint="CamelCase sin espacios. Ej: ViewClients, ManageBilling"
          />
          <Input controlKey="ui.app.app.roles.page.input.2"
            {...register("name")}
            label="Nombre"
            placeholder="Ej. Gestionar clientes"
            error={errors.name?.message}
          />
          <Input controlKey="ui.app.app.roles.page.input.3"
            {...register("description")}
            label="Descripción (opcional)"
            placeholder="Descripción breve del permiso"
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.app.app.roles.page.button.6" type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button controlKey="ui.app.app.roles.page.button.7" type="submit" loading={isSubmitting}>Crear permiso</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
