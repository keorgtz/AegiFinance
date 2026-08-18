"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { Select, SelectItem } from "@/components/ui/select";
import { useRegisterAdjustment } from "@/hooks/use-ledger";
import type { BankAccountListDto } from "@/types/api";

const schema = z.object({
  bankAccountId: z.string().min(1, "Selecciona una cuenta"),
  amount: z.coerce.number().refine((v) => v !== 0, { message: "No puede ser cero" }),
  currency: z.string().min(1, "Requerido"),
  date: z.string().min(1, "Requerido"),
  description: z.string().min(1, "Requerido"),
  reason: z.string().min(1, "El motivo es obligatorio"),
});

type FormValues = z.infer<typeof schema>;

interface AdjustmentFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  accounts: BankAccountListDto[];
  defaultAccountId?: string;
}

export function AdjustmentForm({ open, onOpenChange, accounts, defaultAccountId }: AdjustmentFormProps) {
  const register_ = useRegisterAdjustment();
  const today = new Date().toISOString().split("T")[0];

  const { register, handleSubmit, reset, watch, setValue, formState: { errors, isSubmitting } } =
    useForm<FormValues>({
      resolver: zodResolver(schema),
      defaultValues: {
        bankAccountId: defaultAccountId ?? "",
        amount: 0,
        currency: "MXN",
        date: today,
        description: "",
        reason: "",
      },
    });

  useEffect(() => {
    if (open) {
      reset({
        bankAccountId: defaultAccountId ?? "",
        amount: 0,
        currency: "MXN",
        date: today,
        description: "",
        reason: "",
      });
    }
  }, [open, defaultAccountId, reset, today]);

  const onSubmit = async (values: FormValues) => {
    await register_.mutateAsync(values);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Registrar ajuste" size="md">
        <p className="mb-4 text-[13px] text-muted">
          Usa un importe positivo para incrementar el saldo y negativo para reducirlo.
        </p>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Select controlKey="ui.components.modules.ledger.adjustment.form.select.1"
            label="Cuenta bancaria *"
            value={watch("bankAccountId")}
            onValueChange={(v) => setValue("bankAccountId", v)}
            error={errors.bankAccountId?.message}
          >
            <SelectItem value="">Seleccionar cuenta…</SelectItem>
            {accounts.map((a) => (
              <SelectItem key={a.id} value={a.id}>
                {a.name}{a.bankName ? ` — ${a.bankName}` : ""}
              </SelectItem>
            ))}
          </Select>
          <div className="grid grid-cols-2 gap-3">
            <Input controlKey="ui.components.modules.ledger.adjustment.form.input.1"
              {...register("amount")}
              type="number"
              step="0.01"
              label="Importe *"
              inputMode="decimal"
              autoFocus
              error={errors.amount?.message}
            />
            <Input controlKey="ui.components.modules.ledger.adjustment.form.input.2"
              {...register("currency")}
              label="Moneda *"
              error={errors.currency?.message}
            />
          </div>
          <Input controlKey="ui.components.modules.ledger.adjustment.form.input.3"
            {...register("date")}
            type="date"
            label="Fecha *"
            error={errors.date?.message}
          />
          <Input controlKey="ui.components.modules.ledger.adjustment.form.input.4"
            {...register("description")}
            label="Descripción *"
            placeholder="Ej. Corrección de saldo apertura"
            error={errors.description?.message}
          />
          <Textarea controlKey="ui.components.modules.ledger.adjustment.form.textarea.1"
            {...register("reason")}
            label="Motivo *"
            placeholder="Ej. Error de captura inicial, diferencia de conciliación…"
            rows={3}
            error={errors.reason?.message}
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.ledger.adjustment.form.button.1" type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.ledger.adjustment.form.button.2" type="submit" loading={isSubmitting}>Registrar ajuste</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
