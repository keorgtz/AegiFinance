"use client";

import { useMemo, useState } from "react";
import { AlertTriangle, BookOpen, CheckCircle2, ChevronDown, ChevronUp, DatabaseZap, Scale } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog, DialogClose, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Can } from "@/lib/auth/can";
import { formatAmount, formatDate } from "@/lib/utils/format";
import { useAccountingPeriods, useCloseAccountingPeriod, useGeneralLedgerAccounts, useJournalEntries, useMigrateLegacyLedger, useReverseJournalEntry, useTrialBalance } from "@/hooks/use-major-ledger";
import type { AccountingPeriodDto, JournalEntryDto, LegacyMigrationResultDto } from "@/types/api";

type View = "journal" | "chart" | "trial" | "periods";

const today = () => new Date().toISOString().slice(0, 10);

export function MajorLedgerPanel({ view }: { view: View }) {
  if (view === "chart") return <ChartView />;
  if (view === "trial") return <TrialBalanceView />;
  if (view === "periods") return <PeriodsView />;
  return <JournalView />;
}

function StateCard({ kind, children, retry }: { kind: "loading" | "error" | "empty"; children: React.ReactNode; retry?: () => void }) {
  return (
    <div className="rounded-card border border-border bg-surface p-8 text-center" role={kind === "error" ? "alert" : "status"}>
      <p className={kind === "error" ? "text-danger" : "text-muted"}>{children}</p>
      {retry && <Button controlKey="ledger.major.state.retry" permission="ViewMajorLedger" variant="secondary" className="mt-4" onClick={retry}>Reintentar</Button>}
    </div>
  );
}

function JournalView() {
  const entries = useJournalEntries();
  const migration = useMigrateLegacyLedger();
  const [migrationResult, setMigrationResult] = useState<LegacyMigrationResultDto | null>(null);
  const [expanded, setExpanded] = useState<string | null>(null);
  const [reversing, setReversing] = useState<JournalEntryDto | null>(null);

  const runMigration = async () => setMigrationResult(await migration.mutateAsync());

  return (
    <section aria-labelledby="journal-title" className="space-y-5">
      <div className="flex flex-col gap-4 rounded-card border border-border bg-surface p-5 shadow-dp1 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <div className="flex items-center gap-2"><BookOpen className="h-5 w-5 text-action" aria-hidden="true" /><h2 id="journal-title" className="font-display text-lg font-bold text-foreground">Libro diario</h2></div>
          <p className="mt-2 max-w-2xl text-sm text-muted">Cada asiento contabilizado conserva sus débitos, créditos, origen y reversión. Los movimientos nunca se editan después de contabilizarse.</p>
        </div>
        <Can permission="MigrateMajorLedger">
          <Button controlKey="ledger.major.migration.run" permission="MigrateMajorLedger" loading={migration.isPending} onClick={runMigration}>
            <DatabaseZap className="h-4 w-4" aria-hidden="true" />Migrar historial
          </Button>
        </Can>
      </div>

      {migrationResult && (
        <div className={`rounded-card border p-4 ${migrationResult.isReconciled ? "border-success/30 bg-success-soft" : "border-danger/30 bg-danger-soft"}`} role="status">
          <div className="flex items-start gap-3">
            {migrationResult.isReconciled ? <CheckCircle2 className="mt-0.5 h-5 w-5 text-success" /> : <AlertTriangle className="mt-0.5 h-5 w-5 text-danger" />}
            <div><p className="font-semibold text-foreground">{migrationResult.isReconciled ? "Historial conciliado" : "Migración con diferencia"}</p><p className="mt-1 text-sm text-muted">{migrationResult.migratedEntries} movimientos migrados · diferencia {formatAmount(migrationResult.difference, "MXN")}</p></div>
          </div>
        </div>
      )}

      {entries.isLoading ? <StateCard kind="loading">Cargando asientos…</StateCard> : entries.isError ? <StateCard kind="error" retry={() => entries.refetch()}>No se pudo cargar el libro diario.</StateCard> : !entries.data?.length ? <StateCard kind="empty">Todavía no hay asientos. Registrá un cargo, pago o movimiento para comenzar.</StateCard> : (
        <div className="space-y-3">
          {entries.data.map((entry) => {
            const isOpen = expanded === entry.id;
            return (
              <article key={entry.id} className="overflow-hidden rounded-card border border-border bg-surface shadow-dp1">
                <div className="grid gap-4 p-4 md:grid-cols-[minmax(0,1fr)_auto_auto] md:items-center">
                  <div className="min-w-0"><div className="flex flex-wrap items-center gap-2"><p className="font-mono text-xs font-bold text-action">{entry.entryNumber}</p><StatusBadge status={entry.status} /><Badge variant="muted">{entry.sourceType}</Badge></div><h3 className="mt-2 truncate font-semibold text-foreground">{entry.description}</h3><p className="mt-1 text-xs text-muted">{formatDate(entry.date)} · periodo {entry.accountingPeriodName}</p></div>
                  <div className="grid grid-cols-2 gap-4 text-right"><Amount label="Debe" value={entry.totalDebit} currency={entry.currency} /><Amount label="Haber" value={entry.totalCredit} currency={entry.currency} /></div>
                  <div className="flex items-center justify-end gap-2">
                    {entry.status === "Posted" && !entry.reversesJournalEntryId && <Can permission="ReverseJournalEntries"><Button controlKey="ledger.major.journal.reverse" permission="ReverseJournalEntries" variant="danger" size="sm" onClick={() => setReversing(entry)}>Revertir</Button></Can>}
                    <Button controlKey="ledger.major.journal.details" permission="ViewMajorLedger" variant="ghost" size="icon" aria-expanded={isOpen} aria-label={isOpen ? "Ocultar líneas" : "Mostrar líneas"} onClick={() => setExpanded(isOpen ? null : entry.id)}>{isOpen ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}</Button>
                  </div>
                </div>
                {isOpen && <div className="border-t border-border bg-surface-subtle p-4"><div className="space-y-2">{entry.lines.map((line) => <div key={line.id} className="grid gap-2 rounded-input bg-surface p-3 sm:grid-cols-[1fr_auto_auto] sm:items-center"><div><p className="font-mono text-xs text-muted">{line.accountCode}</p><p className="text-sm font-semibold text-foreground">{line.accountName}</p></div><Amount label="Debe" value={line.debit} currency={entry.currency} /><Amount label="Haber" value={line.credit} currency={entry.currency} /></div>)}</div></div>}
              </article>
            );
          })}
        </div>
      )}
      <ReverseDialog entry={reversing} onOpenChange={(open) => { if (!open) setReversing(null); }} />
    </section>
  );
}

function ChartView() {
  const [query, setQuery] = useState("");
  const accounts = useGeneralLedgerAccounts();
  const filtered = useMemo(() => accounts.data?.filter(account => `${account.code} ${account.name}`.toLowerCase().includes(query.toLowerCase())) ?? [], [accounts.data, query]);
  return <section className="space-y-5" aria-labelledby="chart-title"><div className="grid gap-4 rounded-card border border-border bg-surface p-5 shadow-dp1 md:grid-cols-[1fr_320px] md:items-end"><div><h2 id="chart-title" className="font-display text-lg font-bold text-foreground">Catálogo de cuentas</h2><p className="mt-2 text-sm text-muted">Cuentas de banco, clientes, ingresos, gastos, impuestos, patrimonio y diferencias.</p></div><Input controlKey="ledger.major.chart.search" permission="ViewMajorLedger" label="Buscar cuenta" value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Código o nombre" /></div>{accounts.isLoading ? <StateCard kind="loading">Cargando catálogo…</StateCard> : accounts.isError ? <StateCard kind="error" retry={() => accounts.refetch()}>No se pudo cargar el catálogo.</StateCard> : !filtered.length ? <StateCard kind="empty">No hay cuentas que coincidan con la búsqueda.</StateCard> : <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">{filtered.map(account => <article key={account.id} className="rounded-card border border-border bg-surface p-4 shadow-dp1"><div className="flex items-start justify-between gap-3"><div><p className="font-mono text-xs font-bold text-action">{account.code}</p><h3 className="mt-1 font-semibold text-foreground">{account.name}</h3></div><Badge variant={account.isActive ? "jade" : "muted"}>{account.isActive ? "Activa" : "Inactiva"}</Badge></div><div className="mt-5 flex items-end justify-between gap-3"><div><p className="text-xs text-muted">{account.accountType} · {account.purpose}</p><p className="mt-1 text-xs text-muted">{account.currency}</p></div><p className="text-lg font-bold text-foreground">{formatAmount(account.balance, account.currency)}</p></div></article>)}</div>}</section>;
}

function TrialBalanceView() {
  const [asOfDate, setAsOfDate] = useState(today());
  const trial = useTrialBalance(asOfDate);
  return <section className="space-y-5" aria-labelledby="trial-title"><div className="grid gap-4 rounded-card border border-border bg-surface p-5 shadow-dp1 sm:grid-cols-[1fr_220px] sm:items-end"><div><div className="flex items-center gap-2"><Scale className="h-5 w-5 text-action" /><h2 id="trial-title" className="font-display text-lg font-bold text-foreground">Balanza de comprobación</h2></div><p className="mt-2 text-sm text-muted">Saldos derivados exclusivamente de asientos contabilizados hasta la fecha indicada.</p></div><Input controlKey="ledger.major.trial.asOfDate" permission="ViewMajorLedger" type="date" label="Corte histórico" value={asOfDate} onChange={(event) => setAsOfDate(event.target.value)} /></div>{trial.isLoading ? <StateCard kind="loading">Calculando balanza…</StateCard> : trial.isError ? <StateCard kind="error" retry={() => trial.refetch()}>No se pudo calcular la balanza.</StateCard> : trial.data ? <><div className={`rounded-card border p-4 ${trial.data.isBalanced ? "border-success/30 bg-success-soft" : "border-danger/30 bg-danger-soft"}`} role="status"><div className="flex flex-wrap items-center justify-between gap-3"><p className={`font-semibold ${trial.data.isBalanced ? "text-success" : "text-danger"}`}>{trial.data.isBalanced ? "Balanza cuadrada" : "Balanza descuadrada"}</p><p className="text-sm text-muted">Debe {formatAmount(trial.data.totalDebit, trial.data.currency)} · Haber {formatAmount(trial.data.totalCredit, trial.data.currency)}</p></div></div><div className="overflow-hidden rounded-card border border-border bg-surface shadow-dp1"><div className="hidden grid-cols-[100px_1fr_150px_150px_150px] gap-3 border-b border-border bg-surface-subtle px-4 py-3 text-xs font-bold text-muted md:grid"><span>Cuenta</span><span>Nombre</span><span className="text-right">Debe</span><span className="text-right">Haber</span><span className="text-right">Saldo</span></div>{trial.data.lines.map(line => <article key={line.accountId} className="grid gap-3 border-b border-border px-4 py-4 last:border-0 md:grid-cols-[100px_1fr_150px_150px_150px] md:items-center"><div><p className="font-mono text-xs font-bold text-action">{line.accountCode}</p><p className="mt-1 text-xs text-muted md:hidden">{line.accountType}</p></div><p className="font-semibold text-foreground">{line.accountName}</p><Amount label="Debe" value={line.debit} currency={trial.data.currency} /><Amount label="Haber" value={line.credit} currency={trial.data.currency} /><Amount label="Saldo" value={line.balance} currency={trial.data.currency} /></article>)}</div></> : null}</section>;
}

function PeriodsView() {
  const periods = useAccountingPeriods();
  const close = useCloseAccountingPeriod();
  const [selected, setSelected] = useState<AccountingPeriodDto | null>(null);
  const confirmClose = async () => { if (!selected) return; await close.mutateAsync(selected.id); setSelected(null); };
  return <section className="space-y-5" aria-labelledby="periods-title"><div className="rounded-card border border-border bg-surface p-5 shadow-dp1"><h2 id="periods-title" className="font-display text-lg font-bold text-foreground">Periodos contables</h2><p className="mt-2 text-sm text-muted">Los periodos se abren automáticamente al contabilizar el primer movimiento. Cerrar un periodo bloquea nuevas contabilizaciones dentro de sus fechas.</p></div>{periods.isLoading ? <StateCard kind="loading">Cargando periodos…</StateCard> : periods.isError ? <StateCard kind="error" retry={() => periods.refetch()}>No se pudieron cargar los periodos.</StateCard> : !periods.data?.length ? <StateCard kind="empty">Todavía no hay periodos contables.</StateCard> : <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">{periods.data.map(period => <article key={period.id} className="rounded-card border border-border bg-surface p-4 shadow-dp1"><div className="flex items-start justify-between gap-3"><div><p className="font-semibold text-foreground">{period.name}</p><p className="mt-1 text-xs text-muted">{formatDate(period.startDate)} — {formatDate(period.endDate)}</p></div><Badge variant={period.status === "Open" ? "jade" : "muted"}>{period.status === "Open" ? "Abierto" : "Cerrado"}</Badge></div>{period.status === "Open" && <Can permission="ManageAccountingPeriods"><Button controlKey="ledger.major.periods.close" permission="ManageAccountingPeriods" variant="secondary" size="sm" className="mt-5 w-full" onClick={() => setSelected(period)}>Cerrar periodo</Button></Can>}</article>)}</div>}<Dialog open={!!selected} onOpenChange={(open) => { if (!open) setSelected(null); }}><DialogContent title="Cerrar periodo contable" description={selected ? `${selected.name} · esta acción bloquea nuevas contabilizaciones en sus fechas.` : undefined} size="sm"><p className="text-sm text-muted">Sólo se cerrará si no existen asientos en borrador. Los movimientos contabilizados conservarán su historial.</p><DialogFooter><DialogClose asChild><Button controlKey="ledger.major.periods.cancel" systemRequired type="button" variant="secondary">Cancelar</Button></DialogClose><Button controlKey="ledger.major.periods.confirm" permission="ManageAccountingPeriods" type="button" variant="danger" loading={close.isPending} onClick={confirmClose}>Confirmar cierre</Button></DialogFooter></DialogContent></Dialog></section>;
}

function ReverseDialog({ entry, onOpenChange }: { entry: JournalEntryDto | null; onOpenChange: (open: boolean) => void }) {
  const reverse = useReverseJournalEntry();
  const [date, setDate] = useState(today());
  const [reason, setReason] = useState("");
  const submit = async (event: React.FormEvent) => { event.preventDefault(); if (!entry || !reason.trim()) return; await reverse.mutateAsync({ id: entry.id, date, reason }); setReason(""); onOpenChange(false); };
  return <Dialog open={!!entry} onOpenChange={onOpenChange}><DialogContent title="Revertir asiento" description={entry ? `${entry.entryNumber} · se creará un asiento inverso; el original no se modifica.` : undefined} size="sm"><form onSubmit={submit} className="space-y-4"><Input controlKey="ledger.major.reverse.date" permission="ReverseJournalEntries" type="date" label="Fecha de reversión" required value={date} onChange={(event) => setDate(event.target.value)} /><Textarea controlKey="ledger.major.reverse.reason" permission="ReverseJournalEntries" label="Motivo" required minLength={5} value={reason} onChange={(event) => setReason(event.target.value)} hint="Este texto formará parte de la evidencia de auditoría." /><DialogFooter><DialogClose asChild><Button controlKey="ledger.major.reverse.cancel" systemRequired type="button" variant="secondary">Cancelar</Button></DialogClose><Button controlKey="ledger.major.reverse.confirm" permission="ReverseJournalEntries" type="submit" variant="danger" loading={reverse.isPending}>Crear reversión</Button></DialogFooter></form></DialogContent></Dialog>;
}

function Amount({ label, value, currency }: { label: string; value: number; currency: string }) {
  return <div className="min-w-[92px]"><p className="text-[10px] font-bold uppercase tracking-wider text-muted">{label}</p><p className="mt-1 whitespace-nowrap text-sm font-semibold text-foreground">{formatAmount(value, currency)}</p></div>;
}

function StatusBadge({ status }: { status: JournalEntryDto["status"] }) {
  return <Badge variant={status === "Posted" ? "jade" : status === "Reversed" ? "terracotta" : "saffron"}>{status === "Posted" ? "Contabilizado" : status === "Reversed" ? "Revertido" : "Borrador"}</Badge>;
}
