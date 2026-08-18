"use client";

import { useEffect, useRef, useState } from "react";
import { type ColumnDef } from "@tanstack/react-table";
import { useUsers, useDeleteUser, useToggleUserActive, useUnlockUser } from "@/hooks/use-users";
import { DataTable } from "@/components/ui/data-table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Pagination } from "@/components/ui/pagination";
import { UserForm } from "@/components/modules/users/user-form";
import { UserAccessDialog } from "@/components/modules/users/user-access-dialog";
import { ResetPasswordDialog } from "@/components/modules/users/reset-password-dialog";
import { Can } from "@/lib/auth/can";
import { formatDate, getUserTypeLabel } from "@/lib/utils/format";
import type { UserDto } from "@/types/api";
import {
  MoreHorizontal,
  Plus,
  Search,
  ShieldCheck,
  MonitorSmartphone,
  KeyRound,
  LockOpen,
  Trash2,
  UserCheck,
  UserX,
} from "lucide-react";
import * as DropdownMenu from "@radix-ui/react-dropdown-menu";

const PAGE_SIZE = 15;

export default function UsersPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [formOpen, setFormOpen] = useState(false);
  const [accessOpen, setAccessOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<UserDto | null>(null);
  const [accessUser, setAccessUser] = useState<UserDto | null>(null);
  const [passwordUser, setPasswordUser] = useState<UserDto | null>(null);
  const [passwordOpen, setPasswordOpen] = useState(false);
  const searchRef = useRef<HTMLInputElement>(null);

  // Debounce search
  useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 350);
    return () => clearTimeout(t);
  }, [search]);

  // Keyboard shortcuts
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      const tag = document.activeElement?.tagName ?? "";
      const isInput = tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT";

      if (!isInput) {
        if (e.key === "n" || e.key === "N") {
          e.preventDefault();
          setEditingUser(null);
          setFormOpen(true);
        }
        if (e.key === "/") {
          e.preventDefault();
          searchRef.current?.focus();
        }
      }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, []);

  const { data, isLoading } = useUsers({
    searchTerm: debouncedSearch || undefined,
    pageNumber: page,
    pageSize: PAGE_SIZE,
  });

  const deleteUser = useDeleteUser();
  const toggleActive = useToggleUserActive();
  const unlockUser = useUnlockUser();

  const openEdit = (user: UserDto) => {
    setEditingUser(user);
    setFormOpen(true);
  };

  const openRoles = (user: UserDto) => {
    setAccessUser(user);
    setAccessOpen(true);
  };

  const columns: ColumnDef<UserDto, unknown>[] = [
    {
      accessorKey: "name",
      header: "Nombre",
      cell: ({ row }) => (
        <div>
          <p className="font-semibold text-foreground">{row.original.name}</p>
          <p className="text-[11px] text-muted">@{row.original.userName}</p>
          {row.original.mustChangePassword && <span className="mt-1 inline-block text-[10px] font-bold text-warning">Cambio de contraseña pendiente</span>}
        </div>
      ),
    },
    {
      accessorKey: "email",
      header: "Correo",
      cell: ({ getValue }) => (
        <span className="text-foreground-secondary">{getValue() as string}</span>
      ),
    },
    {
      accessorKey: "userType",
      header: "Tipo",
      size: 120,
      cell: ({ getValue }) => (
        <Badge variant={getValue() === "Administrator" ? "primary" : "periwinkle"}>
          {getUserTypeLabel(getValue() as string)}
        </Badge>
      ),
    },
    {
      accessorKey: "roles",
      header: "Roles",
      cell: ({ getValue }) => {
        const roles = getValue() as string[];
        return roles.length === 0 ? (
          <span className="text-muted">—</span>
        ) : (
          <div className="flex flex-wrap gap-1">
            {roles.map((r) => (
              <Badge key={r} variant="muted">{r}</Badge>
            ))}
          </div>
        );
      },
    },
    {
      accessorKey: "isActive",
      header: "Estado",
      size: 100,
      cell: ({ row }) => {
        const locked = !!row.original.lockoutEnd && new Date(row.original.lockoutEnd) > new Date();
        return <Badge variant={locked ? "saffron" : row.original.isActive ? "jade" : "terracotta"}>{locked ? "Bloqueado" : row.original.isActive ? "Activo" : "Inactivo"}</Badge>;
      },
    },
    {
      accessorKey: "lastLoginAt",
      header: "Último acceso",
      size: 130,
      cell: ({ getValue }) => (
        <span className="text-muted">{formatDate(getValue() as string | null)}</span>
      ),
    },
    {
      accessorKey: "activeSessionCount",
      header: "Sesiones",
      size: 90,
      cell: ({ row }) => <span className="inline-flex items-center gap-1.5 text-xs font-semibold text-foreground-secondary"><MonitorSmartphone className="h-3.5 w-3.5 text-action" />{row.original.activeSessionCount}</span>,
    },
    {
      id: "actions",
      size: 48,
      cell: ({ row }) => {
        const user = row.original;
        return (
          <Can permission="ManageUsers">
            <DropdownMenu.Root>
              <DropdownMenu.Trigger asChild>
                <Button controlKey="ui.app.app.users.page.button.1"
                  variant="ghost"
                  size="icon"
                  aria-label="Más acciones"
                  onClick={(e) => e.stopPropagation()}
                >
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenu.Trigger>
              <DropdownMenu.Portal>
                <DropdownMenu.Content
                  className="z-50 min-w-[160px] overflow-hidden rounded-table border border-border bg-surface shadow-dp2"
                  align="end"
                  sideOffset={4}
                >
                  <DropdownMenu.Item
                    onSelect={() => openEdit(user)}
                    className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle cursor-pointer focus:outline-none"
                  >
                    Editar
                  </DropdownMenu.Item>
                  <DropdownMenu.Item
                    onSelect={() => openRoles(user)}
                    className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle cursor-pointer focus:outline-none"
                  >
                    <ShieldCheck className="h-3.5 w-3.5" />
                    Acceso y sesiones
                  </DropdownMenu.Item>
                  <DropdownMenu.Item data-ui-control="users.actions.reset-password" data-ui-permission="ManageUsers" onSelect={() => { setPasswordUser(user); setPasswordOpen(true); }} className="flex min-h-11 items-center gap-2 px-3 py-2 text-[13px] text-foreground-secondary hover:bg-surface-subtle cursor-pointer focus:outline-none">
                    <KeyRound className="h-3.5 w-3.5" />Restablecer contraseña
                  </DropdownMenu.Item>
                  {user.lockoutEnd && new Date(user.lockoutEnd) > new Date() && <DropdownMenu.Item data-ui-control="users.actions.unlock" data-ui-permission="ManageUsers" onSelect={() => unlockUser.mutate(user.id)} className="flex min-h-11 items-center gap-2 px-3 py-2 text-[13px] text-foreground-secondary hover:bg-surface-subtle cursor-pointer focus:outline-none"><LockOpen className="h-3.5 w-3.5 text-warning" />Desbloquear</DropdownMenu.Item>}
                  <DropdownMenu.Separator className="my-1 h-px bg-surface-subtle" />
                  <DropdownMenu.Item
                    onSelect={() => toggleActive.mutate({ id: user.id, active: !user.isActive })}
                    className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle cursor-pointer focus:outline-none"
                  >
                    {user.isActive ? (
                      <><UserX className="h-3.5 w-3.5 text-warning" /> Desactivar</>
                    ) : (
                      <><UserCheck className="h-3.5 w-3.5 text-success" /> Activar</>
                    )}
                  </DropdownMenu.Item>
                  <DropdownMenu.Item
                    onSelect={() => {
                      if (confirm(`¿Eliminar al usuario "${user.userName}"? Esta acción es irreversible.`)) {
                        deleteUser.mutate(user.id);
                      }
                    }}
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

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 rounded-hero border border-border bg-gradient-to-br from-action-soft via-surface to-accent-soft p-5 sm:flex-row sm:items-end sm:justify-between sm:p-6">
        <div>
          <p className="text-xs font-bold uppercase tracking-[0.14em] text-action">Administración de acceso</p>
          <h1 className="mt-2 font-display text-[24px] font-bold text-foreground">Usuarios</h1>
          <p className="mt-0.5 text-[13px] text-muted">
            Gestión de accesos al sistema
          </p>
        </div>
        <Can permission="ManageUsers">
          <Button controlKey="ui.app.app.users.page.button.2"
            onClick={() => { setEditingUser(null); setFormOpen(true); }}
            size="md"
          >
            <Plus className="h-4 w-4" />
            Nuevo
            <kbd className="ml-1 rounded bg-surface/20 px-1 text-[10px]">N</kbd>
          </Button>
        </Can>
      </div>

      {/* Filtros */}
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
        <div className="w-full sm:max-w-sm">
          <Input controlKey="ui.app.app.users.page.input.1"
            ref={searchRef}
            placeholder="Buscar usuario…"
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            leftIcon={<Search className="h-3.5 w-3.5" />}
          />
        </div>
        <kbd className="rounded bg-border px-1.5 py-0.5 text-[10px] font-semibold text-muted">
          /
        </kbd>
      </div>

      {/* Tabla */}
      <DataTable
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        emptyMessage="No se encontraron usuarios."
        onRowClick={openEdit}
        getRowId={(row) => row.id}
      />

      {/* Paginación */}
      {data && data.totalCount > PAGE_SIZE && (
        <Pagination
          page={page}
          totalPages={data.totalPages}
          totalCount={data.totalCount}
          pageSize={PAGE_SIZE}
          onPageChange={setPage}
        />
      )}

      {/* Dialogs */}
      <UserForm
        open={formOpen}
        onOpenChange={setFormOpen}
        editingUser={editingUser}
      />
      <UserAccessDialog
        open={accessOpen}
        onOpenChange={setAccessOpen}
        user={accessUser}
      />
      <ResetPasswordDialog open={passwordOpen} onOpenChange={setPasswordOpen} user={passwordUser} />
    </div>
  );
}
