"use client";

import { useEffect, useState } from "react";
import { type ColumnDef } from "@tanstack/react-table";
import {
  useBillingCycles,
  useBillingItems,
  useBillingLogs,
  useCloseBillingCycle,
  useReprocessBillingCycle,
} from "@/hooks/use-billing";
import { Tabs, TabList, Tab, TabPanel } from "@/components/ui/tabs";
import { DataTable } from "@/components/ui/data-table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Pagination } from "@/components/ui/pagination";
import { Select, SelectItem } from "@/components/ui/select";
import { BillingCycleStatusBadge } from "@/components/modules/billing/billing-cycle-status-badge";
import { BillingItemStatusBadge } from "@/components/modules/billing/billing-item-status-badge";
import { GenerateBillingDialog } from "@/components/modules/billing/generate-billing-dialog";
import { CancelItemDialog } from "@/components/modules/billing/cancel-item-dialog";
import { Can } from "@/lib/auth/can";
import { formatAmount, formatDate, formatDateTime } from "@/lib/utils/format";
import type {
  BillingCycleDto,
  BillingGenerationLogDto,
  BillingItemListDto,
  BillingItemStatus,
} from "@/types/api";
import {
  AlertCircle,
  CheckCircle2,
  Loader2,
  MoreHorizontal,
  Play,
  RefreshCw,
  XCircle,
} from "lucide-react";
import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import { cn } from "@/lib/utils/cn";

const PAGE_SIZE = 20;

const MONTHS = [
  "Enero","Febrero","Marzo","Abril","Mayo","Junio",
  "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre",
];

function cycleLabel(c: BillingCycleDto): string {
  if (c.month) return `${MONTHS[c.month - 1]} ${c.year}`;
  return `${c.year}`;
}

export default function BillingPage() {
  const now = new Date();
  const currentYear = now.getFullYear();

  // Items tab state
  const [itemsPage, setItemsPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<BillingItemStatus | "">("");
  const [cycleIdFilter, setCycleIdFilter] = useState("");
  const [dueDateFromFilter, setDueDateFromFilter] = useState("");
  const [dueDateToFilter, setDueDateToFilter] = useState("");
  const [clientIdFilter, setClientIdFilter] = useState("");
  const [currencyFilter, setCurrencyFilter] = useState("");
  const [outstandingOnly, setOutstandingOnly] = useState(false);
  const [cancelTarget, setCancelTarget] = useState<BillingItemListDto | null>(null);

  // Cycles tab state
  const [cyclesYear, setCyclesYear] = useState(String(currentYear));

  // Logs tab state
  const [logsYear, setLogsYear] = useState(String(currentYear));

  // Dialogs
  const [generateOpen, setGenerateOpen] = useState(false);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    setDueDateFromFilter(params.get("dueDateFrom") ?? "");
    setDueDateToFilter(params.get("dueDateTo") ?? "");
    setClientIdFilter(params.get("clientId") ?? "");
    setCurrencyFilter(params.get("currency") ?? "");
    setOutstandingOnly(params.get("outstandingOnly") === "true");
  }, []);

  // Data
  const { data: cycles = [], isLoading: cyclesLoading } = useBillingCycles({
    year: Number(cyclesYear),
  });
  const { data: itemsData, isLoading: itemsLoading } = useBillingItems({
    status: statusFilter || undefined,
    billingCycleId: cycleIdFilter || undefined,
    dueDateFrom: dueDateFromFilter || undefined,
    dueDateTo: dueDateToFilter || undefined,
    clientId: clientIdFilter || undefined,
    currency: currencyFilter || undefined,
    outstandingOnly,
    pageNumber: itemsPage,
    pageSize: PAGE_SIZE,
  });
  const { data: logs = [], isLoading: logsLoading } = useBillingLogs({
    year: Number(logsYear),
    limit: 100,
  });

  const closeCycle = useCloseBillingCycle();
  const reprocess = useReprocessBillingCycle();

  const years = Array.from({ length: 5 }, (_, i) => currentYear - 2 + i);

  /* ── Columns ── */

  const itemColumns: ColumnDef<BillingItemListDto, unknown>[] = [
    {
      id: "item",
      header: "Descripción",
      cell: ({ row }) => {
        const item = row.original;
        return (
          <div className="min-w-0">
            <p className="truncate font-medium text-foreground">{item.description}</p>
            <p className="text-[11px] text-muted">
              {item.clientName} · <span className="font-mono">{item.subscriptionCode}</span>
            </p>
          </div>
        );
      },
    },
    {
      accessorKey: "dueDate",
      header: "Vencimiento",
      size: 115,
      cell: ({ getValue }) => (
        <span className="text-[13px] text-muted">{formatDate(getValue() as string)}</span>
      ),
    },
    {
      accessorKey: "amount",
      header: "Importe",
      size: 120,
      cell: ({ row }) => (
        <span className="font-semibold text-foreground">
          {formatAmount(row.original.amount, row.original.currency)}
        </span>
      ),
    },
    {
      accessorKey: "paidAmount",
      header: "Abonado",
      size: 120,
      cell: ({ row }) => {
        const paid = row.original.paidAmount;
        return (
          <span className={cn("text-[13px]", paid > 0 ? "font-semibold text-success" : "text-muted")}>
            {paid > 0 ? formatAmount(paid, row.original.currency) : "—"}
          </span>
        );
      },
    },
    {
      accessorKey: "balance",
      header: "Saldo",
      size: 120,
      cell: ({ row }) => {
        const balance = row.original.balance;
        const cancelled = row.original.status === "Cancelled";
        return (
          <span className={cn("text-[13px] font-semibold", cancelled ? "text-muted line-through" : balance > 0 ? "text-danger" : "text-success")}>
            {formatAmount(balance, row.original.currency)}
          </span>
        );
      },
    },
    {
      accessorKey: "status",
      header: "Estado",
      size: 110,
      cell: ({ getValue }) => <BillingItemStatusBadge status={getValue() as BillingItemStatus} />,
    },
    {
      id: "actions",
      size: 48,
      cell: ({ row }) => {
        const item = row.original;
        const canCancel = item.status === "Pending" || item.status === "Partial";
        if (!canCancel) return null;
        return (
          <Can permission="ManageBilling">
            <Button controlKey="ui.app.app.billing.page.button.1"
              variant="ghost"
              size="icon"
              aria-label="Cancelar cargo"
              onClick={(e) => { e.stopPropagation(); setCancelTarget(item); }}
            >
              <XCircle className="h-4 w-4 text-danger" />
            </Button>
          </Can>
        );
      },
    },
  ];

  const cycleColumns: ColumnDef<BillingCycleDto, unknown>[] = [
    {
      id: "period",
      header: "Período",
      cell: ({ row }) => (
        <span className="font-semibold text-foreground">{cycleLabel(row.original)}</span>
      ),
    },
    {
      accessorKey: "status",
      header: "Estado",
      size: 130,
      cell: ({ getValue }) => <BillingCycleStatusBadge status={getValue() as string} />,
    },
    {
      id: "dates",
      header: "Rango",
      cell: ({ row }) => (
        <span className="text-[12px] text-muted">
          {formatDate(row.original.startDate)} – {formatDate(row.original.endDate)}
        </span>
      ),
    },
    {
      accessorKey: "itemsCount",
      header: "Cargos",
      size: 80,
      cell: ({ getValue }) => (
        <Badge variant="muted">{getValue() as number}</Badge>
      ),
    },
    {
      accessorKey: "closedAt",
      header: "Cerrado",
      size: 120,
      cell: ({ getValue }) => (
        <span className="text-[12px] text-muted">
          {(getValue() as string | null) ? formatDate(getValue() as string) : "—"}
        </span>
      ),
    },
    {
      id: "actions",
      size: 180,
      cell: ({ row }) => {
        const cycle = row.original;
        const isOpen = cycle.status === "Open";
        return (
          <Can permission="ManageBilling">
            <div className="flex items-center gap-1.5">
              {isOpen && (
                <Button controlKey="ui.app.app.billing.page.button.2"
                  size="sm"
                  variant="secondary"
                  onClick={(e) => {
                    e.stopPropagation();
                    if (confirm(`¿Cerrar el ciclo ${cycleLabel(cycle)}? No se podrán agregar más cargos.`)) {
                      closeCycle.mutate(cycle.id);
                    }
                  }}
                >
                  Cerrar
                </Button>
              )}
              <Button controlKey="ui.app.app.billing.page.button.3"
                size="sm"
                variant="ghost"
                onClick={(e) => {
                  e.stopPropagation();
                  reprocess.mutate({ cycleId: cycle.id });
                }}
              >
                <RefreshCw className="h-3.5 w-3.5" />
                Reprocesar
              </Button>
              <Button controlKey="ui.app.app.billing.page.button.4"
                size="sm"
                variant="ghost"
                onClick={(e) => {
                  e.stopPropagation();
                  setCycleIdFilter(cycle.id);
                }}
              >
                Ver cargos
              </Button>
            </div>
          </Can>
        );
      },
    },
  ];

  const logColumns: ColumnDef<BillingGenerationLogDto, unknown>[] = [
    {
      accessorKey: "startedAt",
      header: "Inicio",
      size: 140,
      cell: ({ getValue }) => (
        <span className="text-[12px] text-muted">{formatDateTime(getValue() as string)}</span>
      ),
    },
    {
      accessorKey: "billingCycleLabel",
      header: "Ciclo",
      cell: ({ getValue }) => (
        <span className="text-[13px] text-foreground-secondary">{(getValue() as string | null) ?? "—"}</span>
      ),
    },
    {
      accessorKey: "status",
      header: "Estado",
      size: 110,
      cell: ({ getValue }) => {
        const s = getValue() as string;
        const isOk = s === "Completed" || s === "Success";
        const isErr = s === "Failed" || s === "Error";
        return (
          <span className={cn("flex items-center gap-1 text-[12px] font-semibold", isOk ? "text-success" : isErr ? "text-danger" : "text-warning")}>
            {isOk ? <CheckCircle2 className="h-3.5 w-3.5" /> : isErr ? <AlertCircle className="h-3.5 w-3.5" /> : <Loader2 className="h-3.5 w-3.5 animate-spin" />}
            {s}
          </span>
        );
      },
    },
    {
      accessorKey: "itemsGenerated",
      header: "Cargos",
      size: 80,
      cell: ({ getValue }) => <Badge variant="muted">{getValue() as number}</Badge>,
    },
    {
      accessorKey: "finishedAt",
      header: "Fin",
      size: 140,
      cell: ({ getValue }) => (
        <span className="text-[12px] text-muted">
          {(getValue() as string | null) ? formatDateTime(getValue() as string) : "—"}
        </span>
      ),
    },
    {
      accessorKey: "errors",
      header: "Errores",
      cell: ({ getValue }) => {
        const e = getValue() as string | null;
        return e ? (
          <span className="truncate text-[12px] text-danger" title={e}>{e}</span>
        ) : <span className="text-muted">—</span>;
      },
    },
  ];

  const hasItemFilters = !!statusFilter || !!cycleIdFilter || !!dueDateFromFilter || !!dueDateToFilter || !!clientIdFilter || !!currencyFilter || outstandingOnly;

  return (
    <div>
      {/* Header */}
      <div className="mb-5 flex items-center justify-between gap-4">
        <div>
          <h1 className="font-display text-[22px] font-bold text-foreground">Facturación</h1>
          <p className="mt-0.5 text-[13px] text-muted">
            Motor de cargos y ciclos de cobro
          </p>
        </div>
        <Can permission="ManageBilling">
          <Button controlKey="ui.app.app.billing.page.button.5" onClick={() => setGenerateOpen(true)} size="md">
            <Play className="h-4 w-4" />
            Generar facturación
          </Button>
        </Can>
      </div>

      <Tabs defaultTab="items">
        <TabList>
          <Tab controlKey="ui.app.app.billing.page.tab.1" id="items">
            Cargos
            {itemsData && itemsData.totalCount > 0 && (
              <span className="ml-1.5 rounded-full bg-border px-1.5 py-0.5 text-[10px] font-bold text-muted">
                {itemsData.totalCount}
              </span>
            )}
          </Tab>
          <Tab controlKey="ui.app.app.billing.page.tab.2" id="cycles">
            Ciclos
            {cycles.length > 0 && (
              <span className="ml-1.5 rounded-full bg-border px-1.5 py-0.5 text-[10px] font-bold text-muted">
                {cycles.length}
              </span>
            )}
          </Tab>
          <Tab controlKey="ui.app.app.billing.page.tab.3" id="logs">Registro de generación</Tab>
        </TabList>

        {/* ── CARGOS ── */}
        <TabPanel id="items">
          <div className="mb-4 flex flex-wrap items-center gap-3">
            {outstandingOnly && <Badge variant="saffron">Sólo saldos pendientes</Badge>}
            {currencyFilter && <Badge variant="muted">Moneda: {currencyFilter}</Badge>}
            <Select controlKey="ui.app.app.billing.page.select.1"
              label=""
              value={statusFilter}
              onValueChange={(v) => { setStatusFilter(v as BillingItemStatus | ""); setItemsPage(1); }}
            >
              <SelectItem value="">Todos los estados</SelectItem>
              <SelectItem value="Pending">Pendiente</SelectItem>
              <SelectItem value="Partial">Parcial</SelectItem>
              <SelectItem value="Paid">Pagado</SelectItem>
              <SelectItem value="Cancelled">Cancelado</SelectItem>
            </Select>

            {cycles.length > 0 && (
              <Select controlKey="ui.app.app.billing.page.select.2"
                label=""
                value={cycleIdFilter}
                onValueChange={(v) => { setCycleIdFilter(v); setItemsPage(1); }}
              >
                <SelectItem value="">Todos los ciclos</SelectItem>
                {cycles.map((c) => (
                  <SelectItem key={c.id} value={c.id}>{cycleLabel(c)}</SelectItem>
                ))}
              </Select>
            )}

            <input data-ui-control="billing.filters.due-from" data-ui-permission="ViewPayments" type="date" value={dueDateFromFilter} onChange={(event) => { setDueDateFromFilter(event.target.value); setItemsPage(1); }} aria-label="Vencimiento desde" className="min-h-11 rounded-input border border-border-strong bg-field px-3 text-xs text-foreground" />
            <input data-ui-control="billing.filters.due-to" data-ui-permission="ViewPayments" type="date" value={dueDateToFilter} onChange={(event) => { setDueDateToFilter(event.target.value); setItemsPage(1); }} aria-label="Vencimiento hasta" className="min-h-11 rounded-input border border-border-strong bg-field px-3 text-xs text-foreground" />

            {hasItemFilters && (
              <button data-ui-control="ui.app.app.billing.page.button.6"
                onClick={() => { setStatusFilter(""); setCycleIdFilter(""); setDueDateFromFilter(""); setDueDateToFilter(""); setClientIdFilter(""); setCurrencyFilter(""); setOutstandingOnly(false); setItemsPage(1); }}
                className="text-[12px] text-muted hover:text-action underline"
              >
                Limpiar
              </button>
            )}
          </div>

          <DataTable
            columns={itemColumns}
            data={itemsData?.items ?? []}
            isLoading={itemsLoading}
            emptyMessage="Sin cargos para los filtros actuales."
            getRowId={(r) => r.id}
          />

          {itemsData && itemsData.totalCount > PAGE_SIZE && (
            <Pagination
              page={itemsPage}
              totalPages={itemsData.totalPages}
              totalCount={itemsData.totalCount}
              pageSize={PAGE_SIZE}
              onPageChange={setItemsPage}
            />
          )}
        </TabPanel>

        {/* ── CICLOS ── */}
        <TabPanel id="cycles">
          <div className="mb-4 flex items-center gap-3">
            <Select controlKey="ui.app.app.billing.page.select.3"
              label=""
              value={cyclesYear}
              onValueChange={setCyclesYear}
            >
              {years.map((y) => (
                <SelectItem key={y} value={String(y)}>{y}</SelectItem>
              ))}
            </Select>
          </div>

          <DataTable
            columns={cycleColumns}
            data={cycles}
            isLoading={cyclesLoading}
            emptyMessage="Sin ciclos para el año seleccionado."
            getRowId={(r) => r.id}
          />
        </TabPanel>

        {/* ── LOGS ── */}
        <TabPanel id="logs">
          <div className="mb-4 flex items-center gap-3">
            <Select controlKey="ui.app.app.billing.page.select.4"
              label=""
              value={logsYear}
              onValueChange={setLogsYear}
            >
              {years.map((y) => (
                <SelectItem key={y} value={String(y)}>{y}</SelectItem>
              ))}
            </Select>
          </div>

          <DataTable
            columns={logColumns}
            data={logs}
            isLoading={logsLoading}
            emptyMessage="Sin registros de generación."
            getRowId={(r) => r.id}
          />
        </TabPanel>
      </Tabs>

      {/* Dialogs */}
      <GenerateBillingDialog open={generateOpen} onOpenChange={setGenerateOpen} />

      {cancelTarget && (
        <CancelItemDialog
          open={!!cancelTarget}
          onOpenChange={(v) => { if (!v) setCancelTarget(null); }}
          itemId={cancelTarget.id}
          description={cancelTarget.description}
          amount={cancelTarget.amount}
          currency={cancelTarget.currency}
        />
      )}
    </div>
  );
}
