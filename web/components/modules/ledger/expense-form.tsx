"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Select, SelectItem } from "@/components/ui/select";
import { useRegisterExpense } from "@/hooks/use-ledger";
import type { BankAccountListDto } from "@/types/api";

const schema = z.object({
  bankAccountId: z.string().min(1, "Selecciona una cuenta"),
  amount: z.coerce.number().positive("Debe ser mayor a 0"),
  currency: z.string().min(1, "Requerido"),
  date: z.string().min(1, "Requerido"),
  description: z.string().min(1, "Requerido"),
  reference: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

interface ExpenseFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  accounts: BankAccountListDto[];
  defaultAccountId?: string;
}

export function ExpenseForm({ open, onOpenChange, accounts, defaultAccountId }: ExpenseFormProps) {
  const register_ = useRegisterExpense();
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
        reference: "",
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
        reference: "",
      });
    }
  }, [open, defaultAccountId, reset, today]);

  const onSubmit = async (values: FormValues) => {
    await register_.mutateAsync({ ...values, reference: values.reference || null });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Registrar egreso" size="md">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Select controlKey="ui.components.modules.ledger.expense.form.select.1"
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
            <Input controlKey="ui.components.modules.ledger.expense.form.input.1"
              {...register("amount")}
              type="number"
              step="0.01"
              label="Importe *"
              inputMode="decimal"
              autoFocus
              error={errors.amount?.message}
            />
            <Input controlKey="ui.components.modules.ledger.expense.form.input.2"
              {...register("currency")}
              label="Moneda *"
              error={errors.currency?.message}
            />
          </div>
          <Input controlKey="ui.components.modules.ledger.expense.form.input.3"
            {...register("date")}
            type="date"
            label="Fecha *"
            error={errors.date?.message}
          />
          <Input controlKey="ui.components.modules.ledger.expense.form.input.4"
            {...register("description")}
            label="Descripción *"
            placeholder="Ej. Renta local, servicios, proveedor…"
            error={errors.description?.message}
          />
          <Input controlKey="ui.components.modules.ledger.expense.form.input.5"
            {...register("reference")}
            label="Referencia"
            placeholder="Ej. Comprobante, folio…"
            error={errors.reference?.message}
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.ledger.expense.form.button.1" type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.ledger.expense.form.button.2" type="submit" loading={isSubmitting}>Registrar egreso</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
