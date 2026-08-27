"use client";

import { useState } from "react";
import { AlertTriangle, FileSpreadsheet, RotateCcw, Upload } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Can } from "@/lib/auth/can";
import { formatDateTime } from "@/lib/utils/format";
import { useBankImports, useRollbackBankImport } from "@/hooks/use-bank-imports";
import type { BankImportBatchDto, BankImportStatus } from "@/types/api";

const status: Record<BankImportStatus, { label: string; variant: "jade" | "saffron" | "terracotta" | "muted" }> = {
  Preview: { label: "Sin confirmar", variant: "saffron" }, Committed: { label: "Confirmada", variant: "jade" },
  RolledBack: { label: "Revertida", variant: "muted" }, Failed: { label: "Fallida", variant: "terracotta" },
};

export function BankImportsPanel({ onImport }: { onImport: () => void }) {
  const imports = useBankImports();
  const rollback = useRollbackBankImport();
  const [rollbackBatch, setRollbackBatch] = useState<BankImportBatchDto | null>(null);
  if (imports.isLoading) return <div className="rounded-card border border-border bg-surface p-8 text-sm text-muted">Cargando lotes de importación…</div>;
  if (imports.isError) return <div role="alert" className="rounded-card bg-danger-soft p-5"><p className="font-semibold text-danger">No se pudo cargar el historial.</p><Button controlKey="ledger.bankImports.list.retry" permission="ViewBankStatementImports" className="mt-3" variant="outline" onClick={() => imports.refetch()}>Reintentar</Button></div>;
  return <section aria-labelledby="bank-imports-title">
    <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between"><div><h2 id="bank-imports-title" className="font-display text-lg font-bold text-foreground">Estados bancarios</h2><p className="mt-1 text-sm text-muted">Lotes auditables, filas rechazadas y reversiones controladas.</p></div><Button controlKey="ledger.bankImports.create" permission="CreateBankStatementImports" onClick={onImport}><Upload className="h-4 w-4" />Importar estado</Button></div>
    {!imports.data?.length ? <div className="rounded-card border border-dashed border-border-strong bg-surface p-10 text-center"><FileSpreadsheet className="mx-auto h-9 w-9 text-muted" /><p className="mt-3 font-semibold text-foreground">Todavía no hay importaciones</p><p className="mt-1 text-sm text-muted">Analizá un CSV o XLSX y confirmá únicamente sus filas válidas.</p></div> : <div className="grid gap-3">{imports.data.map((batch) => <article key={batch.id} className="rounded-card border border-border bg-surface p-4 shadow-dp1"><div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between"><div className="min-w-0"><div className="flex flex-wrap items-center gap-2"><h3 className="truncate font-semibold text-foreground">{batch.fileName}</h3><Badge variant={status[batch.status].variant}>{status[batch.status].label}</Badge></div><p className="mt-1 text-xs text-muted">{batch.bankAccountName} · {formatDateTime(batch.attemptedAt)}</p></div>{batch.status === "Committed" && <Can permission="RollbackBankStatementImports"><Button controlKey="ledger.bankImports.list.rollback" permission="RollbackBankStatementImports" variant="danger" size="sm" onClick={() => setRollbackBatch(batch)}><RotateCcw className="h-4 w-4" />Revertir</Button></Can>}</div>
        <dl className="mt-4 grid grid-cols-2 gap-3 rounded-input bg-surface-subtle p-3 sm:grid-cols-5"><Stat label="Total" value={batch.totalRecords} /><Stat label="Importadas" value={batch.recordsImported} /><Stat label="Duplicadas" value={batch.duplicateRecords} /><Stat label="Incompletas" value={batch.incompleteRecords} /><Stat label="Rechazadas" value={batch.rejectedRecords} /></dl>
      </article>)}</div>}
    <Dialog open={!!rollbackBatch} onOpenChange={(open) => { if (!open) setRollbackBatch(null); }}><DialogContent title="Revertir importación" description="Esta acción retira las líneas bancarias, pero conserva la evidencia del lote." size="sm"><div className="flex gap-3 rounded-card bg-warning-soft p-4"><AlertTriangle className="h-5 w-5 shrink-0 text-warning" /><p className="text-sm text-foreground-secondary">Sólo se completará si ninguna línea fue conciliada. El archivo y sus validaciones seguirán auditables.</p></div><DialogFooter><Button controlKey="ledger.bankImports.rollback.cancel" systemRequired variant="secondary" onClick={() => setRollbackBatch(null)}>Cancelar</Button><Button controlKey="ledger.bankImports.rollback.confirm" permission="RollbackBankStatementImports" variant="danger" loading={rollback.isPending} onClick={async () => { if (!rollbackBatch) return; await rollback.mutateAsync(rollbackBatch.id); setRollbackBatch(null); }}>Confirmar reversión</Button></DialogFooter></DialogContent></Dialog>
  </section>;
}

function Stat({ label, value }: { label: string; value: number }) { return <div><dt className="text-[10px] font-semibold uppercase tracking-wide text-muted">{label}</dt><dd className="mt-1 font-bold text-foreground">{value}</dd></div>; }
