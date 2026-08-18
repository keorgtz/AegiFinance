"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import {
  useSubscription,
  useSubscriptionPriceHistory,
  useSubscriptionHistory,
  useSubscriptionPermissions,
  useSuspendSubscription,
  useReactivateSubscription,
  useCancelSubscription,
  useRenewSubscription,
  useDeleteSubscriptionPermission,
  useAddSubscriptionPermission,
} from "@/hooks/use-subscriptions";
import { Tabs, TabList, Tab, TabPanel } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Spinner } from "@/components/ui/spinner";
import { SubscriptionStatusBadge } from "@/components/modules/subscriptions/subscription-status-badge";
import { SubscriptionForm } from "@/components/modules/subscriptions/subscription-form";
import { ChangePriceForm } from "@/components/modules/subscriptions/change-price-form";
import {
  SubscriptionActionDialog,
  type ActionType,
} from "@/components/modules/subscriptions/subscription-action-dialog";
import { BillingTypeBadge } from "@/components/modules/services/billing-type-badge";
import { Can } from "@/lib/auth/can";
import { formatAmount, formatDate, formatDateTime } from "@/lib/utils/format";
import { type ColumnDef } from "@tanstack/react-table";
import { DataTable } from "@/components/ui/data-table";
import type {
  SubscriptionChangeLogDto,
  SubscriptionPermissionDto,
  SubscriptionPriceHistoryDto,
} from "@/types/api";
import {
  ArrowLeft,
  Layers,
  Pencil,
  Plus,
  RefreshCw,
  Trash2,
  TrendingDown,
  TrendingUp,
  XCircle,
} from "lucide-react";

export default function SubscriptionDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();

  const { data: sub, isLoading } = useSubscription(params.id);
  const { data: priceHistory = [] } = useSubscriptionPriceHistory(params.id);
  const { data: changeLogs = [] } = useSubscriptionHistory(params.id);
  const { data: permissions = [] } = useSubscriptionPermissions(params.id);

  const suspend = useSuspendSubscription();
  const reactivate = useReactivateSubscription();
  const cancel = useCancelSubscription();
  const renew = useRenewSubscription();
  const deletePermission = useDeleteSubscriptionPermission();
  const addPermission = useAddSubscriptionPermission();

  const [editOpen, setEditOpen] = useState(false);
  const [priceOpen, setPriceOpen] = useState(false);
  const [actionState, setActionState] = useState<ActionType | null>(null);

  if (isLoading) {
    return <div className="flex h-40 items-center justify-center"><Spinner className="text-action" /></div>;
  }

  if (!sub) {
    return (
      <div className="flex h-40 flex-col items-center justify-center gap-3">
        <p className="text-[14px] text-muted">Suscripción no encontrada.</p>
        <Button controlKey="ui.app.app.subscriptions.id.page.button.1" variant="ghost" onClick={() => router.back()}>
          <ArrowLeft className="h-4 w-4" /> Volver
        </Button>
      </div>
    );
  }

  const handleAction = async (reason?: string | null, effectiveDate?: string | null) => {
    const payload = { id: sub.id, data: { reason, effectiveDate } };
    if (actionState === "suspend") await suspend.mutateAsync(payload);
    else if (actionState === "reactivate") await reactivate.mutateAsync(payload);
    else if (actionState === "cancel") await cancel.mutateAsync(payload);
    else if (actionState === "renew") await renew.mutateAsync(payload);
  };

  const canSuspend = sub.status === "Active";
  const canReactivate = sub.status === "Suspended";
  const canRenew = sub.status === "Expired";
  const canCancel = sub.status !== "Cancelled" && sub.status !== "Expired";

  const priceColumns: ColumnDef<SubscriptionPriceHistoryDto, unknown>[] = [
    {
      accessorKey: "effectiveDate",
      header: "Fecha efectiva",
      cell: ({ getValue }) => <span className="text-[13px]">{formatDate(getValue() as string)}</span>,
    },
    {
      id: "change",
      header: "Cambio",
      cell: ({ row }) => {
        const up = row.original.newPrice > row.original.oldPrice;
        return (
          <span className={`flex items-center gap-1 text-[13px] font-semibold ${up ? "text-danger" : "text-success"}`}>
            {up ? <TrendingUp className="h-3.5 w-3.5" /> : <TrendingDown className="h-3.5 w-3.5" />}
            {formatAmount(row.original.oldPrice, sub.currency)} → {formatAmount(row.original.newPrice, sub.currency)}
          </span>
        );
      },
    },
    {
      accessorKey: "reason",
      header: "Motivo",
      cell: ({ getValue }) => <span className="text-[13px] text-muted">{(getValue() as string | null) ?? "—"}</span>,
    },
    {
      accessorKey: "createdAt",
      header: "Registrado",
      size: 130,
      cell: ({ getValue }) => <span className="text-[12px] text-muted">{formatDate(getValue() as string)}</span>,
    },
  ];

  const changeLogColumns: ColumnDef<SubscriptionChangeLogDto, unknown>[] = [
    {
      accessorKey: "createdAt",
      header: "Fecha",
      size: 130,
      cell: ({ getValue }) => <span className="text-[12px] text-muted">{formatDateTime(getValue() as string)}</span>,
    },
    {
      accessorKey: "changeType",
      header: "Tipo",
      size: 120,
      cell: ({ getValue }) => <Badge variant="muted">{getValue() as string}</Badge>,
    },
    {
      id: "values",
      header: "Detalle",
      cell: ({ row }) => {
        const { oldValue, newValue } = row.original;
        if (!oldValue && !newValue) return <span className="text-muted">—</span>;
        return (
          <span className="text-[13px] text-foreground-secondary">
            {oldValue && <span className="line-through text-muted mr-1">{oldValue}</span>}
            {newValue && <span>{newValue}</span>}
          </span>
        );
      },
    },
    {
      accessorKey: "reason",
      header: "Motivo",
      cell: ({ getValue }) => <span className="text-[13px] text-muted">{(getValue() as string | null) ?? "—"}</span>,
    },
  ];

  const permissionColumns: ColumnDef<SubscriptionPermissionDto, unknown>[] = [
    {
      accessorKey: "userName",
      header: "Usuario",
      cell: ({ getValue }) => <span className="font-semibold text-foreground">{getValue() as string}</span>,
    },
    {
      id: "remove",
      size: 80,
      cell: ({ row }) => (
        <Button controlKey="ui.app.app.subscriptions.id.page.button.2"
          variant="ghost"
          size="sm"
          onClick={() =>
            deletePermission.mutate({ subscriptionId: sub.id, permissionId: row.original.id })
          }
        >
          <Trash2 className="h-3.5 w-3.5 text-danger" />
        </Button>
      ),
    },
  ];

  return (
    <div>
      {/* Header */}
      <div className="mb-5">
        <button data-ui-control="ui.app.app.subscriptions.id.page.button.3" onClick={() => router.back()} className="mb-3 inline-flex items-center gap-1.5 text-[12px] text-muted hover:text-action transition-colors">
          <ArrowLeft className="h-3.5 w-3.5" /> Suscripciones
        </button>

        <div className="flex items-start justify-between gap-4">
          <div className="flex items-center gap-3 min-w-0">
            <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-card bg-canvas text-muted">
              <Layers className="h-6 w-6" />
            </div>
            <div className="min-w-0">
              <div className="flex items-center gap-2 flex-wrap">
                <h1 className="font-display text-[22px] font-bold text-foreground">{sub.serviceName}</h1>
                <SubscriptionStatusBadge status={sub.status} />
              </div>
              <div className="mt-0.5 flex items-center gap-2 flex-wrap text-[12px] text-muted">
                <span className="font-mono">{sub.code}</span>
                <span>·</span>
                <span>{sub.clientName}</span>
                <BillingTypeBadge billingType={sub.billingType} />
              </div>
            </div>
          </div>

          <div className="flex shrink-0 flex-wrap items-center gap-2">
            {canSuspend && (
              <Button controlKey="ui.app.app.subscriptions.id.page.button.4" variant="secondary" size="sm" onClick={() => setActionState("suspend")}>
                Suspender
              </Button>
            )}
            {canReactivate && (
              <Button controlKey="ui.app.app.subscriptions.id.page.button.5" variant="secondary" size="sm" onClick={() => setActionState("reactivate")}>
                <RefreshCw className="h-3.5 w-3.5" /> Reactivar
              </Button>
            )}
            {canRenew && (
              <Button controlKey="ui.app.app.subscriptions.id.page.button.6" variant="secondary" size="sm" onClick={() => setActionState("renew")}>
                <RefreshCw className="h-3.5 w-3.5" /> Renovar
              </Button>
            )}
            {canCancel && (
              <Button controlKey="ui.app.app.subscriptions.id.page.button.7" variant="danger" size="sm" onClick={() => setActionState("cancel")}>
                <XCircle className="h-3.5 w-3.5" /> Cancelar
              </Button>
            )}
            <Button controlKey="ui.app.app.subscriptions.id.page.button.8" variant="secondary" size="sm" onClick={() => setEditOpen(true)}>
              <Pencil className="h-3.5 w-3.5" /> Editar
            </Button>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <Tabs defaultTab="info">
        <TabList>
          <Tab controlKey="ui.app.app.subscriptions.id.page.tab.1" id="info">Información</Tab>
          <Tab controlKey="ui.app.app.subscriptions.id.page.tab.2" id="prices">
            Historial de precios
            {priceHistory.length > 0 && (
              <span className="ml-1.5 rounded-full bg-border px-1.5 py-0.5 text-[10px] font-bold text-muted">
                {priceHistory.length}
              </span>
            )}
          </Tab>
          <Tab controlKey="ui.app.app.subscriptions.id.page.tab.3" id="log">
            Cambios
            {changeLogs.length > 0 && (
              <span className="ml-1.5 rounded-full bg-border px-1.5 py-0.5 text-[10px] font-bold text-muted">
                {changeLogs.length}
              </span>
            )}
          </Tab>
          <Can permission="ManageUsers">
            <Tab controlKey="ui.app.app.subscriptions.id.page.tab.4" id="permissions">Visibilidad</Tab>
          </Can>
        </TabList>

        {/* ── INFO ── */}
        <TabPanel id="info">
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <InfoCard title="Precio y facturación">
              <InfoRow label="Precio actual">
                <span className="text-[20px] font-bold text-foreground">
                  {formatAmount(sub.price, sub.currency)}
                </span>
                <span className="ml-1 text-[12px] text-muted">{sub.currency}</span>
              </InfoRow>
              <InfoRow label="Tipo de facturación">
                <BillingTypeBadge billingType={sub.billingType} />
              </InfoRow>
              <InfoRow label="Día de corte">
                <span className="font-semibold text-foreground">Día {sub.billingDay}</span>
              </InfoRow>
              <InfoRow label="Renovación automática">
                <Badge variant={sub.autoRenew ? "jade" : "muted"}>{sub.autoRenew ? "Sí" : "No"}</Badge>
              </InfoRow>
              <div className="mt-3 border-t border-surface-subtle pt-3">
                <Button controlKey="ui.app.app.subscriptions.id.page.button.9" size="sm" onClick={() => setPriceOpen(true)}>
                  Cambiar precio
                </Button>
              </div>
            </InfoCard>

            <InfoCard title="Período">
              <InfoRow label="Inicio">{formatDate(sub.startDate)}</InfoRow>
              <InfoRow label="Fin">{sub.endDate ? formatDate(sub.endDate) : "Sin fecha de fin"}</InfoRow>
              <InfoRow label="Último cargo">
                {sub.lastBillingDate ? formatDate(sub.lastBillingDate) : "—"}
              </InfoRow>
              <InfoRow label="Próximo cargo">
                {sub.nextBillingDate ? formatDate(sub.nextBillingDate) : "—"}
              </InfoRow>
            </InfoCard>

            {sub.notes && (
              <div className="col-span-full">
                <InfoCard title="Notas">
                  <p className="whitespace-pre-wrap text-[13px] text-foreground-secondary">{sub.notes}</p>
                </InfoCard>
              </div>
            )}
          </div>
        </TabPanel>

        {/* ── PRICE HISTORY ── */}
        <TabPanel id="prices">
          <div className="mb-4 flex items-center justify-between">
            <p className="text-[13px] text-muted">
              {priceHistory.length === 0 ? "Sin cambios de precio." : `${priceHistory.length} registro${priceHistory.length !== 1 ? "s" : ""}`}
            </p>
            <Button controlKey="ui.app.app.subscriptions.id.page.button.10" size="sm" onClick={() => setPriceOpen(true)}>
              <Plus className="h-3.5 w-3.5" /> Registrar cambio
            </Button>
          </div>
          <DataTable
            columns={priceColumns}
            data={priceHistory}
            emptyMessage="Sin historial de precios."
            getRowId={(r) => r.id}
          />
        </TabPanel>

        {/* ── CHANGE LOG ── */}
        <TabPanel id="log">
          <DataTable
            columns={changeLogColumns}
            data={changeLogs}
            emptyMessage="Sin cambios registrados."
            getRowId={(r) => r.id}
          />
        </TabPanel>

        {/* ── PERMISSIONS ── */}
        <Can permission="ManageUsers">
          <TabPanel id="permissions">
            <div className="mb-4 flex items-center justify-between">
              <p className="text-[13px] text-muted">
                Usuarios con visibilidad sobre esta suscripción en el portal.
              </p>
            </div>
            <DataTable
              columns={permissionColumns}
              data={permissions}
              emptyMessage="Sin usuarios asignados."
              getRowId={(r) => r.id}
            />
          </TabPanel>
        </Can>
      </Tabs>

      {/* Modals */}
      <SubscriptionForm open={editOpen} onOpenChange={setEditOpen} editingSubscription={sub} />
      <ChangePriceForm
        open={priceOpen}
        onOpenChange={setPriceOpen}
        subscriptionId={sub.id}
        currentPrice={sub.price}
        currency={sub.currency}
      />
      {actionState && (
        <SubscriptionActionDialog
          open={!!actionState}
          onOpenChange={(v) => { if (!v) setActionState(null); }}
          action={actionState}
          onConfirm={handleAction}
        />
      )}
    </div>
  );
}

function InfoCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="rounded-card border border-border bg-surface p-4 shadow-dp1">
      <p className="mb-3 text-[11px] font-bold uppercase tracking-wider text-muted">{title}</p>
      <div className="space-y-2.5">{children}</div>
    </div>
  );
}

function InfoRow({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-4">
      <span className="text-[12px] text-muted shrink-0">{label}</span>
      <span className="text-[13px] text-foreground-secondary">{children}</span>
    </div>
  );
}
