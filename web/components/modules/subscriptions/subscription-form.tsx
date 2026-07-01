"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { format } from "date-fns";
import { Drawer, DrawerContent, DrawerClose } from "@/components/ui/drawer";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectItem } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import * as RadixSwitch from "@radix-ui/react-switch";
import { useClients } from "@/hooks/use-clients";
import { useServices } from "@/hooks/use-services";
import { useCreateSubscription, useUpdateSubscription } from "@/hooks/use-subscriptions";
import type { BillingType, SubscriptionDetailDto } from "@/types/api";

const BILLING_TYPES: { value: BillingType; label: string }[] = [
  { value: "Monthly", label: "Mensual" },
  { value: "Yearly", label: "Anual" },
  { value: "OneTime", label: "Único" },
  { value: "Hourly", label: "Por hora" },
  { value: "Custom", label: "Personalizado" },
];

const schema = z.object({
  clientId: z.string().min(1, "Selecciona un cliente"),
  serviceId: z.string().min(1, "Selecciona un servicio"),
  billingType: z.enum(["Monthly", "Yearly", "OneTime", "Hourly", "Custom"] as const),
  price: z.coerce.number().min(0, "Debe ser ≥ 0"),
  currency: z.string().min(1, "Requerido"),
  startDate: z.string().min(1, "Requerido"),
  endDate: z.string().optional(),
  billingDay: z.coerce.number().int().min(1, "Min 1").max(31, "Máx 31"),
  autoRenew: z.boolean(),
  notes: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

interface SubscriptionFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  editingSubscription?: SubscriptionDetailDto | null;
  defaultClientId?: string;
}

export function SubscriptionForm({
  open,
  onOpenChange,
  editingSubscription,
  defaultClientId,
}: SubscriptionFormProps) {
  const isEdit = !!editingSubscription;
  const createSub = useCreateSubscription();
  const updateSub = useUpdateSubscription();

  const { data: clientsPage } = useClients({ pageSize: 100 });
  const { data: servicesPage } = useServices({ pageSize: 100, isActive: true });

  const clientOptions =
    clientsPage?.items.map((c) => ({ value: c.id, label: c.name, description: c.code })) ?? [];
  const serviceOptions =
    servicesPage?.items.map((s) => ({
      value: s.id,
      label: s.name,
      description: `${s.code} · ${s.currency} ${s.defaultPrice}`,
    })) ?? [];

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      billingType: "Monthly",
      currency: "MXN",
      billingDay: 1,
      autoRenew: true,
      startDate: format(new Date(), "yyyy-MM-dd"),
    },
  });

  const clientId = watch("clientId");
  const serviceId = watch("serviceId");
  const billingType = watch("billingType");
  const autoRenew = watch("autoRenew");

  // When a service is selected, prefill price/currency/billingType
  const selectedService = servicesPage?.items.find((s) => s.id === serviceId);
  useEffect(() => {
    if (selectedService && !isEdit) {
      setValue("price", selectedService.defaultPrice);
      setValue("currency", selectedService.currency);
      setValue("billingType", selectedService.billingType as BillingType);
    }
  }, [selectedService, isEdit, setValue]);

  useEffect(() => {
    if (open) {
      reset(
        editingSubscription
          ? {
              clientId: editingSubscription.clientId,
              serviceId: editingSubscription.serviceId,
              billingType: editingSubscription.billingType as BillingType,
              price: editingSubscription.price,
              currency: editingSubscription.currency,
              startDate: format(new Date(editingSubscription.startDate), "yyyy-MM-dd"),
              endDate: editingSubscription.endDate
                ? format(new Date(editingSubscription.endDate), "yyyy-MM-dd")
                : "",
              billingDay: editingSubscription.billingDay,
              autoRenew: editingSubscription.autoRenew,
              notes: editingSubscription.notes ?? "",
            }
          : {
              clientId: defaultClientId ?? "",
              serviceId: "",
              billingType: "Monthly",
              price: 0,
              currency: "MXN",
              startDate: format(new Date(), "yyyy-MM-dd"),
              endDate: "",
              billingDay: 1,
              autoRenew: true,
              notes: "",
            }
      );
    }
  }, [open, editingSubscription, defaultClientId, reset]);

  const handleClose = () => {
    if (isDirty && !confirm("¿Descartar los cambios sin guardar?")) return;
    onOpenChange(false);
  };

  const onSubmit = async (values: FormValues) => {
    const payload = {
      clientId: values.clientId,
      serviceId: values.serviceId,
      billingType: values.billingType,
      price: values.price,
      currency: values.currency,
      startDate: new Date(values.startDate).toISOString(),
      endDate: values.endDate ? new Date(values.endDate).toISOString() : null,
      billingDay: values.billingDay,
      autoRenew: values.autoRenew,
      notes: values.notes || null,
    };

    if (isEdit && editingSubscription) {
      await updateSub.mutateAsync({ id: editingSubscription.id, data: payload });
    } else {
      await createSub.mutateAsync(payload);
    }
    onOpenChange(false);
  };

  return (
    <Drawer open={open} onOpenChange={handleClose}>
      <DrawerContent
        title={isEdit ? "Editar suscripción" : "Nueva suscripción"}
        description={isEdit ? editingSubscription?.code : "Los campos marcados son obligatorios"}
        width="md"
        footer={
          <>
            <DrawerClose asChild>
              <Button type="button" variant="secondary" onClick={handleClose}>Cancelar</Button>
            </DrawerClose>
            <Button form="subscription-form" type="submit" loading={isSubmitting}>
              {isEdit ? "Guardar cambios" : "Crear suscripción"}
            </Button>
          </>
        }
      >
        <form id="subscription-form" onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Cliente y servicio */}
          <Combobox
            label="Cliente *"
            value={clientId ?? ""}
            onValueChange={(v) => setValue("clientId", v, { shouldDirty: true })}
            options={clientOptions}
            placeholder="Selecciona un cliente…"
            searchPlaceholder="Buscar cliente…"
            error={errors.clientId?.message}
          />
          <Combobox
            label="Servicio *"
            value={serviceId ?? ""}
            onValueChange={(v) => setValue("serviceId", v, { shouldDirty: true })}
            options={serviceOptions}
            placeholder="Selecciona un servicio…"
            searchPlaceholder="Buscar servicio…"
            error={errors.serviceId?.message}
          />

          <hr className="border-[#F7F8FA]" />

          {/* Facturación */}
          <p className="text-[11px] font-700 uppercase tracking-wider text-[#5B6472]">
            Facturación
          </p>
          <div className="grid grid-cols-2 gap-3">
            <Select
              label="Tipo *"
              value={billingType}
              onValueChange={(v) => setValue("billingType", v as BillingType)}
            >
              {BILLING_TYPES.map((bt) => (
                <SelectItem key={bt.value} value={bt.value}>{bt.label}</SelectItem>
              ))}
            </Select>
            <Input
              {...register("billingDay")}
              label="Día de facturación *"
              type="number"
              min="1"
              max="31"
              inputMode="numeric"
              error={errors.billingDay?.message}
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input
              {...register("price")}
              label="Precio *"
              type="number"
              step="0.01"
              min="0"
              inputMode="decimal"
              error={errors.price?.message}
            />
            <Input
              {...register("currency")}
              label="Moneda *"
              placeholder="MXN"
              error={errors.currency?.message}
            />
          </div>

          <hr className="border-[#F7F8FA]" />

          {/* Fechas */}
          <p className="text-[11px] font-700 uppercase tracking-wider text-[#5B6472]">
            Período
          </p>
          <div className="grid grid-cols-2 gap-3">
            <Input
              {...register("startDate")}
              label="Inicio *"
              type="date"
              error={errors.startDate?.message}
            />
            <Input
              {...register("endDate")}
              label="Fin (opcional)"
              type="date"
            />
          </div>

          {/* Auto renovar */}
          <div className="flex items-center justify-between rounded-input border border-[#E3E6EC] px-3 py-2.5">
            <label htmlFor="auto-renew" className="cursor-pointer">
              <p className="text-[13px] font-500 text-[#3A3F4B]">Renovación automática</p>
              <p className="text-[11px] text-[#5B6472]">
                Renovar automáticamente al vencer
              </p>
            </label>
            <RadixSwitch.Root
              id="auto-renew"
              checked={autoRenew}
              onCheckedChange={(v) => setValue("autoRenew", v)}
              className="relative inline-flex h-5 w-9 shrink-0 items-center rounded-full transition-colors data-[state=checked]:bg-[#0F5C6B] data-[state=unchecked]:bg-[#E3E6EC]"
            >
              <RadixSwitch.Thumb className="block h-4 w-4 rounded-full bg-white shadow-dp1 transition-transform data-[state=checked]:translate-x-4 data-[state=unchecked]:translate-x-0.5" />
            </RadixSwitch.Root>
          </div>

          <hr className="border-[#F7F8FA]" />

          <Textarea
            {...register("notes")}
            label="Notas internas"
            placeholder="Observaciones sobre esta suscripción…"
            rows={3}
          />
        </form>
      </DrawerContent>
    </Drawer>
  );
}
