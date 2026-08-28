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
import { Switch } from "@/components/ui/switch";
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
  currency: z.string().trim().length(3, "Usa un código ISO de 3 letras"),
  startDate: z.string().min(1, "Requerido"),
  endDate: z.string().optional(),
  billingDay: z.coerce.number().int().min(1, "Mínimo 1").max(28, "Máximo 28"),
  customIntervalDays: z.number().int().min(1, "El intervalo mínimo es 1 día").max(3660, "El intervalo máximo es 3660 días").optional(),
  discountPercent: z.coerce.number().min(0).max(100),
  taxPercent: z.coerce.number().min(0).max(100),
  prorationPolicy: z.enum(["None", "Daily"] as const),
  contractTerms: z.string().max(4000).optional(),
  autoRenew: z.boolean(),
  notes: z.string().optional(),
}).superRefine((value, context) => {
  if (value.endDate && value.endDate < value.startDate) {
    context.addIssue({ code: "custom", path: ["endDate"], message: "La fecha final no puede ser anterior al inicio" });
  }
  if (value.billingType === "Custom" && value.customIntervalDays == null) {
    context.addIssue({ code: "custom", path: ["customIntervalDays"], message: "Indica cada cuántos días debe facturarse" });
  }
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
  const formPermission = isEdit ? "UpdateSubscriptions" : "CreateSubscriptions";
  const formScope = isEdit ? "update" : "create";
  const subscriptionControl = (field: string) => `subscriptions.${formScope}.${field}`;
  const createSub = useCreateSubscription();
  const updateSub = useUpdateSubscription();
  const resetCreateSub = createSub.reset;
  const resetUpdateSub = updateSub.reset;

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
    formState: { errors, isSubmitting, isDirty, submitCount },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      billingType: "Monthly",
      currency: "MXN",
      billingDay: 1,
      discountPercent: 0,
      taxPercent: 0,
      prorationPolicy: "None",
      autoRenew: true,
      startDate: format(new Date(), "yyyy-MM-dd"),
    },
  });

  const clientId = watch("clientId");
  const serviceId = watch("serviceId");
  const billingType = watch("billingType");
  const autoRenew = watch("autoRenew");
  const prorationPolicy = watch("prorationPolicy");
  const isRecurring = billingType === "Monthly" || billingType === "Yearly" || billingType === "Custom";

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
    if (!isRecurring && autoRenew) {
      setValue("autoRenew", false, { shouldDirty: true, shouldValidate: true });
    }
    if (billingType !== "Custom") {
      setValue("customIntervalDays", undefined, { shouldValidate: true });
    }
  }, [autoRenew, billingType, isRecurring, setValue]);

  useEffect(() => {
    if (open) {
      resetCreateSub();
      resetUpdateSub();
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
              customIntervalDays: editingSubscription.customIntervalDays ?? undefined,
              discountPercent: editingSubscription.discountPercent ?? 0,
              taxPercent: editingSubscription.taxPercent ?? 0,
              prorationPolicy: editingSubscription.prorationPolicy ?? "None",
              contractTerms: editingSubscription.contractTerms ?? "",
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
              customIntervalDays: undefined,
              discountPercent: 0,
              taxPercent: 0,
              prorationPolicy: "None",
              contractTerms: "",
              autoRenew: true,
              notes: "",
            }
      );
    }
  }, [open, editingSubscription, defaultClientId, reset, resetCreateSub, resetUpdateSub]);

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
      currency: values.currency.trim().toUpperCase(),
      startDate: new Date(values.startDate).toISOString(),
      endDate: values.endDate ? new Date(values.endDate).toISOString() : null,
      billingDay: values.billingDay,
      customIntervalDays: values.billingType === "Custom" ? values.customIntervalDays : null,
      discountPercent: values.discountPercent,
      taxPercent: values.taxPercent,
      prorationPolicy: values.prorationPolicy,
      contractTerms: values.contractTerms || null,
      autoRenew: values.autoRenew,
      notes: values.notes || null,
    };

    try {
      if (isEdit && editingSubscription) {
        await updateSub.mutateAsync({ id: editingSubscription.id, data: payload });
      } else {
        await createSub.mutateAsync(payload);
      }
      onOpenChange(false);
    } catch {
      // React Query exposes the server response through mutationError below.
    }
  };

  const mutationError = createSub.error ?? updateSub.error;
  const validationMessages = Object.values(errors)
    .map((error) => error?.message)
    .filter((message): message is string => Boolean(message));

  return (
    <Drawer open={open} onOpenChange={handleClose}>
      <DrawerContent
        title={isEdit ? "Editar suscripción" : "Nueva suscripción"}
        description={isEdit ? editingSubscription?.code : "Los campos marcados son obligatorios"}
        width="md"
        footer={
          <>
            <DrawerClose asChild>
            <Button controlKey="ui.components.modules.subscriptions.subscription.form.button.1" systemRequired type="button" variant="secondary" onClick={handleClose}>Cancelar</Button>
            </DrawerClose>
            <Button controlKey={subscriptionControl("submit")} permission={formPermission} form="subscription-form" type="submit" loading={isSubmitting}>
              {isEdit ? "Guardar cambios" : "Crear suscripción"}
            </Button>
          </>
        }
      >
        <form id="subscription-form" onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
          {mutationError && (
            <p role="alert" className="rounded-input bg-danger-soft p-3 text-sm font-medium text-danger">
              {mutationError.message || "No se pudo guardar la suscripción. Revisa los datos y vuelve a intentar."}
            </p>
          )}
          {submitCount > 0 && validationMessages.length > 0 && (
            <div role="alert" className="rounded-input border border-warning/40 bg-warning-soft p-3 text-sm text-foreground">
              <p className="font-semibold">Revisa los datos antes de guardar:</p>
              <ul className="mt-1 list-disc space-y-1 pl-5 text-foreground-secondary">
                {validationMessages.map((message) => <li key={message}>{message}</li>)}
              </ul>
            </div>
          )}
          {/* Cliente y servicio */}
          <Combobox
            controlKey={subscriptionControl("client")}
            permission={formPermission}
            label="Cliente *"
            value={clientId ?? ""}
            onValueChange={(v) => setValue("clientId", v, { shouldDirty: true })}
            options={clientOptions}
            placeholder="Selecciona un cliente…"
            searchPlaceholder="Buscar cliente…"
            error={errors.clientId?.message}
            disabled={isEdit}
          />
          <Combobox
            controlKey={subscriptionControl("service")}
            permission={formPermission}
            label="Servicio *"
            value={serviceId ?? ""}
            onValueChange={(v) => setValue("serviceId", v, { shouldDirty: true })}
            options={serviceOptions}
            placeholder="Selecciona un servicio…"
            searchPlaceholder="Buscar servicio…"
            error={errors.serviceId?.message}
            disabled={isEdit}
          />

          <hr className="border-surface-subtle" />

          {/* Facturación */}
          <p className="text-[11px] font-bold uppercase tracking-wider text-muted">
            Facturación
          </p>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Select controlKey={subscriptionControl("billing-type")}
              permission={formPermission}
              label="Tipo *"
              value={billingType}
              onValueChange={(v) => setValue("billingType", v as BillingType, { shouldDirty: true, shouldValidate: true })}
              disabled={isEdit}
            >
              {BILLING_TYPES.map((bt) => (
                <SelectItem key={bt.value} value={bt.value}>{bt.label}</SelectItem>
              ))}
            </Select>
            <Input controlKey={subscriptionControl("billing-day")}
              {...register("billingDay")}
              label="Día de facturación *"
              type="number"
              min="1"
              max="28"
              permission={formPermission}
              inputMode="numeric"
              error={errors.billingDay?.message}
              disabled={isEdit}
            />
            {billingType === "Custom" && (
              <Input
                controlKey={subscriptionControl("custom-interval")}
                permission={formPermission}
                {...register("customIntervalDays", { setValueAs: (value) => value === "" ? undefined : Number(value) })}
                label="Intervalo (días) *"
                type="number"
                min="1"
                max="3660"
                hint="Cantidad de días entre cada cargo recurrente."
                error={errors.customIntervalDays?.message}
                disabled={isEdit}
              />
            )}
          </div>
          <p className="rounded-input bg-info-soft px-3 py-2 text-xs text-foreground-secondary">
            {billingType === "Hourly"
              ? "Por hora no genera cargos automáticos: el importe se registra según las horas consumidas."
              : billingType === "OneTime"
                ? "El cargo único se programa para la fecha de inicio y no se renueva."
                : "La próxima fecha de cobro se calculará a partir del tipo y día de facturación."}
          </p>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Input controlKey={subscriptionControl("price")}
              permission={formPermission}
              {...register("price")}
              label="Precio *"
              type="number"
              step="0.01"
              min="0"
              inputMode="decimal"
              error={errors.price?.message}
              disabled={isEdit}
            />
            <Input controlKey={subscriptionControl("currency")}
              permission={formPermission}
              {...register("currency")}
              label="Moneda *"
              placeholder="MXN"
              error={errors.currency?.message}
              disabled={isEdit}
            />
          </div>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Input controlKey={subscriptionControl("discount")} permission={formPermission} {...register("discountPercent")} label="Descuento (%)" type="number" min="0" max="100" step="0.01" disabled={isEdit} />
            <Input controlKey={subscriptionControl("tax")} permission={formPermission} {...register("taxPercent")} label="Impuesto (%)" type="number" min="0" max="100" step="0.01" disabled={isEdit} />
            <Select controlKey={subscriptionControl("proration")} permission={formPermission} label="Prorrateo" value={prorationPolicy} onValueChange={(value) => setValue("prorationPolicy", value as "None" | "Daily")} disabled={isEdit}><SelectItem value="None">Sin prorrateo</SelectItem><SelectItem value="Daily">Diario</SelectItem></Select>
          </div>

          <hr className="border-surface-subtle" />

          {/* Fechas */}
          <p className="text-[11px] font-bold uppercase tracking-wider text-muted">
            Período
          </p>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Input controlKey={subscriptionControl("start-date")}
              permission={formPermission}
              {...register("startDate")}
              label="Inicio *"
              type="date"
              error={errors.startDate?.message}
              disabled={isEdit}
            />
            <Input controlKey={subscriptionControl("end-date")}
              permission={formPermission}
              {...register("endDate")}
              label="Fin (opcional)"
              type="date"
            />
          </div>

          {/* Auto renovar */}
          <div className="flex items-center justify-between rounded-input border border-border px-3 py-2.5">
            <label htmlFor="auto-renew" className="cursor-pointer">
              <p className="text-[13px] font-medium text-foreground-secondary">Renovación automática</p>
              <p className="text-[11px] text-muted">
                Renovar automáticamente al vencer
              </p>
            </label>
            <Switch
              controlKey={subscriptionControl("auto-renew")}
              permission={formPermission}
              id="auto-renew"
              checked={autoRenew}
              onCheckedChange={(v) => setValue("autoRenew", v, { shouldDirty: true })}
              disabled={!isRecurring}
              ariaLabel="Renovación automática"
              checkedLabel="Activada"
              uncheckedLabel="Desactivada"
            />
          </div>

          <hr className="border-surface-subtle" />

          <Textarea controlKey={subscriptionControl("notes")}
            permission={formPermission}
            {...register("notes")}
            label="Notas internas"
            placeholder="Observaciones sobre esta suscripción…"
            rows={3}
          />
          <Textarea controlKey={subscriptionControl("contract-terms")} permission={formPermission} {...register("contractTerms")} label="Condiciones contractuales" rows={4} disabled={isEdit} />
        </form>
      </DrawerContent>
    </Drawer>
  );
}
