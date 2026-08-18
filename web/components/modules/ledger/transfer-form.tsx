"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Select, SelectItem } from "@/components/ui/select";
import { useRegisterTransfer } from "@/hooks/use-ledger";
import type { BankAccountListDto } from "@/types/api";

const schema = z
  .object({
    fromBankAccountId: z.string().min(1, "Selecciona cuenta origen"),
    toBankAccountId: z.string().min(1, "Selecciona cuenta destino"),
    amount: z.coerce.number().positive("Debe ser mayor a 0"),
    currency: z.string().min(1, "Requerido"),
    date: z.string().min(1, "Requerido"),
    description: z.string().optional(),
  })
  .refine((d) => d.fromBankAccountId !== d.toBankAccountId, {
    message: "Las cuentas origen y destino deben ser diferentes",
    path: ["toBankAccountId"],
  });

type FormValues = z.infer<typeof schema>;

interface TransferFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  accounts: BankAccountListDto[];
  defaultFromAccountId?: string;
}

export function TransferForm({ open, onOpenChange, accounts, defaultFromAccountId }: TransferFormProps) {
  const register_ = useRegisterTransfer();
  const today = new Date().toISOString().split("T")[0];

  const { register, handleSubmit, reset, watch, setValue, formState: { errors, isSubmitting } } =
    useForm<FormValues>({
      resolver: zodResolver(schema),
      defaultValues: {
        fromBankAccountId: defaultFromAccountId ?? "",
        toBankAccountId: "",
        amount: 0,
        currency: "MXN",
        date: today,
        description: "",
      },
    });

  useEffect(() => {
    if (open) {
      reset({
        fromBankAccountId: defaultFromAccountId ?? "",
        toBankAccountId: "",
        amount: 0,
        currency: "MXN",
        date: today,
        description: "",
      });
    }
  }, [open, defaultFromAccountId, reset, today]);

  const onSubmit = async (values: FormValues) => {
    await register_.mutateAsync({ ...values, description: values.description || null });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Registrar transferencia" size="md">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Select controlKey="ui.components.modules.ledger.transfer.form.select.1"
            label="Cuenta origen *"
            value={watch("fromBankAccountId")}
            onValueChange={(v) => setValue("fromBankAccountId", v)}
            error={errors.fromBankAccountId?.message}
          >
            <SelectItem value="">Seleccionar cuenta…</SelectItem>
            {accounts.map((a) => (
              <SelectItem key={a.id} value={a.id}>
                {a.name}{a.bankName ? ` — ${a.bankName}` : ""}
              </SelectItem>
            ))}
          </Select>
          <Select controlKey="ui.components.modules.ledger.transfer.form.select.2"
            label="Cuenta destino *"
            value={watch("toBankAccountId")}
            onValueChange={(v) => setValue("toBankAccountId", v)}
            error={errors.toBankAccountId?.message}
          >
            <SelectItem value="">Seleccionar cuenta…</SelectItem>
            {accounts.map((a) => (
              <SelectItem key={a.id} value={a.id}>
                {a.name}{a.bankName ? ` — ${a.bankName}` : ""}
              </SelectItem>
            ))}
          </Select>
          <div className="grid grid-cols-2 gap-3">
            <Input controlKey="ui.components.modules.ledger.transfer.form.input.1"
              {...register("amount")}
              type="number"
              step="0.01"
              label="Importe *"
              inputMode="decimal"
              autoFocus
              error={errors.amount?.message}
            />
            <Input controlKey="ui.components.modules.ledger.transfer.form.input.2"
              {...register("currency")}
              label="Moneda *"
              error={errors.currency?.message}
            />
          </div>
          <Input controlKey="ui.components.modules.ledger.transfer.form.input.3"
            {...register("date")}
            type="date"
            label="Fecha *"
            error={errors.date?.message}
          />
          <Input controlKey="ui.components.modules.ledger.transfer.form.input.4"
            {...register("description")}
            label="Descripción"
            placeholder="Ej. Traspaso de fondos…"
            error={errors.description?.message}
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.ledger.transfer.form.button.1" type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.ledger.transfer.form.button.2" type="submit" loading={isSubmitting}>Registrar transferencia</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
