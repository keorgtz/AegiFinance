"use client";

import { useEffect, useRef, useState } from "react";
import { type ColumnDef } from "@tanstack/react-table";
import { useUsers, useDeleteUser, useToggleUserActive } from "@/hooks/use-users";
import { DataTable } from "@/components/ui/data-table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Pagination } from "@/components/ui/pagination";
import { UserForm } from "@/components/modules/users/user-form";
import { AssignRolesDialog } from "@/components/modules/users/assign-roles-dialog";
import { Can } from "@/lib/auth/can";
import { formatDate, getUserTypeLabel } from "@/lib/utils/format";
import type { UserDto } from "@/types/api";
import {
  MoreHorizontal,
  Plus,
  Search,
  ShieldCheck,
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
  const [rolesOpen, setRolesOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<UserDto | null>(null);
  const [rolesUser, setRolesUser] = useState<UserDto | null>(null);
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

  const openEdit = (user: UserDto) => {
    setEditingUser(user);
    setFormOpen(true);
  };

  const openRoles = (user: UserDto) => {
    setRolesUser(user);
    setRolesOpen(true);
  };

  const columns: ColumnDef<UserDto, unknown>[] = [
    {
      accessorKey: "name",
      header: "Nombre",
      cell: ({ row }) => (
        <div>
          <p className="font-600 text-[#16181D]">{row.original.name}</p>
          <p className="text-[11px] text-[#5B6472]">@{row.original.userName}</p>
        </div>
      ),
    },
    {
      accessorKey: "email",
      header: "Correo",
      cell: ({ getValue }) => (
        <span className="text-[#3A3F4B]">{getValue() as string}</span>
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
          <span className="text-[#5B6472]">—</span>
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
      cell: ({ getValue }) => (
        <Badge variant={getValue() ? "jade" : "terracotta"}>
          {getValue() ? "Activo" : "Inactivo"}
        </Badge>
      ),
    },
    {
      accessorKey: "lastLoginAt",
      header: "Último acceso",
      size: 130,
      cell: ({ getValue }) => (
        <span className="text-[#5B6472]">{formatDate(getValue() as string | null)}</span>
      ),
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
                <Button
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
                  className="z-50 min-w-[160px] overflow-hidden rounded-table border border-[#E3E6EC] bg-white shadow-dp2"
                  align="end"
                  sideOffset={4}
                >
                  <DropdownMenu.Item
                    onSelect={() => openEdit(user)}
                    className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-[#3A3F4B] hover:bg-[#F7F8FA] cursor-pointer focus:outline-none"
                  >
                    Editar
                  </DropdownMenu.Item>
                  <DropdownMenu.Item
                    onSelect={() => openRoles(user)}
                    className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-[#3A3F4B] hover:bg-[#F7F8FA] cursor-pointer focus:outline-none"
                  >
                    <ShieldCheck className="h-3.5 w-3.5" />
                    Asignar roles
                  </DropdownMenu.Item>
                  <DropdownMenu.Separator className="my-1 h-px bg-[#F7F8FA]" />
                  <DropdownMenu.Item
                    onSelect={() => toggleActive.mutate({ id: user.id, active: !user.isActive })}
                    className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-[#3A3F4B] hover:bg-[#F7F8FA] cursor-pointer focus:outline-none"
                  >
                    {user.isActive ? (
                      <><UserX className="h-3.5 w-3.5 text-[#B7791F]" /> Desactivar</>
                    ) : (
                      <><UserCheck className="h-3.5 w-3.5 text-[#0E9F6E]" /> Activar</>
                    )}
                  </DropdownMenu.Item>
                  <DropdownMenu.Item
                    onSelect={() => {
                      if (confirm(`¿Eliminar al usuario "${user.userName}"? Esta acción es irreversible.`)) {
                        deleteUser.mutate(user.id);
                      }
                    }}
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

  return (
    <div>
      {/* Header */}
      <div className="mb-5 flex items-center justify-between gap-4">
        <div>
          <h1 className="font-display text-[22px] font-700 text-[#16181D]">Usuarios</h1>
          <p className="mt-0.5 text-[13px] text-[#5B6472]">
            Gestión de accesos al sistema
          </p>
        </div>
        <Can permission="ManageUsers">
          <Button
            onClick={() => { setEditingUser(null); setFormOpen(true); }}
            size="md"
          >
            <Plus className="h-4 w-4" />
            Nuevo
            <kbd className="ml-1 rounded bg-white/20 px-1 text-[10px]">N</kbd>
          </Button>
        </Can>
      </div>

      {/* Filtros */}
      <div className="mb-4 flex items-center gap-3">
        <div className="w-72">
          <Input
            ref={searchRef}
            placeholder="Buscar usuario…"
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            leftIcon={<Search className="h-3.5 w-3.5" />}
          />
        </div>
        <kbd className="rounded bg-[#E3E6EC] px-1.5 py-0.5 text-[10px] font-600 text-[#5B6472]">
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
      <AssignRolesDialog
        open={rolesOpen}
        onOpenChange={setRolesOpen}
        user={rolesUser}
      />
    </div>
  );
}
