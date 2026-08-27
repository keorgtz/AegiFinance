"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Select, SelectItem } from "@/components/ui/select";
import { useRegisterIncome } from "@/hooks/use-ledger";
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

interface IncomeFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  accounts: BankAccountListDto[];
  defaultAccountId?: string;
}

export function IncomeForm({ open, onOpenChange, accounts, defaultAccountId }: IncomeFormProps) {
  const register_ = useRegisterIncome();
  const today = new Date().toISOString().split("T")[0];
  const defaultCurrency = accounts.find((account) => account.id === defaultAccountId)?.currency ?? "MXN";

  const { register, handleSubmit, reset, watch, setValue, formState: { errors, isSubmitting } } =
    useForm<FormValues>({
      resolver: zodResolver(schema),
      defaultValues: {
        bankAccountId: defaultAccountId ?? "",
        amount: 0,
        currency: defaultCurrency,
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
        currency: defaultCurrency,
        date: today,
        description: "",
        reference: "",
      });
    }
  }, [open, defaultAccountId, defaultCurrency, reset, today]);

  const onSubmit = async (values: FormValues) => {
    try {
      await register_.mutateAsync({ ...values, reference: values.reference || null });
      onOpenChange(false);
    } catch { /* Keep the API error visible. */ }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Registrar ingreso" size="md">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {register_.error && <p role="alert" className="rounded-input bg-danger-soft p-3 text-sm font-medium text-danger">{register_.error.message}</p>}
          <Select controlKey="ui.components.modules.ledger.income.form.select.1"
            permission="CreateLedgerIncome"
            label="Cuenta bancaria *"
            value={watch("bankAccountId")}
            onValueChange={(v) => { setValue("bankAccountId", v); setValue("currency", accounts.find((account) => account.id === v)?.currency ?? "MXN"); }}
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
            <Input controlKey="ui.components.modules.ledger.income.form.input.1"
              permission="CreateLedgerIncome"
              {...register("amount")}
              type="number"
              step="0.01"
              label="Importe *"
              inputMode="decimal"
              autoFocus
              error={errors.amount?.message}
            />
            <Input controlKey="ui.components.modules.ledger.income.form.input.2"
              permission="CreateLedgerIncome"
              {...register("currency")}
              label="Moneda *"
              error={errors.currency?.message}
              readOnly
            />
          </div>
          <Input controlKey="ui.components.modules.ledger.income.form.input.3"
            permission="CreateLedgerIncome"
            {...register("date")}
            type="date"
            label="Fecha *"
            error={errors.date?.message}
          />
          <Input controlKey="ui.components.modules.ledger.income.form.input.4"
            permission="CreateLedgerIncome"
            {...register("description")}
            label="Descripción *"
            placeholder="Ej. Pago de factura F-0001"
            error={errors.description?.message}
          />
          <Input controlKey="ui.components.modules.ledger.income.form.input.5"
            permission="CreateLedgerIncome"
            {...register("reference")}
            label="Referencia"
            placeholder="Ej. Transferencia, cheque, folio…"
            error={errors.reference?.message}
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.ledger.income.form.button.1" systemRequired type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.ledger.income.form.button.2" permission="CreateLedgerIncome" type="submit" loading={isSubmitting}>Registrar ingreso</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
