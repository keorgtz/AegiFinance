"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Drawer, DrawerContent, DrawerClose } from "@/components/ui/drawer";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import * as RadixSwitch from "@radix-ui/react-switch";
import { useCreateBankAccount, useUpdateBankAccount } from "@/hooks/use-bank-accounts";
import type { BankAccountDto } from "@/types/api";

const schema = z.object({
  name: z.string().min(1, "Requerido"),
  bankName: z.string().optional(),
  accountNumber: z.string().optional(),
  currency: z.string().min(1, "Requerido"),
  openingBalance: z.coerce.number(),
  openingDate: z.string().min(1, "Requerido"),
  isActive: z.boolean(),
});

type FormValues = z.infer<typeof schema>;

interface BankAccountFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  account?: BankAccountDto | null;
}

function SwitchRow({
  id,
  label,
  checked,
  onCheckedChange,
}: {
  id: string;
  label: string;
  checked: boolean;
  onCheckedChange: (v: boolean) => void;
}) {
  return (
    <div className="flex items-center justify-between">
      <label htmlFor={id} className="text-[13px] font-medium text-foreground-secondary cursor-pointer">
        {label}
      </label>
      <RadixSwitch.Root
        id={id}
        checked={checked}
        onCheckedChange={onCheckedChange}
        className="relative h-5 w-9 cursor-pointer rounded-full transition-colors data-[state=checked]:bg-action data-[state=unchecked]:bg-border focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-action focus-visible:ring-offset-2"
      >
        <RadixSwitch.Thumb className="block h-4 w-4 rounded-full bg-surface shadow transition-transform data-[state=checked]:translate-x-[18px] data-[state=unchecked]:translate-x-[2px]" />
      </RadixSwitch.Root>
    </div>
  );
}

export function BankAccountForm({ open, onOpenChange, account }: BankAccountFormProps) {
  const isEdit = !!account;
  const create = useCreateBankAccount();
  const update = useUpdateBankAccount();

  const today = new Date().toISOString().split("T")[0];

  const { register, handleSubmit, reset, watch, setValue, formState: { errors, isSubmitting } } =
    useForm<FormValues>({
      resolver: zodResolver(schema),
      defaultValues: {
        name: "",
        bankName: "",
        accountNumber: "",
        currency: "MXN",
        openingBalance: 0,
        openingDate: today,
        isActive: true,
      },
    });

  useEffect(() => {
    if (open) {
      reset(
        account
          ? {
              name: account.name,
              bankName: account.bankName ?? "",
              accountNumber: account.maskedAccountNumber ?? "",
              currency: account.currency,
              openingBalance: account.openingBalance,
              openingDate: account.openingDate,
              isActive: account.isActive,
            }
          : { name: "", bankName: "", accountNumber: "", currency: "MXN", openingBalance: 0, openingDate: today, isActive: true }
      );
    }
  }, [open, account, reset, today]);

  const onSubmit = async (values: FormValues) => {
    const payload = {
      ...values,
      bankName: values.bankName || null,
      accountNumber: values.accountNumber || null,
    };
    if (isEdit && account) {
      await update.mutateAsync({ id: account.id, data: payload });
    } else {
      await create.mutateAsync(payload);
    }
    onOpenChange(false);
  };

  return (
    <Drawer open={open} onOpenChange={onOpenChange}>
      <DrawerContent title={isEdit ? "Editar cuenta bancaria" : "Nueva cuenta bancaria"} width="md">
        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <Input controlKey="ui.components.modules.ledger.bank.account.form.input.1"
            {...register("name")}
            label="Nombre *"
            placeholder="Ej. Cuenta principal BBVA"
            autoFocus
            error={errors.name?.message}
          />
          <Input controlKey="ui.components.modules.ledger.bank.account.form.input.2"
            {...register("bankName")}
            label="Banco"
            placeholder="Ej. BBVA, Banorte, HSBC…"
            error={errors.bankName?.message}
          />
          <Input controlKey="ui.components.modules.ledger.bank.account.form.input.3"
            {...register("accountNumber")}
            label="Número de cuenta"
            placeholder="Ej. ****1234"
            error={errors.accountNumber?.message}
          />
          <div className="grid grid-cols-2 gap-3">
            <Input controlKey="ui.components.modules.ledger.bank.account.form.input.4"
              {...register("currency")}
              label="Moneda *"
              placeholder="MXN"
              error={errors.currency?.message}
            />
            <Input controlKey="ui.components.modules.ledger.bank.account.form.input.5"
              {...register("openingBalance")}
              type="number"
              step="0.01"
              label="Saldo inicial *"
              inputMode="decimal"
              error={errors.openingBalance?.message}
            />
          </div>
          <Input controlKey="ui.components.modules.ledger.bank.account.form.input.6"
            {...register("openingDate")}
            type="date"
            label="Fecha de apertura *"
            error={errors.openingDate?.message}
          />
          <SwitchRow
            id="isActive"
            label="Cuenta activa"
            checked={watch("isActive")}
            onCheckedChange={(v) => setValue("isActive", v)}
          />

          <div className="mt-2 flex justify-end gap-3">
            <DrawerClose asChild>
              <Button controlKey="ui.components.modules.ledger.bank.account.form.button.1" type="button" variant="secondary">Cancelar</Button>
            </DrawerClose>
            <Button controlKey="ui.components.modules.ledger.bank.account.form.button.2" type="submit" loading={isSubmitting}>
              {isEdit ? "Guardar cambios" : "Crear cuenta"}
            </Button>
          </div>
        </form>
      </DrawerContent>
    </Drawer>
  );
}
