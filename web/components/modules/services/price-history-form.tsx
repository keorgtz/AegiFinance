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
import { useAddPriceHistory } from "@/hooks/use-services";

const schema = z.object({
  price: z.coerce.number().min(0, "Debe ser ≥ 0"),
  currency: z.string().min(1, "Requerido"),
  effectiveDate: z.string().min(1, "Requerido"),
  reason: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

interface PriceHistoryFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  serviceId: string;
  currentCurrency?: string;
}

export function PriceHistoryForm({
  open,
  onOpenChange,
  serviceId,
  currentCurrency = "MXN",
}: PriceHistoryFormProps) {
  const addPriceHistory = useAddPriceHistory();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      price: 0,
      currency: currentCurrency,
      effectiveDate: format(new Date(), "yyyy-MM-dd"),
      reason: "",
    },
  });

  useEffect(() => {
    if (open) {
      reset({
        price: 0,
        currency: currentCurrency,
        effectiveDate: format(new Date(), "yyyy-MM-dd"),
        reason: "",
      });
    }
  }, [open, currentCurrency, reset]);

  const onSubmit = async (values: FormValues) => {
    await addPriceHistory.mutateAsync({
      id: serviceId,
      data: {
        price: values.price,
        currency: values.currency,
        effectiveDate: new Date(values.effectiveDate).toISOString(),
        reason: values.reason || null,
      },
    });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Registrar cambio de precio" size="sm">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <Input controlKey="ui.components.modules.services.price.history.form.input.1"
              {...register("price")}
              label="Nuevo precio *"
              type="number"
              step="0.01"
              min="0"
              inputMode="decimal"
              placeholder="0.00"
              autoFocus
              error={errors.price?.message}
            />
            <Input controlKey="ui.components.modules.services.price.history.form.input.2"
              {...register("currency")}
              label="Moneda *"
              placeholder="MXN"
              error={errors.currency?.message}
            />
          </div>
          <Input controlKey="ui.components.modules.services.price.history.form.input.3"
            {...register("effectiveDate")}
            label="Fecha efectiva *"
            type="date"
            error={errors.effectiveDate?.message}
          />
          <Textarea controlKey="ui.components.modules.services.price.history.form.textarea.1"
            {...register("reason")}
            label="Motivo"
            placeholder="Ej. Ajuste por inflación Q3 2026"
            rows={2}
          />

          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.services.price.history.form.button.1" type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.services.price.history.form.button.2" type="submit" loading={isSubmitting}>Guardar</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
