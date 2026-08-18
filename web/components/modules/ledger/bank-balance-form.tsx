"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogClose, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useRecordBankBalance } from "@/hooks/use-bank-accounts";
import { formatAmount } from "@/lib/utils/format";
import type { BankAccountListDto } from "@/types/api";

const today = () => new Date().toISOString().slice(0, 10);
const schema = z.object({
  balance: z.coerce.number({ message: "Ingresa un saldo válido" }),
  asOfDate: z.string().min(1, "La fecha es obligatoria").refine((value) => value <= today(), "La fecha no puede estar en el futuro"),
});
type Values = z.infer<typeof schema>;

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account: BankAccountListDto | null;
}

export function BankBalanceForm({ open, onOpenChange, account }: Props) {
  const mutation = useRecordBankBalance();
  const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<Values>({
    resolver: zodResolver(schema),
    defaultValues: { balance: 0, asOfDate: today() },
  });

  useEffect(() => {
    if (open) reset({ balance: account?.bankBalance ?? account?.ledgerBalance ?? 0, asOfDate: today() });
  }, [open, account, reset]);

  const submit = async (values: Values) => {
    if (!account) return;
    await mutation.mutateAsync({ id: account.id, data: values });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Registrar saldo bancario" size="sm">
        <form onSubmit={handleSubmit(submit)} className="space-y-4" noValidate>
          <div className="rounded-card bg-action-soft p-4">
            <p className="font-semibold text-foreground">{account?.name}</p>
            <p className="mt-1 text-xs text-muted">Saldo contable actual: {account ? formatAmount(account.ledgerBalance, account.currency) : "—"}. Este registro no modifica el libro mayor.</p>
          </div>
          <Input controlKey="ledger.bankBalances.form.balance" permission="UpdateBankAccountBalances" {...register("balance")} type="number" step="0.01" inputMode="decimal" label="Saldo mostrado por el banco *" autoFocus error={errors.balance?.message} />
          <Input controlKey="ledger.bankBalances.form.asOfDate" permission="UpdateBankAccountBalances" {...register("asOfDate")} type="date" max={today()} label="Fecha del saldo *" error={errors.asOfDate?.message} />
          {mutation.isError && <p role="alert" className="rounded-input bg-danger-soft p-3 text-sm font-medium text-danger">No se pudo guardar. Conservamos tus datos para que puedas reintentar.</p>}
          <DialogFooter>
            <DialogClose asChild><Button controlKey="ledger.bankBalances.form.cancel" type="button" variant="secondary" systemRequired>Cancelar</Button></DialogClose>
            <Button controlKey="ledger.bankBalances.form.submit" permission="UpdateBankAccountBalances" type="submit" loading={isSubmitting}>Guardar saldo</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
