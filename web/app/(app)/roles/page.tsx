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
import { AssignPermissionsDialog } from "@/components/modules/roles/assign-permissions-dialog";
import { Can } from "@/lib/auth/can";
import type { PermissionDto, RoleDto } from "@/types/api";
import { MoreHorizontal, Plus, Shield, Trash2 } from "lucide-react";
import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";

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
        <span className="font-600 text-[#16181D]">{getValue() as string}</span>
      ),
    },
    {
      accessorKey: "description",
      header: "Descripción",
      cell: ({ getValue }) => (
        <span className="text-[#5B6472]">{(getValue() as string) || "—"}</span>
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
                <Button variant="ghost" size="icon" aria-label="Más acciones" onClick={(e) => e.stopPropagation()}>
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenu.Trigger>
              <DropdownMenu.Portal>
                <DropdownMenu.Content className="z-50 min-w-[160px] overflow-hidden rounded-table border border-[#E3E6EC] bg-white shadow-dp2" align="end" sideOffset={4}>
                  <DropdownMenu.Item onSelect={() => { setEditingRole(role); setRoleFormOpen(true); }} className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-[#3A3F4B] hover:bg-[#F7F8FA] cursor-pointer focus:outline-none">
                    Editar
                  </DropdownMenu.Item>
                  <DropdownMenu.Item onSelect={() => { setSelectedRole(role); setPermissionsDialogOpen(true); }} className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-[#3A3F4B] hover:bg-[#F7F8FA] cursor-pointer focus:outline-none">
                    <Shield className="h-3.5 w-3.5" />
                    Permisos
                  </DropdownMenu.Item>
                  <DropdownMenu.Separator className="my-1 h-px bg-[#F7F8FA]" />
                  <DropdownMenu.Item
                    onSelect={() => { if (confirm(`¿Eliminar el rol "${role.name}"?`)) deleteRole.mutate(role.id); }}
                    className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-[#B6452C] hover:bg-[#FFF6F1] cursor-pointer focus:outline-none"
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
        <code className="rounded bg-[#F7F8FA] px-1.5 py-0.5 text-[11px] text-[#0F5C6B]">
          {getValue() as string}
        </code>
      ),
    },
    {
      accessorKey: "name",
      header: "Nombre",
      cell: ({ getValue }) => <span className="font-500 text-[#16181D]">{getValue() as string}</span>,
    },
    {
      accessorKey: "description",
      header: "Descripción",
      cell: ({ getValue }) => (
        <span className="text-[#5B6472]">{(getValue() as string) || "—"}</span>
      ),
    },
    {
      id: "actions",
      size: 48,
      cell: ({ row }) => {
        const perm = row.original;
        return (
          <Can permission="ManageRoles">
            <Button
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
              <Trash2 className="h-3.5 w-3.5 text-[#B6452C]" />
            </Button>
          </Can>
        );
      },
    },
  ];

  return (
    <div className="space-y-8">
      {/* ROLES */}
      <section>
        <div className="mb-5 flex items-center justify-between">
          <div>
            <h1 className="font-display text-[22px] font-700 text-[#16181D]">Roles y Permisos</h1>
            <p className="mt-0.5 text-[13px] text-[#5B6472]">Gestión de roles del sistema</p>
          </div>
          <Can permission="ManageRoles">
            <Button onClick={() => { setEditingRole(null); setRoleFormOpen(true); }}>
              <Plus className="h-4 w-4" />
              Nuevo rol
              <kbd className="ml-1 rounded bg-white/20 px-1 text-[10px]">N</kbd>
            </Button>
          </Can>
        </div>
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
            <h2 className="font-display text-[16px] font-700 text-[#16181D]">Permisos</h2>
            <p className="mt-0.5 text-[12px] text-[#5B6472]">Catálogo de permisos del sistema</p>
          </div>
          <Can permission="ManageRoles">
            <Button
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

      {/* Dialogs */}
      <RoleForm open={roleFormOpen} onOpenChange={setRoleFormOpen} editingRole={editingRole} />
      <AssignPermissionsDialog open={permissionsDialogOpen} onOpenChange={setPermissionsDialogOpen} role={selectedRole} />
      <NewPermissionDialog open={permDialogOpen} onOpenChange={setPermDialogOpen} />
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
    });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Nuevo permiso" size="sm">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input
            {...register("code")}
            label="Código"
            placeholder="Ej. ManageClients"
            autoFocus
            error={errors.code?.message}
            hint="CamelCase sin espacios. Ej: ViewClients, ManageBilling"
          />
          <Input
            {...register("name")}
            label="Nombre"
            placeholder="Ej. Gestionar clientes"
            error={errors.name?.message}
          />
          <Input
            {...register("description")}
            label="Descripción (opcional)"
            placeholder="Descripción breve del permiso"
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button type="submit" loading={isSubmitting}>Crear permiso</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
