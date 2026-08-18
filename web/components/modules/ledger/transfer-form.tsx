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
      const defaultAccount = accounts.find((account) => account.id === defaultFromAccountId);
      reset({
        fromBankAccountId: defaultFromAccountId ?? "",
        toBankAccountId: "",
        amount: 0,
        currency: defaultAccount?.currency ?? "MXN",
        date: today,
        description: "",
      });
    }
  }, [open, defaultFromAccountId, accounts, reset, today]);

  const onSubmit = async (values: FormValues) => {
    await register_.mutateAsync({ ...values, description: values.description || null });
    onOpenChange(false);
  };

  const fromAccount = accounts.find((account) => account.id === watch("fromBankAccountId"));
  const eligibleDestinations = fromAccount
    ? accounts.filter((account) => account.id !== fromAccount.id && account.currency === fromAccount.currency)
    : accounts;

  const selectSource = (value: string) => {
    const selected = accounts.find((account) => account.id === value);
    setValue("fromBankAccountId", value, { shouldValidate: true });
    if (selected) {
      setValue("currency", selected.currency, { shouldValidate: true });
      const currentDestination = accounts.find((account) => account.id === watch("toBankAccountId"));
      if (currentDestination && currentDestination.currency !== selected.currency) setValue("toBankAccountId", "");
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Registrar transferencia" size="md">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="rounded-card bg-action-soft p-4 text-sm text-foreground-secondary">
            La transferencia mueve fondos entre dos cuentas de la misma moneda y genera un único asiento balanceado; no crea ingreso ni gasto.
          </div>
          <Select controlKey="ledger.transfers.form.source" permission="CreateLedgerTransfers"
            label="Cuenta origen *"
            value={watch("fromBankAccountId")}
            onValueChange={selectSource}
            error={errors.fromBankAccountId?.message}
          >
            <SelectItem value="">Seleccionar cuenta…</SelectItem>
            {accounts.map((a) => (
              <SelectItem key={a.id} value={a.id}>
                {a.name}{a.bankName ? ` — ${a.bankName}` : ""}
              </SelectItem>
            ))}
          </Select>
          <Select controlKey="ledger.transfers.form.destination" permission="CreateLedgerTransfers"
            label="Cuenta destino *"
            value={watch("toBankAccountId")}
            onValueChange={(v) => setValue("toBankAccountId", v, { shouldValidate: true })}
            error={errors.toBankAccountId?.message}
          >
            <SelectItem value="">Seleccionar cuenta…</SelectItem>
            {eligibleDestinations.map((a) => (
              <SelectItem key={a.id} value={a.id}>
                {a.name}{a.bankName ? ` — ${a.bankName}` : ""}
              </SelectItem>
            ))}
          </Select>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Input controlKey="ledger.transfers.form.amount" permission="CreateLedgerTransfers"
              {...register("amount")}
              type="number"
              step="0.01"
              label="Importe *"
              inputMode="decimal"
              autoFocus
              error={errors.amount?.message}
            />
            <Input controlKey="ledger.transfers.form.currency" permission="CreateLedgerTransfers"
              {...register("currency")}
              label="Moneda *"
              readOnly
              error={errors.currency?.message}
            />
          </div>
          <Input controlKey="ledger.transfers.form.date" permission="CreateLedgerTransfers"
            {...register("date")}
            type="date"
            label="Fecha *"
            error={errors.date?.message}
          />
          <Input controlKey="ledger.transfers.form.description" permission="CreateLedgerTransfers"
            {...register("description")}
            label="Descripción"
            placeholder="Ej. Traspaso de fondos…"
            error={errors.description?.message}
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ledger.transfers.form.cancel" type="button" variant="secondary" systemRequired>Cancelar</Button>
            </DialogClose>
            <Button controlKey="ledger.transfers.form.submit" permission="CreateLedgerTransfers" type="submit" loading={isSubmitting}>Registrar transferencia</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
