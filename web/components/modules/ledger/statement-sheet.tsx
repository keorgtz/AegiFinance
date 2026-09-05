"use client";

import { useEffect, useRef, useState } from "react";
import { useUiControl } from "@/lib/auth/ui-control";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectItem } from "@/components/ui/select";
import { usePreviewBankImport, useConfirmBankImport } from "@/hooks/use-bank-imports";
import type { BankAccountListDto, BankImportBatchDto } from "@/types/api";

type Row = { id: string; date: string; description: string; debit: string; credit: string; balance: string };
const blank = (): Row => ({ id: crypto.randomUUID(), date: "", description: "", debit: "", credit: "", balance: "" });
const fields = ["date", "description", "debit", "credit", "balance"] as const;
const labels = { date: "Fecha", description: "Concepto", debit: "Cargo", credit: "Abono", balance: "Saldo bancario" };
const quote = (value: string) => `"${value.replaceAll('"', '""')}"`;

export function StatementSheet({ accounts, onImport, onContinue }: { accounts: BankAccountListDto[]; onImport: () => void; onContinue: () => void }) {
  const addAccess = useUiControl({ controlKey: "ledger.sheet.add", permission: "CreateBankStatementImports" });
  const [accountId, setAccountId] = useState("");
  const [rows, setRows] = useState<Row[]>([]);
  const [batch, setBatch] = useState<BankImportBatchDto | null>(null);
  const [offline, setOffline] = useState(false);
  const root = useRef<HTMLDivElement>(null);
  const preview = usePreviewBankImport();
  const confirm = useConfirmBankImport();
  useEffect(() => { setRows([blank()]); const update = () => setOffline(!navigator.onLine); update(); window.addEventListener("online", update); window.addEventListener("offline", update); return () => { window.removeEventListener("online", update); window.removeEventListener("offline", update); }; }, []);
  useEffect(() => { const warn = (event: BeforeUnloadEvent) => { if (rows.some(row => fields.some(field => row[field])) && batch?.status !== "Committed") event.preventDefault(); }; window.addEventListener("beforeunload", warn); return () => window.removeEventListener("beforeunload", warn); }, [rows, batch]);
  const activeRows = rows.filter(row => fields.some(field => row[field].trim()));
  const busy = preview.isPending || confirm.isPending;
  const analyze = async () => {
    const csv = [fields.join(","), ...activeRows.map(row => fields.map(field => quote(row[field])).join(","))].join("\n");
    try { setBatch(await preview.mutateAsync({ bankAccountId: accountId, adapterCode: "generic", file: new File([csv], "captura-manual.csv", { type: "text/csv" }), columns: Object.fromEntries(fields.map(field => [field, field])) })); } catch { /* Mutation renders the error below. */ }
  };
  const focus = (id: string, field: string) => requestAnimationFrame(() => root.current?.querySelector<HTMLInputElement>(`[id="sheet-${id}-${field}"]`)?.focus());
  return <section className="space-y-4" aria-label="Captura de estado de cuenta" ref={root}>
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between"><div><h2 className="text-lg font-bold text-foreground">1. Captura tu estado de cuenta</h2><p className="text-sm text-muted">Fecha → concepto → cargo → abono → saldo. Enter avanza; al terminar agrega otra fila.</p></div><Button controlKey="ledger.sheet.import" permission="CreateBankStatementImports" variant="secondary" onClick={onImport}>Importar CSV o Excel</Button></div>
    {offline && <p role="status" className="rounded-input bg-warning-soft p-3 text-foreground">Sin conexión. Puedes seguir capturando; mantén esta pantalla abierta hasta guardar.</p>}
    <Select controlKey="ledger.sheet.account" permission="CreateBankStatementImports" label="Cuenta bancaria existente" value={accountId} disabled={busy || !!batch} onValueChange={setAccountId}>{accounts.map(account => <SelectItem key={account.id} value={account.id}>{account.name} · {account.currency}</SelectItem>)}</Select>
    {!accounts.length && <p className="text-sm text-muted">Agrega una cuenta en la pestaña Cuentas bancarias para empezar.</p>}
    {!batch ? <>
      <div className="space-y-3">{rows.map((row, index) => <article key={row.id} className="rounded-card border border-border bg-surface p-3"><div className="mb-2 text-xs font-semibold text-muted">Movimiento {index + 1}</div><div className="grid grid-cols-2 gap-3 lg:grid-cols-[150px_minmax(160px,1fr)_110px_110px_130px_auto] lg:items-end">
        {fields.map((field, column) => <div key={field} className={field === "description" ? "col-span-2 lg:col-span-1" : ""}><Input controlKey={`ledger.sheet.field.${field}`} permission="CreateBankStatementImports" id={`sheet-${row.id}-${field}`} label={labels[field]} type={field === "date" ? "date" : field === "description" ? "text" : "number"} step={field === "date" || field === "description" ? undefined : "0.01"} inputMode={field === "date" || field === "description" ? undefined : "decimal"} disabled={busy} value={row[field]} onChange={event => setRows(current => current.map(item => item.id === row.id ? { ...item, [field]: event.target.value } : item))} onKeyDown={event => { if (event.key !== "Enter" || event.nativeEvent.isComposing || event.currentTarget.readOnly) return; event.preventDefault(); if (column < fields.length - 1) focus(row.id, fields[column + 1]); else { if (!rows[index + 1] && (addAccess.hidden || addAccess.disabled || addAccess.readOnly)) return; const next = rows[index + 1] ?? blank(); if (!rows[index + 1]) setRows(current => [...current, next]); focus(next.id, "date"); } }} /></div>)}
        <Button controlKey="ledger.sheet.remove" permission="CreateBankStatementImports" variant="ghost" disabled={busy} aria-label={`Eliminar movimiento ${index + 1}`} onClick={() => setRows(current => current.length === 1 ? [blank()] : current.filter(item => item.id !== row.id))}>Eliminar fila</Button>
      </div></article>)}</div>
      <div className="flex flex-wrap gap-3"><Button controlKey="ledger.sheet.add" permission="CreateBankStatementImports" variant="secondary" disabled={busy} onClick={() => { const row = blank(); setRows(current => [...current, row]); focus(row.id, "date"); }}>Agregar fila</Button><Button controlKey="ledger.sheet.preview" permission="CreateBankStatementImports" loading={preview.isPending} disabled={offline || !accountId || !activeRows.length} onClick={analyze}>Revisar {activeRows.length} movimientos</Button></div>
      <p className="text-xs text-muted">El saldo es el que aparece en tu estado bancario. Captura un cargo o un abono por fila. Puedes eliminar errores antes de guardar.</p>
    </> : <div className="space-y-3">
      <p role="status" className="font-semibold text-foreground">{batch.status === "Committed" ? `${batch.recordsImported} movimientos guardados` : `${batch.validRecords} válidos · ${batch.duplicateRecords} duplicados · ${batch.incompleteRecords + batch.rejectedRecords} por corregir`}</p>
      {batch.rows.map(row => <article key={row.id} className="rounded-input border border-border bg-surface p-3 text-sm text-foreground"><p>Fila {row.rowNumber}: {row.description} · {row.amount}</p>{row.issues.map((issue, index) => <p key={index} className="text-danger">{issue.message} {issue.correction}</p>)}</article>)}
      <div className="flex flex-wrap gap-3">{batch.status === "Committed" ? <><Button controlKey="ledger.sheet.next" permission="ViewReconciliation" onClick={onContinue}>Continuar a conciliación</Button><Button controlKey="ledger.sheet.new" permission="CreateBankStatementImports" variant="secondary" onClick={() => { setBatch(null); setRows([blank()]); }}>Otro estado de cuenta</Button></> : <><Button controlKey="ledger.sheet.edit" systemRequired variant="secondary" disabled={busy} onClick={() => setBatch(null)}>Corregir filas</Button><Button controlKey="ledger.sheet.confirm" permission="ConfirmBankStatementImports" loading={confirm.isPending} disabled={offline || !batch.validRecords || batch.incompleteRecords > 0 || batch.rejectedRecords > 0} onClick={async () => { try { setBatch(await confirm.mutateAsync(batch.id)); } catch { /* Error below. */ } }}>Guardar {batch.validRecords} movimientos</Button></>}</div>
    </div>}
    {(preview.error || confirm.error) && <p role="alert" className="rounded-input bg-danger-soft p-3 text-danger">{(preview.error || confirm.error)?.message}</p>}
  </section>;
}
