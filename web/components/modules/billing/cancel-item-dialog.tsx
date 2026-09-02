"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { useCancelBillingItem } from "@/hooks/use-billing";
import { formatAmount } from "@/lib/utils/format";

const schema = z.object({
  reason: z.string()
    .trim()
    .min(1, "El motivo es obligatorio")
    .max(500, "El motivo no puede superar 500 caracteres"),
});

type FormValues = z.infer<typeof schema>;

interface CancelItemDialogProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  itemId: string;
  description: string;
  amount: number;
  currency: string;
}

export function CancelItemDialog({
  open,
  onOpenChange,
  itemId,
  description,
  amount,
  currency,
}: CancelItemDialogProps) {
  const cancelItem = useCancelBillingItem();
  const resetCancelItem = cancelItem.reset;

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { reason: "" },
  });

  useEffect(() => {
    if (open) {
      resetCancelItem();
      reset({ reason: "" });
    }
  }, [open, reset, resetCancelItem]);

  const onSubmit = async (values: FormValues) => {
    try {
      await cancelItem.mutateAsync({ itemId, data: { reason: values.reason } });
      onOpenChange(false);
    } catch { /* Inline error below. */ }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Cancelar cargo por reversión" size="sm">
        <div className="mb-4 rounded-input border border-border bg-surface-subtle p-3">
          <p className="text-[13px] font-semibold text-foreground">{description}</p>
          <p className="mt-0.5 text-[12px] text-muted">{formatAmount(amount, currency)}</p>
        </div>
        <p className="mb-4 text-[13px] text-muted">
          Se creará el asiento inverso y el cargo quedará cancelado sin borrar su historial.
        </p>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {cancelItem.error && <p role="alert" className="rounded-input bg-danger-soft p-3 text-sm text-danger">{cancelItem.error.message}</p>}
          <Textarea controlKey="ui.components.modules.billing.cancel.item.dialog.textarea.1"
            permission="CancelBillingItems"
            {...register("reason")}
            label="Motivo *"
            placeholder="Ej. Error de generación, duplicado, solicitud del cliente…"
            rows={3}
            maxLength={500}
            hint="Describe el motivo en un máximo de 500 caracteres; quedará registrado en la auditoría."
            autoFocus
            error={errors.reason?.message}
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.billing.cancel.item.dialog.button.1" systemRequired type="button" variant="secondary">Volver</Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.billing.cancel.item.dialog.button.2" permission="CancelBillingItems" type="submit" variant="danger" loading={isSubmitting}>
              Cancelar cargo
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
