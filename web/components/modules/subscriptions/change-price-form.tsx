"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { format } from "date-fns";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { useChangeSubscriptionPrice } from "@/hooks/use-subscriptions";
import { formatAmount } from "@/lib/utils/format";

const schema = z.object({
  newPrice: z.coerce.number().min(0, "Debe ser ≥ 0"),
  effectiveDate: z.string().min(1, "Requerido"),
  reason: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

interface ChangePriceFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  subscriptionId: string;
  currentPrice: number;
  currency: string;
}

export function ChangePriceForm({
  open,
  onOpenChange,
  subscriptionId,
  currentPrice,
  currency,
}: ChangePriceFormProps) {
  const changePrice = useChangeSubscriptionPrice();

  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { newPrice: currentPrice, effectiveDate: format(new Date(), "yyyy-MM-dd"), reason: "" },
  });

  useEffect(() => {
    if (open) {
      reset({
        newPrice: currentPrice,
        effectiveDate: format(new Date(), "yyyy-MM-dd"),
        reason: "",
      });
    }
  }, [open, currentPrice, reset]);

  const onSubmit = async (values: FormValues) => {
    await changePrice.mutateAsync({
      id: subscriptionId,
      data: {
        newPrice: values.newPrice,
        effectiveDate: new Date(values.effectiveDate).toISOString(),
        reason: values.reason || null,
      },
    });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Cambiar precio" size="sm">
        <p className="mb-4 text-[13px] text-muted">
          Precio actual: <strong>{formatAmount(currentPrice, currency)}</strong>
        </p>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <Input controlKey="ui.components.modules.subscriptions.change.price.form.input.1"
              {...register("newPrice")}
              label="Nuevo precio *"
              type="number"
              step="0.01"
              min="0"
              inputMode="decimal"
              autoFocus
              error={errors.newPrice?.message}
            />
            <Input controlKey="ui.components.modules.subscriptions.change.price.form.input.2"
              label="Moneda"
              value={currency}
              readOnly
              disabled
            />
          </div>
          <Input controlKey="ui.components.modules.subscriptions.change.price.form.input.3"
            {...register("effectiveDate")}
            label="Fecha efectiva *"
            type="date"
            error={errors.effectiveDate?.message}
          />
          <Textarea controlKey="ui.components.modules.subscriptions.change.price.form.textarea.1"
            {...register("reason")}
            label="Motivo"
            placeholder="Ej. Ajuste tarifario Q3 2026"
            rows={2}
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.subscriptions.change.price.form.button.1" type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.subscriptions.change.price.form.button.2" type="submit" loading={isSubmitting}>Actualizar precio</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
