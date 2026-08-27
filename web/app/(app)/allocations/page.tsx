"use client";

import { useState } from "react";
import { Bot, CheckCircle2, FileText, Link2, Plus, RefreshCw, RotateCcw, Settings2, SplitSquareVertical, WalletCards, X } from "lucide-react";
import { useClients } from "@/hooks/use-clients";
import { useLedgerEntries } from "@/hooks/use-ledger";
import { useBillingItems } from "@/hooks/use-billing";
import { useServices } from "@/hooks/use-services";
import { useApplyPaymentsAutomatically, useApplyPaymentsManually, usePaymentApplications, usePaymentApplicationSettings, useReapplyPaymentApplication, useReversePaymentApplication, useUpdatePaymentApplicationSettings } from "@/hooks/use-allocations";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Combobox } from "@/components/ui/combobox";
import { Input } from "@/components/ui/input";
import { Select, SelectItem } from "@/components/ui/select";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Can } from "@/lib/auth/can";
import { formatAmount, formatDate, formatDateTime } from "@/lib/utils/format";
import type { PaymentAllocationLineRequest, PaymentApplicationDto, PaymentApplicationPriority, PaymentApplicationStatus } from "@/types/api";

const priorities: Array<{ value: PaymentApplicationPriority; label: string; detail: string }> = [
  { value: "DueDate", label: "Vencimiento", detail: "Primero los cargos más antiguos" },
  { value: "Plan", label: "Plan", detail: "Prioriza el plan seleccionado" },
  { value: "Reference", label: "Referencia", detail: "Busca referencia, suscripción o concepto" },
];
const priorityLabel: Record<PaymentApplicationPriority, string> = { DueDate: "Vencimiento", Plan: "Plan", Reference: "Referencia", Selection: "Selección manual" };

export default function PaymentApplicationsPage() {
  const [clientId, setClientId] = useState("");
  const [status, setStatus] = useState<PaymentApplicationStatus | "">("");
  const [autoOpen, setAutoOpen] = useState(false);
  const [manualOpen, setManualOpen] = useState(false);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [receipt, setReceipt] = useState<PaymentApplicationDto | null>(null);
  const [reverseTarget, setReverseTarget] = useState<PaymentApplicationDto | null>(null);
  const [reverseReason, setReverseReason] = useState("");
  const clients = useClients({ pageSize: 200 });
  const applications = usePaymentApplications(clientId, status || undefined);
  const reverse = useReversePaymentApplication();
  const reapply = useReapplyPaymentApplication();
  const clientOptions = clients.data?.items.map((client) => ({ value: client.id, label: client.name, description: client.code })) ?? [];

  const performReverse = async () => {
    if (!reverseTarget || !reverseReason.trim()) return;
    await reverse.mutateAsync({ id: reverseTarget.id, reason: reverseReason.trim() });
    setReverseTarget(null); setReverseReason("");
  };

  return <div className="space-y-6">
    <header className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
      <div><h1 className="font-display text-[22px] font-bold text-foreground">Aplicación de pagos</h1><p className="mt-1 text-sm text-muted">Distribuí pagos con límites verificables, recibo y asiento asociado.</p></div>
      <div className="flex flex-wrap gap-2">
        <Can permission="ManagePaymentApplicationSettings"><Button controlKey="payments.settings.open" permission="ManagePaymentApplicationSettings" variant="outline" onClick={() => setSettingsOpen(true)}><Settings2 className="h-4 w-4" />Prioridad</Button></Can>
        <Can permission="ApplyPayments"><Button controlKey="payments.auto.open" permission="ApplyPayments" variant="secondary" disabled={!clientId} onClick={() => setAutoOpen(true)}><Bot className="h-4 w-4" />Aplicación automática</Button></Can>
        <Can permission="ApplyPayments"><Button controlKey="payments.manual.open" permission="ApplyPayments" disabled={!clientId} onClick={() => setManualOpen(true)}><SplitSquareVertical className="h-4 w-4" />Aplicar pagos</Button></Can>
      </div>
    </header>

    <section className="grid gap-3 rounded-card border border-border bg-surface p-4 shadow-dp1 sm:grid-cols-[minmax(0,1fr)_220px]">
      <Combobox controlKey="payments.filters.client" permission="ViewPaymentApplications" label="Cliente" value={clientId} onValueChange={setClientId} options={clientOptions} placeholder="Seleccioná un cliente…" searchPlaceholder="Buscar cliente…" />
      <Select controlKey="payments.filters.status" permission="ViewPaymentApplications" label="Estado" value={status || "all"} onValueChange={(value) => setStatus(value === "all" ? "" : value as PaymentApplicationStatus)}><SelectItem value="all">Todos</SelectItem><SelectItem value="Active">Activos</SelectItem><SelectItem value="Reversed">Revertidos</SelectItem></Select>
    </section>

    {!clientId ? <EmptyState icon={<WalletCards className="h-10 w-10" />} title="Seleccioná un cliente" detail="Vas a ver pagos aplicados, anticipos, sobrantes y recibos." />
      : applications.isLoading ? <div className="rounded-card border border-border bg-surface p-8 text-sm text-muted">Cargando aplicaciones y recibos…</div>
      : applications.isError ? <div role="alert" className="rounded-card bg-danger-soft p-5"><p className="font-semibold text-danger">No se pudieron cargar las aplicaciones.</p><Button controlKey="payments.list.retry" permission="ViewPaymentApplications" variant="outline" className="mt-3" onClick={() => applications.refetch()}>Reintentar</Button></div>
      : !applications.data?.length ? <EmptyState icon={<Link2 className="h-10 w-10" />} title="Todavía no hay aplicaciones" detail="Podés distribuir un pago manualmente o usar una prioridad automática." />
      : <div className="grid gap-4">{applications.data.map((item) => <ApplicationCard key={item.id} item={item} onReceipt={() => setReceipt(item)} onReverse={() => { setReverseTarget(item); setReverseReason(""); }} onReapply={() => reapply.mutate({ id: item.id, priority: item.priority === "Selection" ? "DueDate" : item.priority, idempotencyKey: crypto.randomUUID() })} reapplying={reapply.isPending} />)}</div>}

    <AutomaticDialog open={autoOpen} onOpenChange={setAutoOpen} clientId={clientId} />
    <ManualDialog open={manualOpen} onOpenChange={setManualOpen} clientId={clientId} />
    <SettingsDialog open={settingsOpen} onOpenChange={setSettingsOpen} />
    <ReceiptDialog value={receipt} onClose={() => setReceipt(null)} />
    <Dialog open={!!reverseTarget} onOpenChange={(open) => { if (!open) setReverseTarget(null); }}><DialogContent title="Revertir aplicación" description="El recibo se conservará y los saldos de los cargos volverán exactamente al estado anterior." size="sm"><Input controlKey="payments.reverse.reason" permission="ReversePaymentApplications" label="Motivo *" value={reverseReason} onChange={(event) => setReverseReason(event.target.value)} /><DialogFooter><Button controlKey="payments.reverse.cancel" systemRequired variant="secondary" onClick={() => setReverseTarget(null)}>Cancelar</Button><Button controlKey="payments.reverse.submit" permission="ReversePaymentApplications" variant="danger" loading={reverse.isPending} disabled={!reverseReason.trim()} onClick={performReverse}><RotateCcw className="h-4 w-4" />Revertir</Button></DialogFooter></DialogContent></Dialog>
  </div>;
}

function ApplicationCard({ item, onReceipt, onReverse, onReapply, reapplying }: { item: PaymentApplicationDto; onReceipt: () => void; onReverse: () => void; onReapply: () => void; reapplying: boolean }) {
  return <article className="rounded-card border border-border bg-surface p-4 shadow-dp1">
    <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between"><div><div className="flex flex-wrap items-center gap-2"><Badge variant={item.status === "Active" ? "jade" : "muted"}>{item.status === "Active" ? "Activo" : "Revertido"}</Badge><Badge variant="primary">{priorityLabel[item.priority]}</Badge><span className="font-mono text-xs font-bold text-foreground">{item.receiptNumber}</span></div><p className="mt-2 text-xs text-muted">{formatDateTime(item.appliedAt)} · {item.payments.length} pago(s) · {item.allocations.length} aplicación(es)</p></div><div className="flex flex-wrap gap-2"><Button controlKey="payments.receipt.open" permission="ViewPaymentReceipts" size="sm" variant="outline" onClick={onReceipt}><FileText className="h-4 w-4" />Recibo</Button>{item.status === "Active" && <Button controlKey="payments.application.reverse" permission="ReversePaymentApplications" size="sm" variant="ghost" onClick={onReverse}><RotateCcw className="h-4 w-4" />Revertir</Button>}<Button controlKey="payments.application.reapply" permission="ReapplyPayments" size="sm" variant="secondary" loading={reapplying} onClick={onReapply}><RefreshCw className="h-4 w-4" />Reaplicar</Button></div></div>
    <div className="mt-4 grid gap-3 sm:grid-cols-3"><Metric label="Pagos disponibles" value={formatAmount(item.totalPaymentAmount, item.currency)} /><Metric label="Aplicado a cargos" value={formatAmount(item.appliedAmount, item.currency)} success /><Metric label={item.unappliedAmount > 0 ? "Anticipo / sobrante" : "Sin sobrante"} value={formatAmount(item.unappliedAmount, item.currency)} attention={item.unappliedAmount > 0} /></div>
    {item.reversalReason && <p className="mt-3 rounded-input bg-danger-soft p-3 text-sm text-danger"><strong>Motivo:</strong> {item.reversalReason}</p>}
  </article>;
}

function AutomaticDialog({ open, onOpenChange, clientId }: { open: boolean; onOpenChange: (open: boolean) => void; clientId: string }) {
  const [selected, setSelected] = useState<string[]>([]); const [priority, setPriority] = useState<PaymentApplicationPriority | "">(""); const [serviceId, setServiceId] = useState("");
  const payments = useLedgerEntries({ clientId, entryType: "Income", hasUnappliedBalance: true, pageSize: 100 }); const services = useServices({ isActive: true, pageSize: 200 }); const settings = usePaymentApplicationSettings(); const apply = useApplyPaymentsAutomatically();
  const available = payments.data?.items.filter((item) => item.unappliedAmount > 0) ?? []; const chosenPriority = priority || settings.data?.defaultPriority || "DueDate";
  const toggle = (id: string) => setSelected((items) => items.includes(id) ? items.filter((item) => item !== id) : [...items, id]);
  const close = (next: boolean) => { if (!next) { setSelected([]); setPriority(""); setServiceId(""); } onOpenChange(next); };
  return <Dialog open={open} onOpenChange={close}><DialogContent title="Aplicación automática" description="Podés combinar pagos; el orden elegido determina qué cargos reciben el importe primero." size="lg"><div className="grid gap-4 md:grid-cols-2"><Select controlKey="payments.auto.priority" permission="ApplyPayments" label="Prioridad" value={chosenPriority} onValueChange={(value) => setPriority(value as PaymentApplicationPriority)}>{priorities.map((item) => <SelectItem key={item.value} value={item.value}>{item.label} · {item.detail}</SelectItem>)}</Select>{chosenPriority === "Plan" && <Combobox controlKey="payments.auto.plan" permission="ApplyPayments" label="Plan preferido" value={serviceId} onValueChange={setServiceId} options={services.data?.items.map((item) => ({ value: item.id, label: item.name, description: item.code })) ?? []} placeholder="Todos, ordenados por plan…" />}</div><section className="mt-5"><h3 className="text-sm font-bold text-foreground">Pagos disponibles</h3>{payments.isLoading ? <p className="mt-3 text-sm text-muted">Cargando pagos…</p> : !available.length ? <p className="mt-3 rounded-input bg-surface-subtle p-4 text-sm text-muted">No hay pagos con saldo disponible.</p> : <div className="mt-3 grid gap-2 sm:grid-cols-2">{available.map((payment) => <label key={payment.id} className="flex min-h-12 cursor-pointer items-center gap-3 rounded-input border border-border p-3"><input data-ui-control="payments.auto.payment.select" data-ui-permission="ApplyPayments" type="checkbox" className="h-5 w-5 accent-action" checked={selected.includes(payment.id)} onChange={() => toggle(payment.id)} /><span className="min-w-0"><strong className="block truncate text-sm text-foreground">{payment.description}</strong><span className="text-xs text-muted">{formatDate(payment.date)} · disponible {formatAmount(payment.unappliedAmount, payment.currency)}</span></span></label>)}</div>}</section><DialogFooter><Button controlKey="payments.auto.cancel" systemRequired variant="secondary" onClick={() => close(false)}>Cancelar</Button><Button controlKey="payments.auto.submit" permission="ApplyPayments" loading={apply.isPending} disabled={!selected.length || (chosenPriority === "Plan" && !serviceId)} onClick={async () => { await apply.mutateAsync({ ledgerEntryIds: selected, priority: chosenPriority, preferredServiceId: serviceId || null, idempotencyKey: crypto.randomUUID() }); close(false); }}><Bot className="h-4 w-4" />Aplicar automáticamente</Button></DialogFooter></DialogContent></Dialog>;
}

function ManualDialog({ open, onOpenChange, clientId }: { open: boolean; onOpenChange: (open: boolean) => void; clientId: string }) {
  const payments = useLedgerEntries({ clientId, entryType: "Income", hasUnappliedBalance: true, pageSize: 100 }); const charges = useBillingItems({ clientId, pageSize: 200 }); const apply = useApplyPaymentsManually();
  const [lines, setLines] = useState<Array<{ ledgerEntryId: string; billingItemId: string; amount: string }>>([{ ledgerEntryId: "", billingItemId: "", amount: "" }]);
  const paymentOptions = payments.data?.items.filter((item) => item.unappliedAmount > 0).map((item) => ({ value: item.id, label: item.description, description: `Disponible ${formatAmount(item.unappliedAmount, item.currency)} · ${formatDate(item.date)}` })) ?? [];
  const chargeOptions = charges.data?.items.filter((item) => item.status === "Pending" || item.status === "Partial").map((item) => ({ value: item.id, label: item.description, description: `Saldo ${formatAmount(item.balance, item.currency)} · vence ${formatDate(item.dueDate)}` })) ?? [];
  const update = (index: number, key: "ledgerEntryId" | "billingItemId" | "amount", value: string) => setLines((items) => items.map((item, position) => position === index ? { ...item, [key]: value } : item));
  const valid = lines.length > 0 && lines.every((line) => line.ledgerEntryId && line.billingItemId && Number(line.amount) > 0);
  const close = (next: boolean) => { if (!next) setLines([{ ledgerEntryId: "", billingItemId: "", amount: "" }]); onOpenChange(next); };
  return <Dialog open={open} onOpenChange={close}><DialogContent title="Aplicación manual" description="Distribuí uno o varios pagos entre cargos. El servidor volverá a validar cada saldo antes de guardar." size="xl"><div className="space-y-3">{lines.map((line, index) => <div key={index} className="grid gap-3 rounded-card border border-border bg-surface-subtle p-3 lg:grid-cols-[1fr_1fr_150px_44px] lg:items-end"><Combobox controlKey="payments.manual.payment" permission="ApplyPayments" label="Pago" value={line.ledgerEntryId} onValueChange={(value) => update(index, "ledgerEntryId", value)} options={paymentOptions} placeholder="Seleccionar pago…" /><Combobox controlKey="payments.manual.charge" permission="ApplyPayments" label="Cargo" value={line.billingItemId} onValueChange={(value) => update(index, "billingItemId", value)} options={chargeOptions} placeholder="Seleccionar cargo…" /><Input controlKey="payments.manual.amount" permission="ApplyPayments" label="Importe" type="number" min="0.01" step="0.01" value={line.amount} onChange={(event) => update(index, "amount", event.target.value)} /><Button controlKey="payments.manual.line.remove" permission="ApplyPayments" variant="ghost" size="icon" aria-label="Eliminar línea" disabled={lines.length === 1} onClick={() => setLines((items) => items.filter((_, position) => position !== index))}><X className="h-4 w-4" /></Button></div>)}</div><Button controlKey="payments.manual.line.add" permission="ApplyPayments" variant="ghost" className="mt-3" onClick={() => setLines((items) => [...items, { ledgerEntryId: "", billingItemId: "", amount: "" }])}><Plus className="h-4 w-4" />Agregar distribución</Button><DialogFooter><Button controlKey="payments.manual.cancel" systemRequired variant="secondary" onClick={() => close(false)}>Cancelar</Button><Button controlKey="payments.manual.submit" permission="ApplyPayments" loading={apply.isPending} disabled={!valid} onClick={async () => { const allocations: PaymentAllocationLineRequest[] = lines.map((line) => ({ ledgerEntryId: line.ledgerEntryId, billingItemId: line.billingItemId, amount: Number(line.amount) })); await apply.mutateAsync({ allocations, idempotencyKey: crypto.randomUUID() }); close(false); }}><CheckCircle2 className="h-4 w-4" />Confirmar aplicación</Button></DialogFooter></DialogContent></Dialog>;
}

function SettingsDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (open: boolean) => void }) { const settings = usePaymentApplicationSettings(); const update = useUpdatePaymentApplicationSettings(); const [value, setValue] = useState<PaymentApplicationPriority | "">(""); const current = value || settings.data?.defaultPriority || "DueDate"; return <Dialog open={open} onOpenChange={(next) => { if (!next) setValue(""); onOpenChange(next); }}><DialogContent title="Prioridad automática" description="Esta regla se usa cuando el operador no elige otra prioridad." size="sm"><Select controlKey="payments.settings.priority" permission="ManagePaymentApplicationSettings" label="Prioridad predeterminada" value={current} onValueChange={(next) => setValue(next as PaymentApplicationPriority)}>{priorities.map((item) => <SelectItem key={item.value} value={item.value}>{item.label} · {item.detail}</SelectItem>)}</Select><DialogFooter><Button controlKey="payments.settings.cancel" systemRequired variant="secondary" onClick={() => onOpenChange(false)}>Cancelar</Button><Button controlKey="payments.settings.save" permission="ManagePaymentApplicationSettings" loading={update.isPending} onClick={async () => { await update.mutateAsync({ defaultPriority: current }); onOpenChange(false); }}>Guardar prioridad</Button></DialogFooter></DialogContent></Dialog>; }

function ReceiptDialog({ value, onClose }: { value: PaymentApplicationDto | null; onClose: () => void }) { return <Dialog open={!!value} onOpenChange={(open) => { if (!open) onClose(); }}><DialogContent title={value ? `Recibo ${value.receiptNumber}` : "Recibo"} description="Evidencia de pagos, cargos y asientos asociados." size="lg">{value && <div className="space-y-5"><div className="grid gap-3 sm:grid-cols-3"><Metric label="Pagos" value={formatAmount(value.totalPaymentAmount, value.currency)} /><Metric label="Aplicado" value={formatAmount(value.appliedAmount, value.currency)} success /><Metric label="Anticipo / sobrante" value={formatAmount(value.unappliedAmount, value.currency)} attention={value.unappliedAmount > 0} /></div><section><h3 className="text-sm font-bold text-foreground">Pagos y asientos</h3><div className="mt-2 space-y-2">{value.payments.map((payment) => <article key={payment.ledgerEntryId} className="rounded-input bg-surface-subtle p-3"><div className="flex justify-between gap-3"><p className="font-semibold text-foreground">{payment.description}</p><p className="font-bold text-foreground">{formatAmount(payment.appliedAmount, value.currency)}</p></div><p className="mt-1 text-xs text-muted">{formatDate(payment.date)} · asiento {payment.journalEntryNumber}{payment.reference ? ` · ref. ${payment.reference}` : ""}</p></article>)}</div></section><section><h3 className="text-sm font-bold text-foreground">Cargos cubiertos</h3><div className="mt-2 space-y-2">{value.allocations.length ? value.allocations.map((line) => <div key={line.id} className="flex justify-between gap-3 rounded-input border border-border p-3"><p className="text-sm text-foreground">{line.billingItemDescription}</p><p className="font-bold text-success">{formatAmount(line.amount, value.currency)}</p></div>) : <p className="rounded-input bg-warning-soft p-3 text-sm text-warning">El pago quedó completo como anticipo o sobrante, sin cargos aplicados.</p>}</div></section></div>}<DialogFooter><Button controlKey="payments.receipt.close" systemRequired onClick={onClose}>Cerrar</Button></DialogFooter></DialogContent></Dialog>; }

function Metric({ label, value, success, attention }: { label: string; value: string; success?: boolean; attention?: boolean }) { return <div className={`rounded-input p-3 ${success ? "bg-success-soft" : attention ? "bg-warning-soft" : "bg-surface-subtle"}`}><p className="text-[10px] font-bold uppercase tracking-wide text-muted">{label}</p><p className="mt-1 text-lg font-bold text-foreground">{value}</p></div>; }
function EmptyState({ icon, title, detail }: { icon: React.ReactNode; title: string; detail: string }) { return <div className="grid min-h-72 place-items-center rounded-card border border-dashed border-border-strong bg-surface p-8 text-center"><div><div className="mx-auto w-fit text-muted">{icon}</div><p className="mt-3 font-semibold text-foreground">{title}</p><p className="mt-1 text-sm text-muted">{detail}</p></div></div>; }
