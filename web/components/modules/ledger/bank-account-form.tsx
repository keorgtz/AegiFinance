"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Drawer, DrawerContent, DrawerClose } from "@/components/ui/drawer";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Select, SelectItem } from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { useCreateBankAccount, useUpdateBankAccount } from "@/hooks/use-bank-accounts";
import { useCurrencies } from "@/hooks/use-currencies";
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

function PermissionSwitch({
  id,
  label,
  checked,
  onCheckedChange,
  controlKey,
  permission,
}: {
  id: string;
  label: string;
  checked: boolean;
  onCheckedChange: (v: boolean) => void;
  controlKey: string;
  permission: string;
}) {
  return (
    <div className="flex min-h-11 items-center justify-between gap-4">
      <label htmlFor={id} className="text-[13px] font-medium text-foreground-secondary cursor-pointer">
        {label}
      </label>
      <Switch
        controlKey={controlKey}
        permission={permission}
        id={id}
        checked={checked}
        onCheckedChange={onCheckedChange}
        ariaLabel={label}
      />
    </div>
  );
}

export function BankAccountForm({ open, onOpenChange, account }: BankAccountFormProps) {
  const isEdit = !!account;
  const create = useCreateBankAccount();
  const update = useUpdateBankAccount();
  const currencies = useCurrencies();
  const businessPermission = isEdit ? "UpdateBankAccounts" : "CreateBankAccounts";

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
              openingDate: account.openingDate.slice(0, 10),
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
    try {
      if (isEdit && account) {
        await update.mutateAsync({ id: account.id, data: payload });
      } else {
        await create.mutateAsync(payload);
      }
      onOpenChange(false);
    } catch {
      // React Query exposes the server response through mutationError below.
    }
  };

  const mutationError = create.error ?? update.error;

  return (
    <Drawer open={open} onOpenChange={onOpenChange}>
      <DrawerContent title={isEdit ? "Editar cuenta bancaria" : "Nueva cuenta bancaria"} width="md">
        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4" noValidate>
          <div className="rounded-card bg-action-soft p-4 text-sm text-foreground-secondary">
            <p className="font-semibold text-foreground">Identidad y apertura</p>
            <p className="mt-1 text-xs text-muted">El saldo inicial se contabiliza como un asiento. Después de crear la cuenta, moneda, importe y fecha quedan protegidos.</p>
          </div>
          <Input controlKey="ledger.bankAccounts.form.name" permission={businessPermission}
            {...register("name")}
            label="Nombre *"
            placeholder="Ej. Cuenta principal BBVA"
            autoFocus
            error={errors.name?.message}
          />
          <Input controlKey="ledger.bankAccounts.form.bankName" permission={businessPermission}
            {...register("bankName")}
            label="Banco"
            placeholder="Ej. BBVA, Banorte, HSBC…"
            error={errors.bankName?.message}
          />
          <Input controlKey="ledger.bankAccounts.form.accountNumber" permission={businessPermission}
            {...register("accountNumber")}
            label="Número de cuenta"
            placeholder={isEdit ? "Conservar número protegido" : "Número o CLABE"}
            hint={isEdit ? "Deja el valor enmascarado para conservar el número actual." : "Se cifra antes de guardarse; sólo se mostrarán los últimos 4 dígitos."}
            error={errors.accountNumber?.message}
          />
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Select controlKey="ledger.bankAccounts.form.currency" permission={businessPermission}
              label="Moneda *"
              value={watch("currency")}
              onValueChange={(value) => setValue("currency", value, { shouldValidate: true })}
              disabled={isEdit}
              error={errors.currency?.message}
            >
              {(currencies.data ?? []).filter((currency) => currency.isActive).map((currency) => (
                <SelectItem key={currency.code} value={currency.code}>{currency.code} · {currency.name}</SelectItem>
              ))}
              {!currencies.data?.length && <SelectItem value="MXN">MXN · Peso mexicano</SelectItem>}
            </Select>
            <Input controlKey="ledger.bankAccounts.form.openingBalance" permission={businessPermission}
              {...register("openingBalance")}
              type="number"
              step="0.01"
              label="Saldo inicial *"
              inputMode="decimal"
              readOnly={isEdit}
              hint={isEdit ? "Inmutable; utiliza un ajuste contable para corregirlo." : undefined}
              error={errors.openingBalance?.message}
            />
          </div>
          <Input controlKey="ledger.bankAccounts.form.openingDate" permission={businessPermission}
            {...register("openingDate")}
            type="date"
            label="Fecha de apertura *"
            readOnly={isEdit}
            error={errors.openingDate?.message}
          />
          <PermissionSwitch
            id="isActive"
            label="Cuenta activa"
            checked={watch("isActive")}
            onCheckedChange={(v) => setValue("isActive", v)}
            controlKey="ledger.bankAccounts.form.isActive"
            permission={businessPermission}
          />

          {mutationError && (
            <p role="alert" className="rounded-input bg-danger-soft p-3 text-sm font-medium text-danger">
              {mutationError.message || "No se pudo guardar la cuenta. Revisa los datos y vuelve a intentar."}
            </p>
          )}

          <div className="sticky bottom-0 -mx-1 mt-2 flex flex-col-reverse gap-3 border-t border-border bg-surface/95 px-1 pt-4 backdrop-blur sm:flex-row sm:justify-end">
            <DrawerClose asChild>
              <Button controlKey="ledger.bankAccounts.form.cancel" type="button" variant="secondary" systemRequired>Cancelar</Button>
            </DrawerClose>
            <Button controlKey="ledger.bankAccounts.form.submit" permission={businessPermission} type="submit" loading={isSubmitting}>
              {isEdit ? "Guardar cambios" : "Crear cuenta"}
            </Button>
          </div>
        </form>
      </DrawerContent>
    </Drawer>
  );
}
