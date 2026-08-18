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
  reason: z.string().min(1, "El motivo es obligatorio"),
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

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { reason: "" },
  });

  useEffect(() => {
    if (open) reset({ reason: "" });
  }, [open, reset]);

  const onSubmit = async (values: FormValues) => {
    await cancelItem.mutateAsync({ itemId, data: { reason: values.reason } });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Cancelar cargo" size="sm">
        <div className="mb-4 rounded-input border border-border bg-surface-subtle p-3">
          <p className="text-[13px] font-semibold text-foreground">{description}</p>
          <p className="mt-0.5 text-[12px] text-muted">{formatAmount(amount, currency)}</p>
        </div>
        <p className="mb-4 text-[13px] text-muted">
          Esta acción cancelará el cargo. Proporciona un motivo para el registro de auditoría.
        </p>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Textarea controlKey="ui.components.modules.billing.cancel.item.dialog.textarea.1"
            {...register("reason")}
            label="Motivo *"
            placeholder="Ej. Error de generación, duplicado, solicitud del cliente…"
            rows={3}
            autoFocus
            error={errors.reason?.message}
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.billing.cancel.item.dialog.button.1" type="button" variant="secondary">Volver</Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.billing.cancel.item.dialog.button.2" type="submit" variant="danger" loading={isSubmitting}>
              Cancelar cargo
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
