"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { type ColumnDef } from "@tanstack/react-table";
import { useClients, useDeleteClient, useToggleClientStatus } from "@/hooks/use-clients";
import { useClientCategories } from "@/hooks/use-client-categories";
import { useClientTags } from "@/hooks/use-client-tags";
import { DataTable } from "@/components/ui/data-table";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Pagination } from "@/components/ui/pagination";
import { ClientForm } from "@/components/modules/clients/client-form";
import { ClientStatusBadge } from "@/components/modules/clients/client-status-badge";
import { TagChip } from "@/components/modules/clients/tag-chip";
import { Can } from "@/lib/auth/can";
import type { ClientListDto, ClientStatus } from "@/types/api";
import {
  Building2,
  MoreHorizontal,
  Plus,
  Search,
  Tag,
  Trash2,
  UserCheck,
  UserX,
} from "lucide-react";
import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import * as RadixSelect from "@radix-ui/react-select";
import { ChevronDown, Check } from "lucide-react";
import { cn } from "@/lib/utils/cn";

const PAGE_SIZE = 15;

function FilterSelect({
  label,
  value,
  onValueChange,
  children,
}: {
  label: string;
  value: string;
  onValueChange: (v: string) => void;
  children: React.ReactNode;
}) {
  return (
    <RadixSelect.Root value={value} onValueChange={onValueChange}>
      <RadixSelect.Trigger className="inline-flex h-8 items-center gap-1.5 rounded-input border border-[#E3E6EC] bg-white px-3 text-[13px] text-[#3A3F4B] shadow-dp1 hover:border-[#5BAEBC] focus:outline-none focus-visible:ring-2 focus-visible:ring-[#5BAEBC]">
        <RadixSelect.Value placeholder={label} />
        <ChevronDown className="h-3 w-3 text-[#5B6472]" />
      </RadixSelect.Trigger>
      <RadixSelect.Portal>
        <RadixSelect.Content
          className="z-50 min-w-[160px] overflow-hidden rounded-table border border-[#E3E6EC] bg-white shadow-dp2"
          position="popper"
          sideOffset={4}
        >
          <RadixSelect.Viewport className="p-1">
            {children}
          </RadixSelect.Viewport>
        </RadixSelect.Content>
      </RadixSelect.Portal>
    </RadixSelect.Root>
  );
}

function FilterItem({ value, children }: { value: string; children: React.ReactNode }) {
  return (
    <RadixSelect.Item
      value={value}
      className="flex cursor-pointer items-center justify-between rounded px-3 py-1.5 text-[13px] text-[#3A3F4B] hover:bg-[#F7F8FA] focus:bg-[#F7F8FA] focus:outline-none data-[state=checked]:text-[#0F5C6B] data-[state=checked]:font-600"
    >
      <RadixSelect.ItemText>{children}</RadixSelect.ItemText>
      <RadixSelect.ItemIndicator>
        <Check className="h-3.5 w-3.5" />
      </RadixSelect.ItemIndicator>
    </RadixSelect.Item>
  );
}

export default function ClientsPage() {
  const router = useRouter();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<ClientStatus | "">("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [tagFilter, setTagFilter] = useState("");
  const [formOpen, setFormOpen] = useState(false);
  const searchRef = useRef<HTMLInputElement>(null);

  const { data: categories = [] } = useClientCategories();
  const { data: allTags = [] } = useClientTags();

  // Debounce
  useEffect(() => {
    const t = setTimeout(() => { setDebouncedSearch(search); setPage(1); }, 350);
    return () => clearTimeout(t);
  }, [search]);

  // Reset page on filter change
  useEffect(() => { setPage(1); }, [statusFilter, categoryFilter, tagFilter]);

  // Keyboard shortcuts
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      const tag = (document.activeElement as HTMLElement)?.tagName ?? "";
      const isInput = tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT";
      if (isInput) return;
      if (e.key === "n" || e.key === "N") { e.preventDefault(); setFormOpen(true); }
      if (e.key === "/") { e.preventDefault(); searchRef.current?.focus(); }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, []);

  const { data, isLoading } = useClients({
    searchTerm: debouncedSearch || undefined,
    status: statusFilter || undefined,
    categoryId: categoryFilter || undefined,
    tagId: tagFilter || undefined,
    pageNumber: page,
    pageSize: PAGE_SIZE,
  });

  const deleteClient = useDeleteClient();
  const toggleStatus = useToggleClientStatus();

  const columns: ColumnDef<ClientListDto, unknown>[] = [
    {
      id: "client",
      header: "Cliente",
      cell: ({ row }) => {
        const c = row.original;
        return (
          <div className="flex items-center gap-2.5 min-w-0">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-card bg-[#EFF9F9] text-[#0F5C6B]">
              <Building2 className="h-4 w-4" />
            </div>
            <div className="min-w-0">
              <p className="truncate font-600 text-[#16181D]">{c.name}</p>
              {c.tradeName && (
                <p className="truncate text-[11px] text-[#5B6472]">{c.tradeName}</p>
              )}
            </div>
          </div>
        );
      },
    },
    {
      accessorKey: "code",
      header: "Código",
      size: 90,
      cell: ({ getValue }) => (
        <span className="font-mono text-[12px] text-[#5B6472]">{getValue() as string}</span>
      ),
    },
    {
      accessorKey: "status",
      header: "Estado",
      size: 110,
      cell: ({ getValue }) => <ClientStatusBadge status={getValue() as ClientStatus} />,
    },
    {
      accessorKey: "categoryName",
      header: "Categoría",
      size: 130,
      cell: ({ getValue }) => (
        <span className="text-[13px] text-[#5B6472]">{(getValue() as string | null) ?? "—"}</span>
      ),
    },
    {
      accessorKey: "primaryContactName",
      header: "Contacto principal",
      cell: ({ getValue }) => (
        <span className="text-[13px] text-[#3A3F4B]">{(getValue() as string | null) ?? "—"}</span>
      ),
    },
    {
      accessorKey: "tags",
      header: "Etiquetas",
      cell: ({ getValue }) => {
        const tags = getValue() as ClientListDto["tags"];
        return tags.length === 0 ? (
          <span className="text-[#5B6472]">—</span>
        ) : (
          <div className="flex flex-wrap gap-1">
            {tags.map((t) => <TagChip key={t.id} tag={t} />)}
          </div>
        );
      },
    },
    {
      id: "actions",
      size: 48,
      cell: ({ row }) => {
        const client = row.original;
        const isActive = client.status === "Active";
        return (
          <Can permission="ManageClients">
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
                    onSelect={() => router.push(`/clients/${client.id}`)}
                    className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-[#3A3F4B] hover:bg-[#F7F8FA] cursor-pointer focus:outline-none"
                  >
                    Ver detalle
                  </DropdownMenu.Item>
                  <DropdownMenu.Separator className="my-1 h-px bg-[#F7F8FA]" />
                  <DropdownMenu.Item
                    onSelect={() =>
                      toggleStatus.mutate({ id: client.id, active: !isActive })
                    }
                    className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-[#3A3F4B] hover:bg-[#F7F8FA] cursor-pointer focus:outline-none"
                  >
                    {isActive ? (
                      <><UserX className="h-3.5 w-3.5 text-[#B7791F]" /> Desactivar</>
                    ) : (
                      <><UserCheck className="h-3.5 w-3.5 text-[#0E9F6E]" /> Activar</>
                    )}
                  </DropdownMenu.Item>
                  <DropdownMenu.Item
                    onSelect={() => {
                      if (confirm(`¿Eliminar a "${client.name}"? Esta acción es irreversible.`)) {
                        deleteClient.mutate(client.id);
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

  const hasFilters = !!statusFilter || !!categoryFilter || !!tagFilter || !!debouncedSearch;

  return (
    <div>
      {/* Header */}
      <div className="mb-5 flex items-center justify-between gap-4">
        <div>
          <h1 className="font-display text-[22px] font-700 text-[#16181D]">Clientes</h1>
          <p className="mt-0.5 text-[13px] text-[#5B6472]">
            {data ? `${data.totalCount} registro${data.totalCount !== 1 ? "s" : ""}` : "Cargando…"}
          </p>
        </div>
        <Can permission="ManageClients">
          <Button onClick={() => setFormOpen(true)} size="md">
            <Plus className="h-4 w-4" />
            Nuevo
            <kbd className="ml-1 rounded bg-white/20 px-1 text-[10px]">N</kbd>
          </Button>
        </Can>
      </div>

      {/* Filtros */}
      <div className="mb-4 flex flex-wrap items-center gap-3">
        <div className="w-72">
          <Input
            ref={searchRef}
            placeholder="Buscar cliente…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            leftIcon={<Search className="h-3.5 w-3.5" />}
          />
        </div>
        <kbd className="rounded bg-[#E3E6EC] px-1.5 py-0.5 text-[10px] font-600 text-[#5B6472]">/</kbd>

        <FilterSelect
          label="Estado"
          value={statusFilter}
          onValueChange={(v) => setStatusFilter(v as ClientStatus | "")}
        >
          <FilterItem value="">Todos los estados</FilterItem>
          <FilterItem value="Active">Activo</FilterItem>
          <FilterItem value="Prospective">Prospecto</FilterItem>
          <FilterItem value="Inactive">Inactivo</FilterItem>
        </FilterSelect>

        {categories.length > 0 && (
          <FilterSelect
            label="Categoría"
            value={categoryFilter}
            onValueChange={setCategoryFilter}
          >
            <FilterItem value="">Todas las categorías</FilterItem>
            {categories.map((c) => (
              <FilterItem key={c.id} value={c.id}>{c.name}</FilterItem>
            ))}
          </FilterSelect>
        )}

        {allTags.length > 0 && (
          <FilterSelect
            label="Etiqueta"
            value={tagFilter}
            onValueChange={setTagFilter}
          >
            <FilterItem value="">Todas las etiquetas</FilterItem>
            {allTags.map((t) => (
              <FilterItem key={t.id} value={t.id}>
                <span className="flex items-center gap-1.5">
                  <span
                    className="inline-block h-2 w-2 rounded-full"
                    style={{ backgroundColor: t.color }}
                  />
                  {t.name}
                </span>
              </FilterItem>
            ))}
          </FilterSelect>
        )}

        {hasFilters && (
          <button
            onClick={() => {
              setSearch("");
              setStatusFilter("");
              setCategoryFilter("");
              setTagFilter("");
            }}
            className="text-[12px] text-[#5B6472] hover:text-[#0F5C6B] underline"
          >
            Limpiar filtros
          </button>
        )}
      </div>

      {/* Tabla */}
      <DataTable
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        emptyMessage={
          hasFilters
            ? "Sin resultados para los filtros actuales."
            : "Aún no hay clientes registrados."
        }
        onRowClick={(row) => router.push(`/clients/${row.id}`)}
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

      {/* Drawer alta rápida */}
      <ClientForm open={formOpen} onOpenChange={setFormOpen} />
    </div>
  );
}
