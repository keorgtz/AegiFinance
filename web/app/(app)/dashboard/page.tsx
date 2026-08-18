"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  AlertCircle,
  ArrowRight,
  Building2,
  CalendarClock,
  CheckCircle2,
  CircleDollarSign,
  CloudOff,
  Landmark,
  ReceiptText,
  RefreshCw,
  ShieldAlert,
  SlidersHorizontal,
  WalletCards,
} from "lucide-react";
import { dashboardApi } from "@/lib/api/dashboard";
import { clientsApi } from "@/lib/api/clients";
import { bankAccountsApi } from "@/lib/api/bank-accounts";
import { currenciesApi } from "@/lib/api/currencies";
import { usePermissions } from "@/lib/auth/use-permissions";
import { Can } from "@/lib/auth/can";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectItem } from "@/components/ui/select";
import { cn } from "@/lib/utils/cn";
import { formatAmount, formatDate } from "@/lib/utils/format";
import type { DashboardFilters, DashboardMetricDto, DashboardTrendPointDto, LedgerEntryListDto } from "@/types/api";

const today = () => new Date().toISOString().slice(0, 10);
const daysAgo = (days: number) => {
  const date = new Date();
  date.setDate(date.getDate() - days);
  return date.toISOString().slice(0, 10);
};

export default function DashboardPage() {
  const { can, isClient } = usePermissions();
  const canViewClients = can("ViewClients") && !isClient();
  const canViewPayments = can("ViewPayments");
  const [from, setFrom] = useState(daysAgo(29));
  const [to, setTo] = useState(today());
  const [clientId, setClientId] = useState("");
  const [bankAccountId, setBankAccountId] = useState("");
  const [currency, setCurrency] = useState("MXN");
  const [online, setOnline] = useState(true);

  useEffect(() => {
    const update = () => setOnline(navigator.onLine);
    update();
    window.addEventListener("online", update);
    window.addEventListener("offline", update);
    return () => {
      window.removeEventListener("online", update);
      window.removeEventListener("offline", update);
    };
  }, []);

  const filters = useMemo<DashboardFilters>(() => ({
    from, to, clientId: clientId || undefined, bankAccountId: bankAccountId || undefined, currency,
  }), [from, to, clientId, bankAccountId, currency]);
  const summary = useQuery({ queryKey: ["dashboard", "summary", filters], queryFn: () => dashboardApi.summary(filters) });
  const attention = useQuery({ queryKey: ["dashboard", "attention", filters], queryFn: () => dashboardApi.attention(filters) });
  const activity = useQuery({ queryKey: ["dashboard", "activity", filters], queryFn: () => dashboardApi.activity(filters) });
  const clients = useQuery({
    queryKey: ["dashboard", "clients-filter"],
    queryFn: () => clientsApi.list({ status: "Active", pageNumber: 1, pageSize: 100 }),
    enabled: canViewClients,
  });
  const accounts = useQuery({
    queryKey: ["dashboard", "accounts-filter"],
    queryFn: () => bankAccountsApi.getAll({ isActive: true, pageNumber: 1, pageSize: 100 }),
    enabled: canViewPayments && !isClient(),
  });
  const currencies = useQuery({ queryKey: ["dashboard", "currencies"], queryFn: currenciesApi.list });

  const metricHref = (path: string, extra: Record<string, string | boolean> = {}) => {
    const query = new URLSearchParams({ currency });
    if (path === "/billing") {
      query.set("dueDateFrom", from);
      query.set("dueDateTo", to);
    }
    if (path === "/ledger") {
      query.set("dateFrom", from);
      query.set("dateTo", to);
    }
    if (clientId) query.set("clientId", clientId);
    if (bankAccountId) query.set("bankAccountId", bankAccountId);
    Object.entries(extra).forEach(([key, value]) => query.set(key, String(value)));
    return `${path}?${query.toString()}`;
  };

  const scopedAttentionHref = (href: string) => {
    const [path, originalQuery = ""] = href.split("?");
    const scoped = metricHref(path);
    const query = new URLSearchParams(scoped.split("?")[1]);
    new URLSearchParams(originalQuery).forEach((value, key) => query.set(key, value));
    return `${path}?${query.toString()}`;
  };

  return (
    <Can permission="ViewDashboard" fallback={<AccessDenied />}>
      <div className="space-y-6">
        {!online && (
          <div role="status" className="flex items-start gap-3 rounded-card border border-warning/30 bg-warning-soft p-4 text-warning">
            <CloudOff className="mt-0.5 h-5 w-5 flex-none" />
            <div><p className="text-sm font-bold">Sin conexión</p><p className="mt-0.5 text-xs">Mostramos la última información disponible; los datos se actualizarán al reconectar.</p></div>
          </div>
        )}

        <section className="relative overflow-hidden rounded-hero border border-border bg-gradient-to-br from-action-soft via-surface to-accent-soft p-5 sm:p-7">
          <div className="absolute -right-14 -top-20 h-52 w-52 rounded-full bg-action/10" />
          <div className="relative z-10 flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-2xl">
              <p className="text-xs font-bold uppercase tracking-[0.14em] text-action">Panel operativo</p>
              <h1 className="mt-3 text-2xl font-bold tracking-[-0.04em] text-foreground sm:text-3xl">
                {isClient() ? "Tus movimientos y cargos, en orden." : "Tu operación, lista para la siguiente decisión."}
              </h1>
              <p className="mt-2 max-w-xl text-sm leading-6 text-muted">Primero aparece lo que requiere atención; la tendencia y el historial quedan como segunda capa.</p>
            </div>
            <Can permission="ManageReconciliation">
              <Link href={metricHref("/ledger", { isReconciled: false })} data-ui-control="dashboard.reconciliation.open" data-ui-permission="ManageReconciliation" className="inline-flex min-h-12 items-center justify-center gap-2 rounded-full bg-action px-6 text-sm font-bold text-on-action shadow-dp2 transition-ui hover:bg-action-hover">
                Revisar conciliación <ArrowRight className="h-4 w-4" />
              </Link>
            </Can>
          </div>
        </section>

        <DashboardFiltersPanel
          from={from} to={to} currency={currency} clientId={clientId} bankAccountId={bankAccountId}
          setFrom={setFrom} setTo={setTo} setCurrency={setCurrency} setClientId={setClientId} setBankAccountId={setBankAccountId}
          clients={clients.data?.items ?? []} accounts={accounts.data?.items ?? []}
          currencies={currencies.data?.filter((item) => item.isActive) ?? []}
          showClients={canViewClients} showAccounts={canViewPayments && !isClient()}
        />

        <section aria-labelledby="dashboard-metrics-title">
          <div className="mb-3 flex flex-col gap-1 sm:flex-row sm:items-end sm:justify-between">
            <div><h2 id="dashboard-metrics-title" className="text-base font-bold text-foreground">Lectura rápida</h2><p className="text-xs text-muted">Cada cifra abre el mismo detalle y conserva tus filtros.</p></div>
            <span className="text-xs text-muted">{formatDate(from)} — {formatDate(to)}</span>
          </div>
          {summary.isError ? (
            <SectionError message="No pudimos cargar el resumen." retry={() => summary.refetch()} />
          ) : (
            <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
              <MetricCard label="Clientes activos" metric={summary.data?.activeClients} loading={summary.isLoading} icon={Building2} tone="info" href={metricHref("/clients", { status: "Active" })} />
              <MetricCard label="Cargos pendientes" metric={summary.data?.outstandingCharges} loading={summary.isLoading} icon={ReceiptText} tone="warning" href={metricHref("/billing", { outstandingOnly: true })} />
              <MetricCard label="Cargos vencidos" metric={summary.data?.overdueCharges} loading={summary.isLoading} icon={CalendarClock} tone="danger" href={metricHref("/billing", { outstandingOnly: true, dueDateTo: to < daysAgo(1) ? to : daysAgo(1) })} />
              <MetricCard label="Movimientos" metric={summary.data?.ledgerMovements} loading={summary.isLoading} icon={WalletCards} tone="success" href={metricHref("/ledger")} />
            </div>
          )}
        </section>

        <section aria-labelledby="attention-title">
          <div className="mb-3"><p className="text-xs font-bold uppercase tracking-[0.12em] text-warning">Prioridad</p><h2 id="attention-title" className="mt-1 text-lg font-bold text-foreground">Requiere atención</h2></div>
          {attention.isLoading ? <AttentionSkeleton /> : attention.isError ? (
            <SectionError message="La cola de atención no está disponible." retry={() => attention.refetch()} />
          ) : attention.data?.items.length ? (
            <div className="grid gap-3 lg:grid-cols-2">
              {attention.data.items.map((item) => (
                <Can key={item.kind} permission={item.permission}>
                  <Link href={scopedAttentionHref(item.href)} data-ui-control={`dashboard.attention.${item.kind}.open`} data-ui-permission={item.permission} className="group flex min-h-28 items-start gap-4 rounded-card border border-border bg-surface p-4 shadow-dp1 transition-ui hover:border-border-strong hover:shadow-dp2">
                    <span className={cn("grid h-11 w-11 flex-none place-items-center rounded-input", item.severity === "Risk" ? "bg-danger-soft text-danger" : "bg-warning-soft text-warning")}>
                      {item.severity === "Risk" ? <AlertCircle className="h-5 w-5" /> : <CircleDollarSign className="h-5 w-5" />}
                    </span>
                    <div className="min-w-0 flex-1"><div className="flex items-start justify-between gap-3"><h3 className="text-sm font-bold text-foreground">{item.title}</h3><span className="text-lg font-bold text-foreground">{item.count}</span></div><p className="mt-1 text-xs leading-5 text-muted">{item.detail}</p>{item.amount != null && <p className="mt-2 text-xs font-bold text-warning">{formatAmount(item.amount, item.currency)} por resolver</p>}</div>
                    <ArrowRight className="mt-3 h-4 w-4 flex-none text-muted transition-transform group-hover:translate-x-1" />
                  </Link>
                </Can>
              ))}
            </div>
          ) : (
            <div className="flex min-h-28 items-center gap-4 rounded-card border border-success/25 bg-success-soft p-5 text-success"><CheckCircle2 className="h-7 w-7 flex-none" /><div><p className="text-sm font-bold">Sin pendientes críticos</p><p className="mt-1 text-xs">No encontramos diferencias, importaciones fallidas ni pagos pendientes de aplicación para estos filtros.</p></div></div>
          )}
        </section>

        <div className="grid gap-5 xl:grid-cols-[1.15fr_0.85fr]">
          <TrendPanel loading={activity.isLoading} error={activity.isError} points={activity.data?.trend ?? []} summary={activity.data?.summary ?? ""} currency={currency} retry={() => activity.refetch()} />
          <RecentMovements loading={activity.isLoading} error={activity.isError} items={activity.data?.recentMovements ?? []} href={metricHref("/ledger")} retry={() => activity.refetch()} />
        </div>
      </div>
    </Can>
  );
}

type FiltersPanelProps = {
  from: string; to: string; currency: string; clientId: string; bankAccountId: string;
  setFrom: (value: string) => void; setTo: (value: string) => void; setCurrency: (value: string) => void;
  setClientId: (value: string) => void; setBankAccountId: (value: string) => void;
  clients: Array<{ id: string; name: string }>; accounts: Array<{ id: string; name: string }>;
  currencies: Array<{ code: string; name: string }>; showClients: boolean; showAccounts: boolean;
};

function DashboardFiltersPanel(props: FiltersPanelProps) {
  return (
    <section className="rounded-card border border-border bg-surface p-4 shadow-dp1" aria-label="Filtros del dashboard">
      <div className="mb-4 flex items-center gap-2 text-sm font-bold text-foreground"><SlidersHorizontal className="h-4 w-4 text-action" /> Alcance del panel</div>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
        <Input controlKey="dashboard.filters.from" permission="ViewDashboard" type="date" label="Desde" value={props.from} max={props.to} onChange={(event) => props.setFrom(event.target.value)} />
        <Input controlKey="dashboard.filters.to" permission="ViewDashboard" type="date" label="Hasta" value={props.to} min={props.from} onChange={(event) => props.setTo(event.target.value)} />
        {props.showClients && <Select controlKey="dashboard.filters.client" permission="ViewClients" label="Cliente" value={props.clientId || "all"} onValueChange={(value) => props.setClientId(value === "all" ? "" : value)}><SelectItem value="all">Todos los clientes</SelectItem>{props.clients.map((client) => <SelectItem key={client.id} value={client.id}>{client.name}</SelectItem>)}</Select>}
        {props.showAccounts && <Select controlKey="dashboard.filters.account" permission="ViewPayments" label="Cuenta" value={props.bankAccountId || "all"} onValueChange={(value) => props.setBankAccountId(value === "all" ? "" : value)}><SelectItem value="all">Todas las cuentas</SelectItem>{props.accounts.map((account) => <SelectItem key={account.id} value={account.id}>{account.name}</SelectItem>)}</Select>}
        <Select controlKey="dashboard.filters.currency" permission="ViewDashboard" label="Moneda" value={props.currency} onValueChange={props.setCurrency}><SelectItem value="MXN">MXN · Peso mexicano</SelectItem>{props.currencies.filter((item) => item.code !== "MXN").map((item) => <SelectItem key={item.code} value={item.code}>{item.code} · {item.name}</SelectItem>)}</Select>
      </div>
      <div className="mt-4 flex flex-wrap gap-2" aria-label="Periodos rápidos">
        {[7, 30, 90].map((days) => <Button key={days} controlKey={`dashboard.filters.period.${days}`} permission="ViewDashboard" variant="ghost" size="sm" onClick={() => { props.setFrom(daysAgo(days - 1)); props.setTo(today()); }}>{days} días</Button>)}
      </div>
    </section>
  );
}

const TONES = { info: "bg-info-soft text-info", warning: "bg-warning-soft text-warning", danger: "bg-danger-soft text-danger", success: "bg-success-soft text-success" };

function MetricCard({ label, metric, loading, icon: Icon, tone, href }: { label: string; metric?: DashboardMetricDto | null; loading: boolean; icon: React.ElementType; tone: keyof typeof TONES; href: string }) {
  if (loading) return <div className="h-40 animate-pulse rounded-card border border-border bg-surface-subtle" aria-label={`Cargando ${label}`} />;
  if (!metric) return <article className="rounded-card border border-dashed border-border bg-surface-subtle p-4"><div className="grid h-10 w-10 place-items-center rounded-input bg-surface-strong text-muted"><ShieldAlert className="h-5 w-5" /></div><p className="mt-4 text-xs font-semibold text-muted">{label}</p><p className="mt-1 text-sm font-bold text-muted">Sin acceso</p></article>;
  return (
    <Link href={href} data-ui-control={`dashboard.metric.${label.toLowerCase().replaceAll(" ", "-")}.open`} data-ui-permission="ViewDashboard" className="group block rounded-card border border-border bg-surface p-4 shadow-dp1 transition-ui hover:border-border-strong hover:shadow-dp2 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-action">
      <div className="flex items-start justify-between"><span className={cn("grid h-10 w-10 place-items-center rounded-input", TONES[tone])}><Icon className="h-5 w-5" /></span><ArrowRight className="h-4 w-4 text-muted transition-transform group-hover:translate-x-1" /></div>
      <p className="mt-4 text-xs font-semibold text-muted">{label}</p><p className="mt-1 text-2xl font-bold tracking-[-0.04em] text-foreground">{metric.count.toLocaleString("es-MX")}</p>
      {metric.amount != null && <p className="mt-1 text-xs font-semibold text-muted">{formatAmount(metric.amount, metric.currency)}</p>}
    </Link>
  );
}

function TrendPanel({ loading, error, points, summary, currency, retry }: { loading: boolean; error: boolean; points: DashboardTrendPointDto[]; summary: string; currency: string; retry: () => void }) {
  const max = Math.max(1, ...points.flatMap((point) => [point.charges, point.payments]));
  return (
    <section className="rounded-card border border-border bg-surface p-5 shadow-dp1" aria-labelledby="trend-title">
      <div><h2 id="trend-title" className="text-base font-bold text-foreground">Cargos y pagos</h2><p className="mt-1 text-xs text-muted">Importes del periodo · {currency}</p></div>
      {loading ? <div className="mt-5 h-52 animate-pulse rounded-table bg-surface-subtle" /> : error ? <SectionError message="No pudimos cargar la tendencia." retry={retry} /> : points.length === 0 ? <div className="mt-5 grid min-h-48 place-items-center text-center"><div><Landmark className="mx-auto h-7 w-7 text-action" /><p className="mt-3 text-sm font-bold text-foreground">Sin actividad en este periodo</p><p className="mt-1 text-xs text-muted">Probá ampliar las fechas o quitar filtros.</p></div></div> : (
        <><div className="mt-6 flex h-48 items-end gap-2" role="img" aria-label={`Gráfica de cargos y pagos. ${summary}`}>
          {points.map((point) => <div key={point.periodStart} className="flex min-w-0 flex-1 flex-col items-center gap-2"><div className="flex h-36 w-full items-end justify-center gap-1"><span className="w-[38%] min-w-2 rounded-t-sm bg-action" style={{ height: `${Math.max(3, point.charges / max * 100)}%` }} title={`Cargos: ${formatAmount(point.charges, currency)}`} /><span className="w-[38%] min-w-2 rounded-t-sm bg-success" style={{ height: `${Math.max(3, point.payments / max * 100)}%` }} title={`Pagos: ${formatAmount(point.payments, currency)}`} /></div><span className="truncate text-[10px] font-semibold text-muted">{point.label}</span></div>)}
        </div><div className="mt-3 flex gap-4 text-xs text-muted"><span className="flex items-center gap-2"><i className="h-2.5 w-2.5 rounded-full bg-action" />Cargos</span><span className="flex items-center gap-2"><i className="h-2.5 w-2.5 rounded-full bg-success" />Pagos</span></div><p className="mt-4 rounded-input bg-surface-subtle p-3 text-xs leading-5 text-foreground-secondary">{summary}</p></>
      )}
    </section>
  );
}

function RecentMovements({ loading, error, items, href, retry }: { loading: boolean; error: boolean; items: LedgerEntryListDto[]; href: string; retry: () => void }) {
  return (
    <section className="overflow-hidden rounded-card border border-border bg-surface shadow-dp1" aria-labelledby="recent-ledger-title">
      <div className="flex items-start justify-between gap-3 border-b border-border p-5"><div><h2 id="recent-ledger-title" className="text-base font-bold text-foreground">Movimientos recientes</h2><p className="mt-1 text-xs text-muted">Últimas entradas dentro del periodo.</p></div><Link data-ui-control="dashboard.movements.all" data-ui-permission="ViewPayments" href={href} className="inline-flex min-h-11 items-center text-xs font-bold text-action hover:underline">Ver todos</Link></div>
      {loading ? <div className="space-y-3 p-5">{[1,2,3].map((item) => <div key={item} className="h-14 animate-pulse rounded-input bg-surface-subtle" />)}</div> : error ? <SectionError message="No pudimos consultar el ledger." retry={retry} /> : items.length === 0 ? <div className="grid min-h-56 place-items-center p-8 text-center"><div><Landmark className="mx-auto h-7 w-7 text-action" /><p className="mt-3 text-sm font-bold text-foreground">Todavía no hay movimientos</p><p className="mt-1 text-xs text-muted">El historial aparecerá cuando exista actividad con estos filtros.</p></div></div> : <ul className="divide-y divide-border">{items.map((entry) => <li key={entry.id} className="flex min-h-16 items-center gap-3 px-5 py-3"><span className={cn("grid h-9 w-9 flex-none place-items-center rounded-full", entry.isReconciled ? "bg-success-soft text-success" : "bg-warning-soft text-warning")}>{entry.isReconciled ? <CheckCircle2 className="h-4 w-4" /> : <CircleDollarSign className="h-4 w-4" />}</span><div className="min-w-0 flex-1"><p className="truncate text-sm font-semibold text-foreground">{entry.description}</p><p className="truncate text-xs text-muted">{entry.bankAccountName} · {formatDate(entry.date)}</p></div><div className="text-right"><p className="text-sm font-bold text-foreground">{formatAmount(entry.amount, entry.currency)}</p><p className={cn("text-[10px] font-bold", entry.isReconciled ? "text-success" : "text-warning")}>{entry.isReconciled ? "Conciliado" : "Pendiente"}</p></div></li>)}</ul>}
    </section>
  );
}

function SectionError({ message, retry }: { message: string; retry: () => void }) { return <div className="flex min-h-28 flex-col items-center justify-center rounded-card border border-danger/25 bg-danger-soft p-5 text-center text-danger"><AlertCircle className="h-6 w-6" /><p className="mt-2 text-sm font-bold">{message}</p><Button controlKey="dashboard.error.retry" permission="ViewDashboard" variant="ghost" size="sm" className="mt-2" onClick={retry}><RefreshCw className="h-4 w-4" />Reintentar</Button></div>; }
function AttentionSkeleton() { return <div className="grid gap-3 lg:grid-cols-2">{[1,2].map((item) => <div key={item} className="h-28 animate-pulse rounded-card border border-border bg-surface-subtle" />)}</div>; }
function AccessDenied() { return <div className="grid min-h-[55vh] place-items-center"><div className="max-w-md text-center"><span className="mx-auto grid h-14 w-14 place-items-center rounded-card bg-warning-soft text-warning"><ShieldAlert className="h-6 w-6" /></span><h1 className="mt-4 text-xl font-bold text-foreground">Resumen no disponible</h1><p className="mt-2 text-sm text-muted">Tu rol no tiene permiso para consultar este espacio.</p></div></div>; }
