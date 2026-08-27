"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { type ColumnDef } from "@tanstack/react-table";
import {
  useSubscriptions,
  useDeleteSubscription,
  useSuspendSubscription,
  useReactivateSubscription,
  useCancelSubscription,
  useRenewSubscription,
} from "@/hooks/use-subscriptions";
import { DataTable } from "@/components/ui/data-table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Pagination } from "@/components/ui/pagination";
import { SubscriptionForm } from "@/components/modules/subscriptions/subscription-form";
import { SubscriptionStatusBadge } from "@/components/modules/subscriptions/subscription-status-badge";
import {
  SubscriptionActionDialog,
  type ActionType,
} from "@/components/modules/subscriptions/subscription-action-dialog";
import { BillingTypeBadge } from "@/components/modules/services/billing-type-badge";
import { formatAmount, formatDate } from "@/lib/utils/format";
import { Can } from "@/lib/auth/can";
import { usePermissions } from "@/lib/auth/use-permissions";
import type { SubscriptionListDto, SubscriptionStatus } from "@/types/api";
import {
  Layers,
  MoreHorizontal,
  Plus,
  RefreshCw,
  Trash2,
  XCircle,
} from "lucide-react";
import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import * as RadixSelect from "@radix-ui/react-select";
import { ChevronDown, Check } from "lucide-react";

const PAGE_SIZE = 15;

const STATUS_OPTIONS: { value: SubscriptionStatus | ""; label: string }[] = [
  { value: "", label: "Todos los estados" },
  { value: "Pending", label: "Pendiente" },
  { value: "Active", label: "Activa" },
  { value: "Suspended", label: "Suspendida" },
  { value: "Cancelled", label: "Cancelada" },
  { value: "Expired", label: "Vencida" },
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
      <RadixSelect.Trigger className="inline-flex h-8 items-center gap-1.5 rounded-input border border-border bg-surface px-3 text-[13px] text-foreground-secondary shadow-dp1 hover:border-action focus:outline-none">
        <RadixSelect.Value placeholder={label} />
        <ChevronDown className="h-3 w-3 text-muted" />
      </RadixSelect.Trigger>
      <RadixSelect.Portal>
        <RadixSelect.Content className="z-50 min-w-[180px] overflow-hidden rounded-table border border-border bg-surface shadow-dp2" position="popper" sideOffset={4}>
          <RadixSelect.Viewport className="p-1">{children}</RadixSelect.Viewport>
        </RadixSelect.Content>
      </RadixSelect.Portal>
    </RadixSelect.Root>
  );
}

function FilterItem({ value, children }: { value: string; children: React.ReactNode }) {
  return (
    <RadixSelect.Item value={value} className="flex cursor-pointer items-center justify-between rounded px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:bg-surface-subtle focus:outline-none data-[state=checked]:text-action data-[state=checked]:font-semibold">
      <RadixSelect.ItemText>{children}</RadixSelect.ItemText>
      <RadixSelect.ItemIndicator><Check className="h-3.5 w-3.5" /></RadixSelect.ItemIndicator>
    </RadixSelect.Item>
  );
}

export default function SubscriptionsPage() {
  const router = useRouter();
  const { can } = usePermissions();
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<SubscriptionStatus | "">("");
  const [formOpen, setFormOpen] = useState(false);
  const [actionTarget, setActionTarget] = useState<{ id: string; action: ActionType } | null>(null);

  const suspend = useSuspendSubscription();
  const reactivate = useReactivateSubscription();
  const cancel = useCancelSubscription();
  const renew = useRenewSubscription();
  const deleteSubscription = useDeleteSubscription();

  useEffect(() => { setPage(1); }, [statusFilter]);

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      const tag = (document.activeElement as HTMLElement)?.tagName ?? "";
      if (tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT") return;
      if ((e.key === "n" || e.key === "N") && can("ManageSubscriptions")) { e.preventDefault(); setFormOpen(true); }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, [can]);

  const { data, isLoading } = useSubscriptions({
    status: statusFilter || undefined,
    pageNumber: page,
    pageSize: PAGE_SIZE,
  });

  const handleAction = async (reason?: string | null, effectiveDate?: string | null) => {
    if (!actionTarget) return;
    const { id, action } = actionTarget;
    const payload = { id, data: { reason, effectiveDate } };
    if (action === "suspend") await suspend.mutateAsync(payload);
    else if (action === "reactivate") await reactivate.mutateAsync(payload);
    else if (action === "cancel") await cancel.mutateAsync(payload);
    else if (action === "renew") await renew.mutateAsync(payload);
  };

  const columns: ColumnDef<SubscriptionListDto, unknown>[] = [
    {
      id: "subscription",
      header: "Suscripción",
      cell: ({ row }) => {
        const s = row.original;
        return (
          <div className="flex items-center gap-2.5 min-w-0">
            <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-card bg-canvas text-muted">
              <Layers className="h-4 w-4" />
            </div>
            <div className="min-w-0">
              <p className="truncate font-semibold text-foreground">{s.serviceName}</p>
              <p className="truncate text-[11px] text-muted">{s.clientName}</p>
            </div>
          </div>
        );
      },
    },
    {
      accessorKey: "code",
      header: "Código",
      size: 110,
      cell: ({ getValue }) => (
        <span className="font-mono text-[12px] text-muted">{getValue() as string}</span>
      ),
    },
    {
      accessorKey: "status",
      header: "Estado",
      size: 110,
      cell: ({ getValue }) => <SubscriptionStatusBadge status={getValue() as SubscriptionStatus} />,
    },
    {
      accessorKey: "billingType",
      header: "Facturación",
      size: 120,
      cell: ({ getValue }) => <BillingTypeBadge billingType={getValue() as string} />,
    },
    {
      accessorKey: "price",
      header: "Precio",
      size: 130,
      cell: ({ row }) => (
        <span className="font-semibold text-foreground">
          {formatAmount(row.original.price, row.original.currency)}
        </span>
      ),
    },
    {
      accessorKey: "nextBillingDate",
      header: "Próx. cargo",
      size: 120,
      cell: ({ getValue }) => (
        <span className="text-[13px] text-muted">
          {(getValue() as string | null) ? formatDate(getValue() as string) : "—"}
        </span>
      ),
    },
    {
      accessorKey: "autoRenew",
      header: "Auto",
      size: 70,
      cell: ({ getValue }) => (
        <Badge variant={(getValue() as boolean) ? "jade" : "muted"}>
          {(getValue() as boolean) ? "Sí" : "No"}
        </Badge>
      ),
    },
    {
      id: "actions",
      size: 48,
      cell: ({ row }) => {
        const s = row.original;
        const canSuspend = s.status === "Active";
        const canReactivate = s.status === "Suspended";
        const canRenew = s.status === "Expired";
        const canCancel = s.status !== "Cancelled" && s.status !== "Expired";

        return (
          <DropdownMenu.Root>
            <DropdownMenu.Trigger asChild>
              <Button controlKey="ui.app.app.subscriptions.page.button.1" variant="ghost" size="icon" aria-label="Más acciones" onClick={(e) => e.stopPropagation()}>
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenu.Trigger>
            <DropdownMenu.Portal>
              <DropdownMenu.Content className="z-50 min-w-[170px] overflow-hidden rounded-table border border-border bg-surface shadow-dp2" align="end" sideOffset={4}>
                <DropdownMenu.Item onSelect={() => router.push(`/subscriptions/${s.id}`)} className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle cursor-pointer focus:outline-none">
                  Ver detalle
                </DropdownMenu.Item>
                <DropdownMenu.Separator className="my-1 h-px bg-surface-subtle" />
                {canSuspend && (
                  <DropdownMenu.Item onSelect={() => setActionTarget({ id: s.id, action: "suspend" })} className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-warning hover:bg-warning-soft cursor-pointer focus:outline-none">
                    Suspender
                  </DropdownMenu.Item>
                )}
                {canReactivate && (
                  <DropdownMenu.Item onSelect={() => setActionTarget({ id: s.id, action: "reactivate" })} className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-success hover:bg-success-soft cursor-pointer focus:outline-none">
                    <RefreshCw className="h-3.5 w-3.5" /> Reactivar
                  </DropdownMenu.Item>
                )}
                {canRenew && (
                  <DropdownMenu.Item onSelect={() => setActionTarget({ id: s.id, action: "renew" })} className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-action hover:bg-action-soft cursor-pointer focus:outline-none">
                    <RefreshCw className="h-3.5 w-3.5" /> Renovar
                  </DropdownMenu.Item>
                )}
                {canCancel && (
                  <DropdownMenu.Item onSelect={() => setActionTarget({ id: s.id, action: "cancel" })} className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-danger hover:bg-danger-soft cursor-pointer focus:outline-none">
                    <XCircle className="h-3.5 w-3.5" /> Cancelar
                  </DropdownMenu.Item>
                )}
                <DropdownMenu.Separator className="my-1 h-px bg-surface-subtle" />
                <DropdownMenu.Item
                  onSelect={() => {
                    if (confirm(`¿Eliminar la suscripción "${s.code}"?`)) {
                      deleteSubscription.mutate(s.id);
                    }
                  }}
                  className="flex items-center gap-2 px-3 py-1.5 text-[13px] text-danger hover:bg-danger-soft cursor-pointer focus:outline-none"
                >
                  <Trash2 className="h-3.5 w-3.5" /> Eliminar
                </DropdownMenu.Item>
              </DropdownMenu.Content>
            </DropdownMenu.Portal>
          </DropdownMenu.Root>
        );
      },
    },
  ];

  return (
    <div>
      <div className="mb-5 flex items-center justify-between gap-4">
        <div>
          <h1 className="font-display text-[22px] font-bold text-foreground">Suscripciones</h1>
          <p className="mt-0.5 text-[13px] text-muted">
            {data ? `${data.totalCount} suscripción${data.totalCount !== 1 ? "es" : ""}` : "Cargando…"}
          </p>
        </div>
        <Can permission="ViewSubscriptions">
          <Button controlKey="ui.app.app.subscriptions.page.button.2" onClick={() => setFormOpen(true)} size="md">
            <Plus className="h-4 w-4" />
            Nueva
            <kbd className="ml-1 rounded bg-surface/20 px-1 text-[10px]">N</kbd>
          </Button>
        </Can>
      </div>

      <div className="mb-4 flex flex-wrap items-center gap-3">
        <FilterSelect label="Estado" value={statusFilter} onValueChange={(v) => setStatusFilter(v as SubscriptionStatus | "")}>
          {STATUS_OPTIONS.map((o) => (
            <FilterItem key={o.value} value={o.value}>{o.label}</FilterItem>
          ))}
        </FilterSelect>
        {statusFilter && (
          <button data-ui-control="ui.app.app.subscriptions.page.button.3" onClick={() => setStatusFilter("")} className="text-[12px] text-muted hover:text-action underline">
            Limpiar
          </button>
        )}
      </div>

      <DataTable
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        emptyMessage="Sin suscripciones para los filtros actuales."
        onRowClick={(row) => router.push(`/subscriptions/${row.id}`)}
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

      <SubscriptionForm open={formOpen} onOpenChange={setFormOpen} />

      {actionTarget && (
        <SubscriptionActionDialog
          open={!!actionTarget}
          onOpenChange={(v) => { if (!v) setActionTarget(null); }}
          action={actionTarget.action}
          onConfirm={handleAction}
        />
      )}
    </div>
  );
}
