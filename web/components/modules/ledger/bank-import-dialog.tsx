"use client";

import { useEffect, useMemo, useState } from "react";
import { AlertTriangle, CheckCircle2, FileSpreadsheet, Info, Upload } from "lucide-react";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectItem } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { useUiControl } from "@/lib/auth/ui-control";
import { cn } from "@/lib/utils/cn";
import { formatAmount, formatDate } from "@/lib/utils/format";
import { useBankImportMetadata, useConfirmBankImport, useCreateBankImportProfile, usePreviewBankImport } from "@/hooks/use-bank-imports";
import type { BankAccountListDto, BankImportBatchDto, BankImportRowStatus } from "@/types/api";

interface Props { open: boolean; onOpenChange: (open: boolean) => void; accounts: BankAccountListDto[]; defaultAccountId?: string; }
type Columns = { date: string; description: string; reference: string; amount: string; debit: string; credit: string; currency: string; balance: string };
const emptyColumns: Columns = { date: "", description: "", reference: "", amount: "", debit: "", credit: "", currency: "", balance: "" };

const rowBadge: Record<BankImportRowStatus, { label: string; variant: "jade" | "saffron" | "terracotta" | "muted" }> = {
  Valid: { label: "Válida", variant: "jade" }, Duplicate: { label: "Duplicada", variant: "muted" },
  Incomplete: { label: "Incompleta", variant: "saffron" }, Rejected: { label: "Rechazada", variant: "terracotta" },
};

function PermissionFile({ onChange }: { onChange: (file: File | null) => void }) {
  const access = useUiControl({ controlKey: "ledger.bankImports.form.file", permission: "CreateBankStatementImports" });
  if (access.hidden) return null;
  return <label className="flex min-h-28 cursor-pointer flex-col items-center justify-center rounded-card border border-dashed border-border-strong bg-surface-subtle p-5 text-center transition-ui hover:border-action" {...access.dataAttributes}>
    <FileSpreadsheet className="h-7 w-7 text-action" aria-hidden="true" /><span className="mt-2 text-sm font-semibold text-foreground">Seleccionar CSV o XLSX</span><span className="mt-1 text-xs text-muted">Máximo 10 MB; el archivo se analiza antes de guardar.</span>
    <input data-ui-control="ui.components.modules.ledger.bank.import.dialog.input.1" className="sr-only" type="file" accept=".csv,.xlsx,text/csv,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" disabled={access.disabled || access.readOnly} onChange={(event) => onChange(event.target.files?.[0] ?? null)} />
  </label>;
}

export function BankImportDialog({ open, onOpenChange, accounts, defaultAccountId }: Props) {
  const [accountId, setAccountId] = useState(defaultAccountId ?? "");
  const [file, setFile] = useState<File | null>(null);
  const [adapterCode, setAdapterCode] = useState("generic");
  const [profileId, setProfileId] = useState("");
  const [advanced, setAdvanced] = useState(false);
  const [columns, setColumns] = useState<Columns>(emptyColumns);
  const [profileName, setProfileName] = useState("");
  const [delimiter, setDelimiter] = useState(",");
  const [headerRow, setHeaderRow] = useState("1");
  const [dateFormat, setDateFormat] = useState("");
  const [batch, setBatch] = useState<BankImportBatchDto | null>(null);
  const metadata = useBankImportMetadata(open);
  const preview = usePreviewBankImport();
  const confirm = useConfirmBankImport();
  const createProfile = useCreateBankImportProfile();
  const selectedAccount = useMemo(() => accounts.find((item) => item.id === accountId), [accounts, accountId]);

  useEffect(() => { if (open) setAccountId(defaultAccountId || accounts.find((item) => item.isActive)?.id || ""); }, [open, defaultAccountId, accounts]);
  const reset = () => { setFile(null); setBatch(null); setColumns(emptyColumns); setAdvanced(false); setProfileName(""); setDelimiter(","); setHeaderRow("1"); setDateFormat(""); };
  const close = (next: boolean) => { if (!next) reset(); onOpenChange(next); };

  const analyze = async () => {
    if (!file || !accountId) return;
    const mapped = Object.fromEntries(Object.entries(columns).filter(([, value]) => value.trim()).map(([key, value]) => [key, value.trim()]));
    const result = await preview.mutateAsync({ bankAccountId: accountId, file, adapterCode, profileId: profileId || undefined, columns: mapped });
    setBatch(result);
  };
  const saveProfile = async () => {
    if (!profileName.trim() || !columns.date.trim() || !columns.description.trim() || (!columns.amount.trim() && !columns.debit.trim() && !columns.credit.trim())) return;
    const saved = await createProfile.mutateAsync({ name: profileName.trim(), adapterCode, dateColumn: columns.date.trim(), descriptionColumn: columns.description.trim(), referenceColumn: columns.reference.trim() || null, amountColumn: columns.amount.trim() || null, debitColumn: columns.debit.trim() || null, creditColumn: columns.credit.trim() || null, currencyColumn: columns.currency.trim() || null, balanceColumn: columns.balance.trim() || null, dateFormat: dateFormat.trim() || null, delimiter, headerRow: Math.max(1, Number(headerRow) || 1) });
    setProfileId(saved.id);
  };
  const commit = async () => { if (!batch) return; const result = await confirm.mutateAsync(batch.id); setBatch(result); };

  return <Dialog open={open} onOpenChange={close}><DialogContent title="Importar estado bancario" description="Revisá cada fila antes de incorporarla a conciliación." size="lg" className="max-w-5xl">
    {!batch ? <div className="space-y-5">
      <div className="grid gap-4 md:grid-cols-2">
        <Select controlKey="ledger.bankImports.form.account" permission="CreateBankStatementImports" label="Cuenta bancaria *" value={accountId} onValueChange={setAccountId}>
          {accounts.filter((item) => item.isActive).map((item) => <SelectItem key={item.id} value={item.id}>{item.name} · {item.currency}</SelectItem>)}
        </Select>
        <Select controlKey="ledger.bankImports.form.adapter" permission="CreateBankStatementImports" label="Formato del banco" value={adapterCode} onValueChange={setAdapterCode}>
          {(metadata.adapters.data ?? [{ code: "generic", name: "Genérico", description: "" }]).map((item) => <SelectItem key={item.code} value={item.code}>{item.name}</SelectItem>)}
        </Select>
      </div>
      {(metadata.profiles.data?.length ?? 0) > 0 && <Select controlKey="ledger.bankImports.form.profile" permission="CreateBankStatementImports" label="Perfil de columnas (opcional)" value={profileId || "automatic"} onValueChange={(value) => setProfileId(value === "automatic" ? "" : value)}>
        <SelectItem value="automatic">Detección automática</SelectItem>{metadata.profiles.data?.map((item) => <SelectItem key={item.id} value={item.id}>{item.name}</SelectItem>)}
      </Select>}
      <PermissionFile onChange={setFile} />
      {file && <div className="flex items-center justify-between gap-3 rounded-input border border-border bg-surface p-3"><div className="min-w-0"><p className="truncate text-sm font-semibold text-foreground">{file.name}</p><p className="text-xs text-muted">{(file.size / 1024).toFixed(1)} KB</p></div><Badge variant="primary">Listo para analizar</Badge></div>}
      <Button controlKey="ledger.bankImports.form.mapping.toggle" permission="CreateBankStatementImports" type="button" variant="ghost" onClick={() => setAdvanced((value) => !value)}>{advanced ? "Ocultar asignación de columnas" : "Asignar columnas manualmente"}</Button>
      {advanced && <section className="rounded-card border border-border bg-surface-subtle p-4" aria-labelledby="mapping-title"><h3 id="mapping-title" className="font-semibold text-foreground">Encabezados del archivo</h3><p className="mt-1 text-xs text-muted">Escribí exactamente el nombre visible en la primera fila. Fecha, descripción y monto —o cargo/abono— son obligatorios.</p>
        <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">{(["date", "description", "reference", "amount", "debit", "credit", "currency", "balance"] as const).map((key) => <Input key={key} controlKey={`ledger.bankImports.mapping.${key}`} permission="CreateBankStatementImports" label={{date:"Fecha *",description:"Descripción *",reference:"Referencia",amount:"Monto",debit:"Cargo",credit:"Abono",currency:"Moneda",balance:"Saldo"}[key]} value={columns[key]} onChange={(event) => setColumns((current) => ({ ...current, [key]: event.target.value }))} />)}</div>
        <div className="mt-4 grid gap-3 sm:grid-cols-3"><Select controlKey="ledger.bankImports.profile.delimiter" permission="ManageBankImportProfiles" label="Separador CSV" value={delimiter} onValueChange={setDelimiter}><SelectItem value=",">Coma</SelectItem><SelectItem value=";">Punto y coma</SelectItem><SelectItem value="\t">Tabulador</SelectItem></Select><Input controlKey="ledger.bankImports.profile.headerRow" permission="ManageBankImportProfiles" type="number" min="1" max="100" label="Fila de encabezados" value={headerRow} onChange={(event) => setHeaderRow(event.target.value)} /><Input controlKey="ledger.bankImports.profile.dateFormat" permission="ManageBankImportProfiles" label="Formato de fecha" value={dateFormat} onChange={(event) => setDateFormat(event.target.value)} placeholder="Ej. dd/MM/yyyy" /></div>
        <div className="mt-4 grid gap-3 sm:grid-cols-[1fr_auto]"><Input controlKey="ledger.bankImports.profile.name" permission="ManageBankImportProfiles" label="Nombre para guardar este perfil" value={profileName} onChange={(event) => setProfileName(event.target.value)} placeholder="Ej. BBVA cuenta principal" /><Button controlKey="ledger.bankImports.profile.save" permission="ManageBankImportProfiles" type="button" variant="secondary" loading={createProfile.isPending} disabled={!profileName.trim()} onClick={saveProfile}>Guardar perfil</Button></div>
      </section>}
      {preview.error && <p role="alert" className="rounded-input bg-danger-soft p-3 text-sm text-danger">{preview.error.message}</p>}
      <DialogFooter><Button controlKey="ledger.bankImports.form.cancel" systemRequired type="button" variant="secondary" onClick={() => close(false)}>Cancelar</Button><Button controlKey="ledger.bankImports.form.preview" permission="CreateBankStatementImports" type="button" loading={preview.isPending} disabled={!file || !accountId} onClick={analyze}><Upload className="h-4 w-4" />Analizar archivo</Button></DialogFooter>
    </div> : <div className="space-y-5">
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-5">
        <Metric label="Total" value={batch.totalRecords} /><Metric label="Válidas" value={batch.validRecords} tone="success" /><Metric label="Duplicadas" value={batch.duplicateRecords} /><Metric label="Incompletas" value={batch.incompleteRecords} tone="warning" /><Metric label="Rechazadas" value={batch.rejectedRecords} tone="danger" />
      </div>
      {batch.status === "Committed" ? <div role="status" className="flex gap-3 rounded-card bg-success-soft p-4"><CheckCircle2 className="h-5 w-5 shrink-0 text-success" /><div><p className="font-semibold text-success">Importación confirmada</p><p className="mt-1 text-sm text-foreground-secondary">Se incorporaron {batch.recordsImported} líneas bancarias. No se crearon movimientos contables automáticos.</p></div></div> : <div className="flex gap-3 rounded-card bg-info-soft p-4"><Info className="h-5 w-5 shrink-0 text-info" /><p className="text-sm text-foreground-secondary">Sólo las filas marcadas como válidas serán importadas. Las demás quedan como evidencia con su corrección.</p></div>}
      <div className="max-h-[46dvh] space-y-2 overflow-y-auto pr-1" aria-label="Vista previa de filas">{batch.rows.map((row) => <article key={row.id} className={cn("rounded-input border bg-surface p-3", row.status === "Rejected" ? "border-danger/30" : row.status === "Incomplete" ? "border-warning/30" : "border-border")}>
        <div className="grid gap-2 sm:grid-cols-[64px_110px_1fr_130px_auto] sm:items-center"><span className="text-xs text-muted">Fila {row.rowNumber}</span><span className="text-xs text-foreground-secondary">{row.transactionDate ? formatDate(row.transactionDate) : "Sin fecha"}</span><div className="min-w-0"><p className="truncate text-sm font-semibold text-foreground">{row.description || "Sin descripción"}</p>{row.reference && <p className="truncate text-xs text-muted">Ref. {row.reference}</p>}</div><span className="font-semibold text-foreground">{row.amount == null ? "Sin monto" : formatAmount(row.amount, row.currency || selectedAccount?.currency || "MXN")}</span><Badge variant={rowBadge[row.status].variant}>{rowBadge[row.status].label}</Badge></div>
        {row.issues.length > 0 && <ul className="mt-3 space-y-1 border-t border-border pt-2">{row.issues.map((issue, index) => <li key={`${issue.field}-${index}`} className="flex gap-2 text-xs text-foreground-secondary"><AlertTriangle className="mt-0.5 h-3.5 w-3.5 shrink-0 text-warning" /><span><strong>{issue.message}</strong> {issue.correction}</span></li>)}</ul>}
      </article>)}</div>
      <DialogFooter>{batch.status === "Preview" ? <><Button controlKey="ledger.bankImports.preview.back" systemRequired type="button" variant="secondary" onClick={() => setBatch(null)}>Volver</Button><Button controlKey="ledger.bankImports.preview.confirm" permission="ConfirmBankStatementImports" type="button" loading={confirm.isPending} disabled={batch.validRecords === 0} onClick={commit}>Confirmar {batch.validRecords} filas</Button></> : <Button controlKey="ledger.bankImports.success.close" systemRequired type="button" onClick={() => close(false)}>Finalizar</Button>}</DialogFooter>
    </div>}
  </DialogContent></Dialog>;
}

function Metric({ label, value, tone }: { label: string; value: number; tone?: "success" | "warning" | "danger" }) {
  return <div className="rounded-input bg-surface-subtle p-3"><p className="text-[10px] font-semibold uppercase tracking-wide text-muted">{label}</p><p className={cn("mt-1 text-xl font-bold text-foreground", tone === "success" && "text-success", tone === "warning" && "text-warning", tone === "danger" && "text-danger")}>{value}</p></div>;
}
