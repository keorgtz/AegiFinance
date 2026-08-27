"use client";

import { useEffect, useMemo, useState } from "react";
import { AlertCircle, ArrowDownLeft, ArrowUpRight, CheckCircle2, Download, FileSpreadsheet, FileText, HelpCircle, Mail, RefreshCw, ShieldCheck } from "lucide-react";
import { useAuth } from "@/lib/auth/context";
import { usePermissions } from "@/lib/auth/use-permissions";
import { Can } from "@/lib/auth/can";
import { useAccountStatementClients, useAccountStatementInquiries, useAccountStatementSubscriptions, useClientStatement, useCreateAccountStatementInquiry, useResolveAccountStatementInquiry } from "@/hooks/use-account-statements";
import { accountStatementsApi } from "@/lib/api/account-statements";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import { Input } from "@/components/ui/input";
import { Select, SelectItem } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Spinner } from "@/components/ui/spinner";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { formatAmount, formatDate, formatDateTime } from "@/lib/utils/format";
import { cn } from "@/lib/utils/cn";
import type { AccountStatementDto, AccountStatementInquiryDto, AccountStatementItemDto, GetClientStatementParams } from "@/types/api";
import { toast } from "sonner";

const currencies = ["MXN", "USD", "EUR"];
const typeLabels: Record<string, string> = { Charge: "Cargo", Payment: "Pago", Adjustment: "Ajuste", Reversal: "Reversa" };
const statusLabels: Record<AccountStatementInquiryDto["status"], string> = { Open: "Abierta", InReview: "En revisión", Resolved: "Resuelta", Closed: "Cerrada" };

export default function AccountStatementPage() {
  const { user } = useAuth();
  const { can, isClient } = usePermissions();
  const clientMode = isClient();
  const [selectedClientId, setSelectedClientId] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [currency, setCurrency] = useState("MXN");
  const [subscriptionId, setSubscriptionId] = useState("");
  const [inquiryMovement, setInquiryMovement] = useState<AccountStatementItemDto | null>(null);
  const [downloading, setDownloading] = useState<"pdf" | "csv" | null>(null);
  const clientId = clientMode ? user?.clientId ?? "" : selectedClientId;

  useEffect(() => { if (clientMode && user?.clientId) setSelectedClientId(user.clientId); }, [clientMode, user?.clientId]);
  useEffect(() => setSubscriptionId(""), [clientId]);

  const clients = useAccountStatementClients(!clientMode);
  const subscriptions = useAccountStatementSubscriptions(clientId);
  const params = useMemo<GetClientStatementParams>(() => ({ from: from || undefined, to: to || undefined, currency, subscriptionId: subscriptionId || undefined }), [from, to, currency, subscriptionId]);
  const statementQuery = useClientStatement(clientId, params);
  const inquiries = useAccountStatementInquiries(clientId, can("ViewAccountStatementInquiries"));
  const statement = statementQuery.data;
  const clientOptions = clients.data?.map(item => ({ value: item.id, label: item.name, description: item.code })) ?? [];

  async function download(kind: "pdf" | "csv") {
    if (!clientId) return;
    setDownloading(kind);
    try {
      const blob = kind === "pdf" ? await accountStatementsApi.exportPdf(clientId, params) : await accountStatementsApi.exportCsv(clientId, params);
      const url = URL.createObjectURL(blob); const anchor = document.createElement("a");
      anchor.href = url; anchor.download = `estado-cuenta-${statement?.clientName ?? clientId}.${kind}`; anchor.click(); URL.revokeObjectURL(url);
    } catch (error) { toast.error(error instanceof Error ? error.message : "No se pudo descargar el estado de cuenta."); }
    finally { setDownloading(null); }
  }

  return (
    <main className="mx-auto max-w-[1200px] space-y-6 pb-10">
      <header className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div><h1 className="font-display text-2xl font-bold text-foreground">Estado de cuenta</h1><p className="mt-1 text-sm text-muted">{clientMode ? "Tus cargos, pagos y aclaraciones en un solo lugar." : "Cuentas por cobrar verificadas contra el Major Ledger."}</p></div>
        {statement && <div className="flex flex-wrap gap-2">
          <Can permission="ExportAccountStatements"><Button controlKey="statements.export.csv" permission="ExportAccountStatements" variant="outline" loading={downloading === "csv"} onClick={() => download("csv")}><FileSpreadsheet className="h-4 w-4" />CSV</Button></Can>
          <Can permission="ExportAccountStatements"><Button controlKey="statements.export.pdf" permission="ExportAccountStatements" loading={downloading === "pdf"} onClick={() => download("pdf")}><Download className="h-4 w-4" />Descargar PDF</Button></Can>
        </div>}
      </header>

      <section aria-label="Filtros del estado de cuenta" className="grid gap-3 rounded-card border border-border bg-surface p-4 sm:grid-cols-2 lg:grid-cols-5">
        {!clientMode && <Combobox controlKey="statements.filters.client" permission="ViewAccountStatements" label="Cliente" value={clientId} onValueChange={setSelectedClientId} options={clientOptions} placeholder="Seleccionar cliente…" searchPlaceholder="Buscar cliente…" />}
        <Input controlKey="statements.filters.from" permission="ViewAccountStatements" label="Desde" type="date" value={from} onChange={event => setFrom(event.target.value)} />
        <Input controlKey="statements.filters.to" permission="ViewAccountStatements" label="Hasta" type="date" min={from || undefined} value={to} onChange={event => setTo(event.target.value)} />
        <Select controlKey="statements.filters.currency" permission="ViewAccountStatements" label="Moneda" value={currency} onValueChange={setCurrency}>{currencies.map(item => <SelectItem key={item} value={item}>{item}</SelectItem>)}</Select>
        <Select controlKey="statements.filters.subscription" permission="ViewAccountStatements" label="Plan" value={subscriptionId || "all"} onValueChange={value => setSubscriptionId(value === "all" ? "" : value)} disabled={!clientId}><SelectItem value="all">Todos los planes</SelectItem>{subscriptions.data?.map(item => <SelectItem key={item.id} value={item.id}>{item.code} · {item.serviceName}</SelectItem>)}</Select>
        <Button controlKey="statements.filters.clear" permission="ViewAccountStatements" variant="ghost" className="self-end" onClick={() => { setFrom(""); setTo(""); setCurrency("MXN"); setSubscriptionId(""); }}>Limpiar filtros</Button>
      </section>

      {!clientId && <StateCard icon={<FileText className="h-10 w-10" />} title="Seleccioná un cliente" detail="El estado se calculará directamente desde sus movimientos contables." />}
      {clientId && statementQuery.isLoading && <div className="grid min-h-64 place-items-center rounded-card border border-border bg-surface"><div className="text-center"><Spinner size="lg" /><p className="mt-3 text-sm text-muted">Verificando el Major Ledger…</p></div></div>}
      {clientId && statementQuery.isError && <StateCard tone="error" icon={<AlertCircle className="h-10 w-10" />} title="No pudimos generar el estado" detail={(statementQuery.error as Error).message} action={<Button controlKey="statements.error.retry" permission="ViewAccountStatements" variant="secondary" onClick={() => statementQuery.refetch()}><RefreshCw className="h-4 w-4" />Reintentar</Button>} />}
      {statement && !statementQuery.isLoading && <StatementContent statement={statement} fetching={statementQuery.isFetching} onInquiry={setInquiryMovement} onContact={() => statement.contactEmail && (window.location.href = `mailto:${statement.contactEmail}?subject=${encodeURIComponent(`Estado de cuenta ${statement.verificationCode}`)}`)} />}

      {clientId && can("ViewAccountStatementInquiries") && <InquiryList items={inquiries.data ?? []} loading={inquiries.isLoading} />}
      <InquiryDialog open={!!inquiryMovement} movement={inquiryMovement} statement={statement} clientId={clientId} onClose={() => setInquiryMovement(null)} />
    </main>
  );
}

function StatementContent({ statement, fetching, onInquiry, onContact }: { statement: AccountStatementDto; fetching: boolean; onInquiry: (item: AccountStatementItemDto) => void; onContact: () => void }) {
  return <div className={cn("space-y-6 transition-opacity", fetching && "pointer-events-none opacity-60")}>
    <section className="rounded-card border border-border bg-surface p-5">
      <div className="flex flex-col gap-4 sm:flex-row sm:justify-between"><div><div className="flex flex-wrap items-center gap-2"><h2 className="font-display text-lg font-bold text-foreground">{statement.clientName}</h2>{statement.subscriptionCode && <Badge variant="primary">{statement.subscriptionCode}</Badge>}{statement.isSubledgerView && <Badge variant="periwinkle">Subledger autorizado</Badge>}</div><p className="mt-1 text-sm text-muted">{periodLabel(statement)} · {statement.displayCurrency}</p><p className="mt-2 text-xs text-muted">{statement.scopeDescription}</p></div><div className="sm:text-right"><div className="inline-flex items-center gap-2 rounded-full bg-success-soft px-3 py-2 text-xs font-bold text-success"><ShieldCheck className="h-4 w-4" />Verificado {statement.verificationCode}</div><p className="mt-2 text-xs text-muted">Generado {formatDateTime(statement.statementDate)}</p></div></div>
    </section>

    <section aria-label="Resumen financiero" className="grid grid-cols-2 gap-3 lg:grid-cols-6">
      <Metric label="Saldo inicial" value={statement.initialBalance} currency={statement.displayCurrency} />
      <Metric label="Cargos" value={statement.totalCharges} currency={statement.displayCurrency} tone="danger" />
      <Metric label="Pagos" value={statement.totalPayments} currency={statement.displayCurrency} tone="success" />
      <Metric label="Ajustes netos" value={statement.totalAdjustments} currency={statement.displayCurrency} />
      <Metric label="Saldo final" value={statement.finalBalance} currency={statement.displayCurrency} emphasized />
      <Metric label="Vencido" value={statement.overdueBalance} currency={statement.displayCurrency} tone={statement.overdueBalance > 0 ? "warning" : "success"} />
    </section>

    {statement.formulaValid && <div className="flex items-start gap-3 rounded-input bg-success-soft p-3 text-sm text-success"><CheckCircle2 className="mt-0.5 h-4 w-4 flex-none" /><p><strong>Saldo reconciliado.</strong> Saldo inicial + cargos + ajustes − pagos = saldo final.</p></div>}

    <section className="overflow-hidden rounded-card border border-border bg-surface"><div className="border-b border-border p-4"><h2 className="font-display text-base font-bold text-foreground">Movimientos</h2><p className="mt-1 text-xs text-muted">Importes en {statement.displayCurrency}. Cada fila conserva su asiento y referencia.</p></div>
      {statement.items.length === 0 ? <StateCard icon={<FileText className="h-8 w-8" />} title="Sin movimientos" detail="No hay actividad para los filtros seleccionados." compact /> : <>
        <div className="hidden overflow-x-auto md:block"><table className="w-full min-w-[920px] text-left text-sm"><thead className="bg-surface-subtle text-xs text-muted"><tr><th className="p-3">Fecha</th><th className="p-3">Movimiento</th><th className="p-3">Plan</th><th className="p-3">Descripción</th><th className="p-3 text-right">Cargo</th><th className="p-3 text-right">Abono</th><th className="p-3 text-right">Saldo</th><th className="p-3"><span className="sr-only">Acciones</span></th></tr></thead><tbody className="divide-y divide-border">{statement.items.map(item => <MovementRow key={item.id} item={item} onInquiry={onInquiry} />)}</tbody></table></div>
        <div className="divide-y divide-border md:hidden">{statement.items.map(item => <MovementCard key={item.id} item={item} onInquiry={onInquiry} />)}</div>
      </>}
    </section>

    <section className="flex flex-col gap-3 rounded-card border border-border bg-surface-subtle p-4 sm:flex-row sm:items-center sm:justify-between"><div><h2 className="text-sm font-bold text-foreground">¿Necesitás ayuda?</h2><p className="mt-1 text-xs text-muted">Abrí una aclaración desde el movimiento o contactá a {statement.contactName ?? "tu responsable de cuenta"}.</p></div><Button controlKey="statements.contact.manager" permission="ViewAccountStatements" variant="secondary" disabled={!statement.contactEmail} onClick={onContact}><Mail className="h-4 w-4" />Contactar</Button></section>
  </div>;
}

function MovementRow({ item, onInquiry }: { item: AccountStatementItemDto; onInquiry: (item: AccountStatementItemDto) => void }) {
  return <tr><td className="p-3 text-muted">{formatDate(item.date)}<p className="mt-1 font-mono text-[10px]">{item.journalEntryNumber}</p></td><td className="p-3"><MovementBadge item={item} /></td><td className="p-3 text-xs text-muted">{item.subscriptionCode ?? "General"}<p>{item.serviceName}</p></td><td className="max-w-sm p-3 text-foreground"><p>{item.description}</p>{item.reference && <p className="mt-1 text-xs text-muted">Ref. {item.reference}</p>}</td><AmountCell value={item.debit} currency={item.currency} debit /><AmountCell value={item.credit} currency={item.currency} /><td className="p-3 text-right font-bold text-foreground">{formatAmount(item.balance, item.currency)}</td><td className="p-3"><Can permission="CreateAccountStatementInquiries"><Button controlKey="statements.movement.inquiry" permission="CreateAccountStatementInquiries" size="icon" variant="ghost" aria-label={`Solicitar aclaración de ${item.description}`} onClick={() => onInquiry(item)}><HelpCircle className="h-4 w-4" /></Button></Can></td></tr>;
}

function MovementCard({ item, onInquiry }: { item: AccountStatementItemDto; onInquiry: (item: AccountStatementItemDto) => void }) {
  return <article className="space-y-3 p-4"><div className="flex items-start justify-between gap-3"><div><MovementBadge item={item} /><p className="mt-2 font-semibold text-foreground">{item.description}</p><p className="mt-1 text-xs text-muted">{formatDate(item.date)} · {item.journalEntryNumber}</p></div><Can permission="CreateAccountStatementInquiries"><Button controlKey="statements.movement.inquiry.mobile" permission="CreateAccountStatementInquiries" size="icon" variant="ghost" aria-label={`Solicitar aclaración de ${item.description}`} onClick={() => onInquiry(item)}><HelpCircle className="h-4 w-4" /></Button></Can></div><div className="grid grid-cols-3 gap-2 rounded-input bg-surface-subtle p-3 text-xs"><MobileAmount label="Cargo" value={item.debit} item={item} /><MobileAmount label="Abono" value={item.credit} item={item} /><MobileAmount label="Saldo" value={item.balance} item={item} bold /></div>{item.subscriptionCode && <p className="text-xs text-muted">Plan {item.subscriptionCode} · {item.serviceName}</p>}</article>;
}

function MovementBadge({ item }: { item: AccountStatementItemDto }) { const variant = item.type === "Payment" ? "jade" : item.type === "Charge" ? "terracotta" : item.type === "Reversal" ? "saffron" : "periwinkle"; return <div className="flex flex-wrap gap-1"><Badge variant={variant}>{typeLabels[item.type] ?? item.type}</Badge>{item.isOverdue && <Badge variant="saffron">Vencido</Badge>}</div>; }
function AmountCell({ value, currency, debit = false }: { value: number; currency: string; debit?: boolean }) { return <td className={cn("p-3 text-right font-semibold", value ? debit ? "text-danger" : "text-success" : "text-muted")}>{value ? <span className="inline-flex items-center gap-1">{debit ? <ArrowUpRight className="h-3.5 w-3.5" /> : <ArrowDownLeft className="h-3.5 w-3.5" />}{formatAmount(value, currency)}</span> : "—"}</td>; }
function MobileAmount({ label, value, item, bold }: { label: string; value: number; item: AccountStatementItemDto; bold?: boolean }) { return <div><p className="text-muted">{label}</p><p className={cn("mt-1 text-foreground", bold && "font-bold")}>{formatAmount(value, item.currency)}</p></div>; }

function InquiryDialog({ open, movement, statement, clientId, onClose }: { open: boolean; movement: AccountStatementItemDto | null; statement?: AccountStatementDto; clientId: string; onClose: () => void }) {
  const create = useCreateAccountStatementInquiry(clientId); const [subject, setSubject] = useState(""); const [message, setMessage] = useState("");
  useEffect(() => { if (movement) { setSubject(`Aclaración sobre ${movement.journalEntryNumber}`); setMessage(""); } }, [movement]);
  const valid = subject.trim().length >= 3 && message.trim().length >= 10;
  return <Dialog open={open} onOpenChange={next => { if (!next) onClose(); }}><DialogContent title="Solicitar aclaración" description={movement ? `${formatDate(movement.date)} · ${movement.description}` : undefined}><Input controlKey="statements.inquiry.subject" permission="CreateAccountStatementInquiries" label="Asunto" value={subject} maxLength={200} onChange={event => setSubject(event.target.value)} /><Textarea controlKey="statements.inquiry.message" permission="CreateAccountStatementInquiries" label="¿Qué necesitás aclarar?" className="mt-4" value={message} maxLength={4000} hint="Incluí el dato que esperabas ver; no compartas contraseñas ni datos bancarios completos." onChange={event => setMessage(event.target.value)} /><DialogFooter><Button controlKey="statements.inquiry.cancel" systemRequired variant="secondary" onClick={onClose}>Cancelar</Button><Button controlKey="statements.inquiry.submit" permission="CreateAccountStatementInquiries" disabled={!valid || !movement} loading={create.isPending} onClick={async () => { if (!movement) return; await create.mutateAsync({ subscriptionId: movement.subscriptionId, journalEntryId: movement.journalEntryId === "00000000-0000-0000-0000-000000000000" ? null : movement.journalEntryId, statementVerificationCode: statement?.verificationCode, subject, message, idempotencyKey: crypto.randomUUID() }); onClose(); }}>Enviar aclaración</Button></DialogFooter></DialogContent></Dialog>;
}

function InquiryList({ items, loading }: { items: AccountStatementInquiryDto[]; loading: boolean }) {
  const [selected, setSelected] = useState<AccountStatementInquiryDto | null>(null);
  return <><section className="rounded-card border border-border bg-surface"><div className="border-b border-border p-4"><h2 className="font-display text-base font-bold text-foreground">Aclaraciones</h2><p className="mt-1 text-xs text-muted">Seguimiento de consultas enviadas desde tus movimientos.</p></div>{loading ? <div className="grid min-h-28 place-items-center"><Spinner /></div> : items.length === 0 ? <p className="p-5 text-sm text-muted">Todavía no hay aclaraciones.</p> : <div className="divide-y divide-border">{items.map(item => <article key={item.id} className="p-4"><div className="flex flex-wrap items-center justify-between gap-2"><p className="font-semibold text-foreground">{item.subject}</p><Badge variant={item.status === "Resolved" || item.status === "Closed" ? "jade" : item.status === "InReview" ? "periwinkle" : "saffron"}>{statusLabels[item.status]}</Badge></div><p className="mt-2 text-sm text-muted">{item.message}</p><p className="mt-2 text-xs text-muted">{formatDateTime(item.requestedAt)}{item.journalEntryNumber ? ` · ${item.journalEntryNumber}` : ""}</p>{item.resolution && <p className="mt-3 rounded-input bg-success-soft p-3 text-sm text-success"><strong>Respuesta:</strong> {item.resolution}</p>}{item.status !== "Resolved" && item.status !== "Closed" && <Can permission="ResolveAccountStatementInquiries"><Button controlKey="statements.inquiry.resolve.open" permission="ResolveAccountStatementInquiries" className="mt-3" variant="secondary" onClick={() => setSelected(item)}>Responder y resolver</Button></Can>}</article>)}</div>}</section><ResolveInquiryDialog item={selected} onClose={() => setSelected(null)} /></>;
}

function ResolveInquiryDialog({ item, onClose }: { item: AccountStatementInquiryDto | null; onClose: () => void }) { const [resolution, setResolution] = useState(""); const resolve = useResolveAccountStatementInquiry(item?.clientId ?? ""); useEffect(() => setResolution(""), [item]); return <Dialog open={!!item} onOpenChange={next => { if (!next) onClose(); }}><DialogContent title="Responder aclaración" description={item?.subject}><Textarea controlKey="statements.inquiry.resolve.message" permission="ResolveAccountStatementInquiries" label="Respuesta para el cliente" value={resolution} maxLength={4000} onChange={event => setResolution(event.target.value)} /><DialogFooter><Button controlKey="statements.inquiry.resolve.cancel" systemRequired variant="secondary" onClick={onClose}>Cancelar</Button><Button controlKey="statements.inquiry.resolve.submit" permission="ResolveAccountStatementInquiries" disabled={resolution.trim().length < 3} loading={resolve.isPending} onClick={async () => { if (!item) return; await resolve.mutateAsync({ id: item.id, resolution }); onClose(); }}>Publicar respuesta</Button></DialogFooter></DialogContent></Dialog>; }

function Metric({ label, value, currency, tone = "default", emphasized }: { label: string; value: number; currency: string; tone?: "default" | "success" | "danger" | "warning"; emphasized?: boolean }) { return <div className={cn("rounded-card border border-border bg-surface p-4", emphasized && "border-action bg-action-soft")}><p className="text-[11px] font-bold uppercase tracking-wider text-muted">{label}</p><p className={cn("mt-2 text-lg font-bold", tone === "success" ? "text-success" : tone === "danger" ? "text-danger" : tone === "warning" ? "text-warning" : "text-foreground")}>{formatAmount(value, currency)}</p></div>; }
function StateCard({ icon, title, detail, action, tone = "default", compact }: { icon: React.ReactNode; title: string; detail: string; action?: React.ReactNode; tone?: "default" | "error"; compact?: boolean }) { return <div className={cn("grid place-items-center text-center", compact ? "py-12" : "min-h-72 rounded-card border border-dashed border-border bg-surface", tone === "error" && "border-danger bg-danger-soft")}><div className={cn("max-w-md px-5", tone === "error" ? "text-danger" : "text-muted")}>{icon}<h2 className="mt-3 font-display text-base font-bold text-foreground">{title}</h2><p className="mt-1 text-sm">{detail}</p>{action && <div className="mt-4">{action}</div>}</div></div>; }
function periodLabel(statement: AccountStatementDto) { return `${statement.startDate ? formatDate(statement.startDate) : "Inicio"} — ${statement.endDate ? formatDate(statement.endDate) : "Hoy"}`; }
