"use client";

import { useState, useCallback } from "react";
import { Drawer, DrawerContent, DrawerClose } from "@/components/ui/drawer";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Combobox } from "@/components/ui/combobox";
import { useManualAllocate } from "@/hooks/use-allocations";
import { useLedgerEntries } from "@/hooks/use-ledger";
import { useBillingItems } from "@/hooks/use-billing";
import { formatAmount, formatDate } from "@/lib/utils/format";
import { Plus, Trash2 } from "lucide-react";
import type { AllocationResult } from "@/types/api";
import { AlertCircle, CheckCircle2 } from "lucide-react";
import { cn } from "@/lib/utils/cn";

interface AllocationRow {
  billingItemId: string;
  amount: string;
}

interface ManualAllocateFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  clientId: string;
}

export function ManualAllocateForm({ open, onOpenChange, clientId }: ManualAllocateFormProps) {
  const [ledgerEntryId, setLedgerEntryId] = useState("");
  const [rows, setRows] = useState<AllocationRow[]>([{ billingItemId: "", amount: "" }]);
  const [result, setResult] = useState<AllocationResult | null>(null);
  const manualAllocate = useManualAllocate();

  const { data: entriesPage } = useLedgerEntries({
    clientId,
    entryType: "Income",
    pageSize: 100,
  });

  const { data: itemsPage } = useBillingItems({
    clientId,
    status: "Pending",
    pageSize: 100,
  });

  const entryOptions =
    entriesPage?.items.map((e) => ({
      value: e.id,
      label: e.description,
      description: `${formatDate(e.date)} · ${formatAmount(e.amount, e.currency)}`,
    })) ?? [];

  const billingItemOptions =
    itemsPage?.items.map((i) => ({
      value: i.id,
      label: i.description,
      description: `Saldo: ${formatAmount(i.balance, i.currency)} · Vence: ${formatDate(i.dueDate)}`,
    })) ?? [];

  const addRow = useCallback(() => {
    setRows((prev) => [...prev, { billingItemId: "", amount: "" }]);
  }, []);

  const removeRow = useCallback((idx: number) => {
    setRows((prev) => prev.filter((_, i) => i !== idx));
  }, []);

  const updateRow = useCallback(<K extends keyof AllocationRow>(idx: number, key: K, value: AllocationRow[K]) => {
    setRows((prev) => prev.map((r, i) => (i === idx ? { ...r, [key]: value } : r)));
  }, []);

  const isValid =
    !!ledgerEntryId &&
    rows.length > 0 &&
    rows.every((r) => !!r.billingItemId && Number(r.amount) > 0);

  const handleSubmit = async () => {
    if (!isValid) return;
    const res = await manualAllocate.mutateAsync({
      ledgerEntryId,
      allocations: rows.map((r) => ({
        billingItemId: r.billingItemId,
        amount: Number(r.amount),
      })),
    });
    setResult(res);
  };

  const handleClose = (v: boolean) => {
    if (!v) {
      setLedgerEntryId("");
      setRows([{ billingItemId: "", amount: "" }]);
      setResult(null);
    }
    onOpenChange(v);
  };

  return (
    <Drawer open={open} onOpenChange={handleClose}>
      <DrawerContent title="Asignación manual" width="lg">
        {result ? (
          <div className="space-y-4">
            <div
              className={cn(
                "flex items-start gap-3 rounded-input border p-3",
                result.success
                  ? "border-[#0E9F6E]/30 bg-[#ECFDF5]"
                  : "border-[#B6452C]/30 bg-[#FEF2F2]"
              )}
            >
              {result.success ? (
                <CheckCircle2 className="mt-0.5 h-4 w-4 flex-shrink-0 text-[#0E9F6E]" />
              ) : (
                <AlertCircle className="mt-0.5 h-4 w-4 flex-shrink-0 text-[#B6452C]" />
              )}
              <div className="space-y-1">
                <p className="text-[13px] font-600 text-[#16181D]">
                  {result.success ? "Asignación guardada" : "Asignación incompleta"}
                </p>
                <p className="text-[12px] text-[#5B6472]">
                  Asignado: <span className="font-600">{formatAmount(result.allocatedAmount)}</span>
                  {result.remainingAmount > 0 && (
                    <> · Restante: <span className="font-600 text-[#B6452C]">{formatAmount(result.remainingAmount)}</span></>
                  )}
                </p>
                {result.errors.length > 0 && (
                  <ul className="mt-1 space-y-0.5">
                    {result.errors.map((e, i) => (
                      <li key={i} className="text-[12px] text-[#B6452C]">· {e}</li>
                    ))}
                  </ul>
                )}
              </div>
            </div>
            <div className="flex justify-end gap-3">
              <Button onClick={() => { setResult(null); setLedgerEntryId(""); setRows([{ billingItemId: "", amount: "" }]); }}>
                Nueva asignación
              </Button>
              <DrawerClose asChild>
                <Button variant="secondary">Cerrar</Button>
              </DrawerClose>
            </div>
          </div>
        ) : (
          <div className="flex flex-col gap-5">
            <Combobox
              label="Ingreso (entrada ledger) *"
              value={ledgerEntryId}
              onValueChange={setLedgerEntryId}
              options={entryOptions}
              placeholder="Seleccionar ingreso…"
              searchPlaceholder="Buscar por descripción…"
            />

            <div>
              <p className="mb-2 text-[11px] font-600 uppercase tracking-wider text-[#5B6472]">
                Cargos a aplicar
              </p>
              <div className="space-y-2">
                {rows.map((row, idx) => (
                  <div key={idx} className="flex items-end gap-2">
                    <div className="flex-1">
                      <Combobox
                        label={idx === 0 ? "Cargo (billing item)" : ""}
                        value={row.billingItemId}
                        onValueChange={(v) => updateRow(idx, "billingItemId", v)}
                        options={billingItemOptions}
                        placeholder="Seleccionar cargo…"
                        searchPlaceholder="Buscar cargo…"
                      />
                    </div>
                    <div className="w-32">
                      <Input
                        label={idx === 0 ? "Importe" : ""}
                        type="number"
                        step="0.01"
                        inputMode="decimal"
                        value={row.amount}
                        onChange={(e) => updateRow(idx, "amount", e.target.value)}
                        placeholder="0.00"
                      />
                    </div>
                    {rows.length > 1 && (
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className={idx === 0 ? "mt-5" : ""}
                        onClick={() => removeRow(idx)}
                        aria-label="Eliminar fila"
                      >
                        <Trash2 className="h-4 w-4 text-[#B6452C]" />
                      </Button>
                    )}
                  </div>
                ))}
              </div>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                className="mt-2"
                onClick={addRow}
              >
                <Plus className="h-3.5 w-3.5" />
                Agregar cargo
              </Button>
            </div>

            <div className="flex justify-end gap-3">
              <DrawerClose asChild>
                <Button type="button" variant="secondary">Cancelar</Button>
              </DrawerClose>
              <Button
                onClick={handleSubmit}
                disabled={!isValid}
                loading={manualAllocate.isPending}
              >
                Guardar asignación
              </Button>
            </div>
          </div>
        )}
      </DrawerContent>
    </Drawer>
  );
}
