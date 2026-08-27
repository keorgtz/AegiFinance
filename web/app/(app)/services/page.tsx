"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { type ColumnDef } from "@tanstack/react-table";
import { useServices, useDeleteService, useToggleServiceStatus } from "@/hooks/use-services";
import { useServiceCategories } from "@/hooks/use-service-categories";
import { DataTable } from "@/components/ui/data-table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Pagination } from "@/components/ui/pagination";
import { ServiceForm } from "@/components/modules/services/service-form";
import { BillingTypeBadge } from "@/components/modules/services/billing-type-badge";
import { formatAmount, getBillingTypeLabel } from "@/lib/utils/format";
import type { BillingType, ServiceListDto } from "@/types/api";
import {
  MoreHorizontal,
  Package,
  Plus,
  Search,
  Trash2,
  ToggleLeft,
  ToggleRight,
} from "lucide-react";
import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import * as RadixSelect from "@radix-ui/react-select";
import { ChevronDown, Check } from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { usePermissions } from "@/lib/auth/use-permissions";

const PAGE_SIZE = 15;

const BILLING_TYPES: { value: BillingType; label: string }[] = [
  { value: "Monthly", label: "Mensual" },
  { value: "Yearly", label: "Anual" },
  { value: "OneTime", label: "Único" },
  { value: "Hourly", label: "Por hora" },
  { value: "Custom", label: "Personalizado" },
];

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
      <RadixSelect.Trigger className="inline-flex h-8 items-center gap-1.5 rounded-input border border-border bg-surface px-3 text-[13px] text-foreground-secondary shadow-dp1 hover:border-action focus:outline-none focus-visible:ring-2 focus-visible:ring-action">
        <RadixSelect.Value placeholder={label} />
        <ChevronDown className="h-3 w-3 text-muted" />
      </RadixSelect.Trigger>
      <RadixSelect.Portal>
        <RadixSelect.Content
          className="z-50 min-w-[180px] overflow-hidden rounded-table border border-border bg-surface shadow-dp2"
          position="popper"
          sideOffset={4}
        >
          <RadixSelect.Viewport className="p-1">{children}</RadixSelect.Viewport>
        </RadixSelect.Content>
      </RadixSelect.Portal>
    </RadixSelect.Root>
  );
}

function FilterItem({ value, children }: { value: string; children: React.ReactNode }) {
  return (
    <RadixSelect.Item
      value={value}
      className="flex cursor-pointer items-center justify-between rounded px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:bg-surface-subtle focus:outline-none data-[state=checked]:text-action data-[state=checked]:font-semibold"
    >
      <RadixSelect.ItemText>{children}</RadixSelect.ItemText>
      <RadixSelect.ItemIndicator>
        <Check className="h-3.5 w-3.5" />
      </RadixSelect.ItemIndicator>
    </RadixSelect.Item>
  );
}

export default function ServicesPage() {
  const router = useRouter();
  const { can } = usePermissions();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [billingTypeFilter, setBillingTypeFilter] = useState<BillingType | "">("");
  const [activeFilter, setActiveFilter] = useState<"" | "true" | "false">("");
  const [formOpen, setFormOpen] = useState(false);
  const searchRef = useRef<HTMLInputElement>(null);

  const { data: categories = [] } = useServiceCategories();

  useEffect(() => {
    const t = setTimeout(() => { setDebouncedSearch(search); setPage(1); }, 350);
    return () => clearTimeout(t);
  }, [search]);

  useEffect(() => { setPage(1); }, [categoryFilter, billingTypeFilter, activeFilter]);

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      const tag = (document.activeElement as HTMLElement)?.tagName ?? "";
      const isInput = tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT";
      if (isInput) return;
      if ((e.key === "n" || e.key === "N") && can("ManageServices")) { e.preventDefault(); setFormOpen(true); }
      if (e.key === "/") { e.preventDefault(); searchRef.current?.focus(); }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, [can]);

  const { data, isLoading } = useServices({
    searchTerm: debouncedSearch || undefined,
    categoryId: categoryFilter || undefined,
    billingType: billingTypeFilter || undefined,
    isActive: activeFilter === "" ? undefined : activeFilter === "true",
    pageNumber: page,
    pageSize: PAGE_SIZE,
  });

  const deleteService = useDeleteService();
  const toggleStatus = useToggleServiceStatus();

  const columns: ColumnDef<ServiceListDto, unknown>[] = [
    {
      id: "service",
      header: "Servicio",
      cell: ({ row }) => {
        const s = row.original;
        return (
          <div className="flex items-center gap-2.5 min-w-0">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-card bg-action-soft text-action">
              <Package className="h-4 w-4" />
            </div>
            <div className="min-w-0">
              <p className="truncate font-semibold text-foreground">{s.name}</p>
              {s.categoryName && (
                <p className="truncate text-[11px] text-muted">{s.categoryName}</p>
              )}
            </div>
          </div>
        );
      },
    },
    {
      accessorKey: "code",
      header: "Código",
      size: 100,
      cell: ({ getValue }) => (
        <span className="font-mono text-[12px] text-muted">{getValue() as string}</span>
      ),
    },
    {
      accessorKey: "billingType",
      header: "Facturación",
      size: 130,
      cell: ({ getValue }) => <BillingTypeBadge billingType={getValue() as string} />,
    },
    {
      accessorKey: "defaultPrice",
      header: "Precio base",
      size: 130,
      cell: ({ row }) => (
        <span className="font-semibold text-foreground">
          {formatAmount(row.original.defaultPrice, row.original.currency)}
        </span>
      ),
    },
    {
      accessorKey: "isActive",
      header: "Estado",
      size: 100,
      cell: ({ getValue }) => (
        <Badge variant={(getValue() as boolean) ? "jade" : "terracotta"}>
          {(getValue() as boolean) ? "Activo" : "Inactivo"}
        </Badge>
      ),
    },
    {
      accessorKey: "isPublic",
      header: "Portal",
      size: 80,
      cell: ({ getValue }) => (
        <Badge variant={(getValue() as boolean) ? "periwinkle" : "muted"}>
          {(getValue() as boolean) ? "Visible" : "Oculto"}
        </Badge>
      ),
    },
    {
      id: "actions",
      size: 48,
      cell: ({ row }) => {
        const svc = row.original;
        return (
          <DropdownMenu.Root>
            <DropdownMenu.Trigger asChild>
              <Button controlKey="ui.app.app.services.page.button.1"
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
                  onSelect={() => router.push(`/services/${svc.id}`)}
                  className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle cursor-pointer focus:outline-none"
                >
                  Ver detalle
                </DropdownMenu.Item>
                <DropdownMenu.Separator className="my-1 h-px bg-surface-subtle" />
                <DropdownMenu.Item
                  onSelect={() => toggleStatus.mutate({ id: svc.id, active: !svc.isActive })}
                  className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle cursor-pointer focus:outline-none"
                >
                  {svc.isActive ? (
                    <><ToggleLeft className="h-3.5 w-3.5 text-warning" /> Desactivar</>
                  ) : (
                    <><ToggleRight className="h-3.5 w-3.5 text-success" /> Activar</>
                  )}
                </DropdownMenu.Item>
                <DropdownMenu.Item
                  onSelect={() => {
                    if (confirm(`¿Eliminar el servicio "${svc.name}"? Esta acción es irreversible.`)) {
                      deleteService.mutate(svc.id);
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
        );
      },
    },
  ];

  const hasFilters =
    !!debouncedSearch || !!categoryFilter || !!billingTypeFilter || !!activeFilter;

  return (
    <div>
      {/* Header */}
      <div className="mb-5 flex items-center justify-between gap-4">
        <div>
          <h1 className="font-display text-[22px] font-bold text-foreground">
            Catálogo de servicios
          </h1>
          <p className="mt-0.5 text-[13px] text-muted">
            {data
              ? `${data.totalCount} servicio${data.totalCount !== 1 ? "s" : ""}`
              : "Cargando…"}
          </p>
        </div>
        <Button controlKey="ui.app.app.services.page.button.2" onClick={() => setFormOpen(true)} size="md">
          <Plus className="h-4 w-4" />
          Nuevo
          <kbd className="ml-1 rounded bg-surface/20 px-1 text-[10px]">N</kbd>
        </Button>
      </div>

      {/* Filtros */}
      <div className="mb-4 flex flex-wrap items-center gap-3">
        <div className="w-72">
          <Input controlKey="ui.app.app.services.page.input.1"
            ref={searchRef}
            placeholder="Buscar servicio…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            leftIcon={<Search className="h-3.5 w-3.5" />}
          />
        </div>
        <kbd className="rounded bg-border px-1.5 py-0.5 text-[10px] font-semibold text-muted">/</kbd>

        <FilterSelect
          label="Estado"
          value={activeFilter}
          onValueChange={(v) => setActiveFilter(v as "" | "true" | "false")}
        >
          <FilterItem value="">Todos</FilterItem>
          <FilterItem value="true">Activos</FilterItem>
          <FilterItem value="false">Inactivos</FilterItem>
        </FilterSelect>

        <FilterSelect
          label="Facturación"
          value={billingTypeFilter}
          onValueChange={(v) => setBillingTypeFilter(v as BillingType | "")}
        >
          <FilterItem value="">Todos los tipos</FilterItem>
          {BILLING_TYPES.map((bt) => (
            <FilterItem key={bt.value} value={bt.value}>{bt.label}</FilterItem>
          ))}
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

        {hasFilters && (
          <button data-ui-control="ui.app.app.services.page.button.3"
            onClick={() => {
              setSearch("");
              setCategoryFilter("");
              setBillingTypeFilter("");
              setActiveFilter("");
            }}
            className="text-[12px] text-muted hover:text-action underline"
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
            : "Aún no hay servicios en el catálogo."
        }
        onRowClick={(row) => router.push(`/services/${row.id}`)}
        getRowId={(row) => row.id}
      />

      {data && data.totalCount > PAGE_SIZE && (
        <Pagination
          page={page}
          totalPages={data.totalPages}
          totalCount={data.totalCount}
          pageSize={PAGE_SIZE}
          onPageChange={setPage}
        />
      )}

      <ServiceForm open={formOpen} onOpenChange={setFormOpen} />
    </div>
  );
}
