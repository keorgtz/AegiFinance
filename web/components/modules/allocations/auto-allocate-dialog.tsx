"use client";

import { useState } from "react";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import { useAutoAllocate } from "@/hooks/use-allocations";
import { useLedgerEntries } from "@/hooks/use-ledger";
import { formatAmount, formatDate } from "@/lib/utils/format";
import { AlertCircle, CheckCircle2 } from "lucide-react";
import type { AllocationResult } from "@/types/api";
import { cn } from "@/lib/utils/cn";

interface AutoAllocateDialogProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  clientId: string;
}

export function AutoAllocateDialog({ open, onOpenChange, clientId }: AutoAllocateDialogProps) {
  const [selectedEntryId, setSelectedEntryId] = useState("");
  const [result, setResult] = useState<AllocationResult | null>(null);
  const autoAllocate = useAutoAllocate();

  const { data: entriesPage } = useLedgerEntries({
    clientId,
    entryType: "Income",
    pageSize: 100,
  });

  const entryOptions =
    entriesPage?.items.map((e) => ({
      value: e.id,
      label: e.description,
      description: `${formatDate(e.date)} · ${formatAmount(e.amount, e.currency)}`,
    })) ?? [];

  const handleSubmit = async () => {
    if (!selectedEntryId) return;
    const res = await autoAllocate.mutateAsync(selectedEntryId);
    setResult(res);
  };

  const handleClose = (v: boolean) => {
    if (!v) {
      setSelectedEntryId("");
      setResult(null);
    }
    onOpenChange(v);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent title="Auto-asignar ingreso" size="sm">
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
                  {result.success ? "Asignación completada" : "Asignación incompleta"}
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
            <DialogFooter>
              <Button onClick={() => { setResult(null); setSelectedEntryId(""); }}>
                Nueva asignación
              </Button>
              <DialogClose asChild>
                <Button variant="secondary">Cerrar</Button>
              </DialogClose>
            </DialogFooter>
          </div>
        ) : (
          <>
            <p className="mb-4 text-[13px] text-[#5B6472]">
              Selecciona un ingreso del cliente. El sistema lo aplicará automáticamente a los
              cargos pendientes más antiguos.
            </p>
            <Combobox
              label="Ingreso (entrada ledger) *"
              value={selectedEntryId}
              onValueChange={setSelectedEntryId}
              options={entryOptions}
              placeholder="Seleccionar ingreso…"
              searchPlaceholder="Buscar por descripción…"
            />
            <DialogFooter>
              <DialogClose asChild>
                <Button type="button" variant="secondary">Cancelar</Button>
              </DialogClose>
              <Button
                onClick={handleSubmit}
                disabled={!selectedEntryId}
                loading={autoAllocate.isPending}
              >
                Auto-asignar
              </Button>
            </DialogFooter>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}
