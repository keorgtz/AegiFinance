"use client";

import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { useDuplicateRule, useUpdateDuplicateRule } from "@/hooks/use-clients";
import { useUiControl } from "@/lib/auth/ui-control";
import type { ClientDuplicateRuleDto } from "@/types/api";

const initial: ClientDuplicateRuleDto = { matchTaxId: true, matchName: true, matchBillingEmail: true, blockOnMatch: true };

export function ClientDuplicateRulesDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (open: boolean) => void }) {
  const { data, isLoading } = useDuplicateRule();
  const update = useUpdateDuplicateRule();
  const [rule, setRule] = useState(initial);
  useEffect(() => { if (data) setRule(data); }, [data]);

  const save = async () => { await update.mutateAsync(rule); onOpenChange(false); };
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Detección de duplicados" description="Definí qué coincidencias se revisan antes de guardar un cliente." size="sm">
        {isLoading ? <p className="text-sm text-muted">Cargando reglas…</p> : (
          <div className="space-y-2">
            <RuleToggle controlKey="clients.duplicate-rules.tax-id" label="RFC / ID fiscal" checked={rule.matchTaxId} onChange={(value) => setRule((current) => ({ ...current, matchTaxId: value }))} />
            <RuleToggle controlKey="clients.duplicate-rules.name" label="Nombre normalizado" checked={rule.matchName} onChange={(value) => setRule((current) => ({ ...current, matchName: value }))} />
            <RuleToggle controlKey="clients.duplicate-rules.email" label="Correo de facturación" checked={rule.matchBillingEmail} onChange={(value) => setRule((current) => ({ ...current, matchBillingEmail: value }))} />
            <div className="my-3 h-px bg-border" />
            <RuleToggle controlKey="clients.duplicate-rules.block" label="Bloquear el alta cuando exista coincidencia" checked={rule.blockOnMatch} onChange={(value) => setRule((current) => ({ ...current, blockOnMatch: value }))} />
          </div>
        )}
        <DialogFooter>
          <Button controlKey="clients.duplicate-rules.cancel" systemRequired type="button" variant="secondary" onClick={() => onOpenChange(false)}>Cancelar</Button>
          <Button controlKey="clients.duplicate-rules.save" permission="ManageClientDuplicateRules" type="button" loading={update.isPending} onClick={save}>Guardar reglas</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function RuleToggle({ controlKey, label, checked, onChange }: { controlKey: string; label: string; checked: boolean; onChange: (value: boolean) => void }) {
  const access = useUiControl({ controlKey, permission: "ManageClientDuplicateRules" });
  if (access.hidden) return null;
  return (
    <label className="flex min-h-11 items-center justify-between gap-4 rounded-input border border-border px-3 text-sm text-foreground" {...access.dataAttributes}>
      <span>{label}</span>
      <input data-ui-control={controlKey} data-ui-permission="ManageClientDuplicateRules" type="checkbox" checked={checked} disabled={access.disabled || access.readOnly} onChange={(event) => onChange(event.target.checked)} className="h-5 w-5 accent-action" />
    </label>
  );
}
