"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { Drawer, DrawerContent, DrawerClose } from "@/components/ui/drawer";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectItem } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { useCreateServiceVersion } from "@/hooks/use-services";
import type { BillingType, CreateServiceVersionRequest, ServiceDetailDto } from "@/types/api";

interface Props { open: boolean; onOpenChange: (value: boolean) => void; service: ServiceDetailDto; }

export function ServiceVersionForm({ open, onOpenChange, service }: Props) {
  const createVersion = useCreateServiceVersion();
  const { register, handleSubmit, reset, setValue, watch, formState: { isSubmitting } } = useForm<CreateServiceVersionRequest>();
  const billingType = watch("billingType");
  const prorationPolicy = watch("prorationPolicy");

  useEffect(() => {
    if (open) reset({
      name: service.name, description: service.description, billingType: service.billingType as BillingType,
      basePrice: service.defaultPrice, currency: service.currency, defaultDiscountPercent: 0,
      defaultTaxPercent: 0, prorationPolicy: "None", effectiveFrom: new Date().toISOString().slice(0, 10),
      terms: "", isPublished: true,
      concepts: [{ code: "BASE", name: service.name, description: null, quantity: 1, unitPrice: service.defaultPrice, taxPercent: 0, sortOrder: 0 }],
    });
  }, [open, reset, service]);

  const submit = async (values: CreateServiceVersionRequest) => {
    await createVersion.mutateAsync({ id: service.id, data: { ...values, effectiveFrom: new Date(`${values.effectiveFrom}T00:00:00Z`).toISOString(), customIntervalDays: values.billingType === "Custom" ? Number(values.customIntervalDays) : null, concepts: [{ ...values.concepts[0], quantity: Number(values.concepts[0].quantity), unitPrice: Number(values.concepts[0].unitPrice), taxPercent: Number(values.concepts[0].taxPercent) }] } });
    onOpenChange(false);
  };

  return (
    <Drawer open={open} onOpenChange={onOpenChange}>
      <DrawerContent title="Nueva versión del plan" description="Los cargos anteriores conservarán sus condiciones originales." width="lg" footer={<><DrawerClose asChild><Button controlKey="services.version.cancel" variant="secondary">Cancelar</Button></DrawerClose><Button controlKey="services.version.publish" form="service-version-form" type="submit" loading={isSubmitting}>Publicar versión</Button></>}>
        <form id="service-version-form" onSubmit={handleSubmit(submit)} className="space-y-5">
          <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
            <Input controlKey="services.version.name" {...register("name", { required: true })} label="Nombre del plan *" />
            <Input controlKey="services.version.effective-from" {...register("effectiveFrom", { required: true })} type="date" label="Vigente desde *" />
            <Select controlKey="services.version.billing-type" label="Periodicidad" value={billingType} onValueChange={(value) => setValue("billingType", value as BillingType)}>
              <SelectItem value="Monthly">Mensual</SelectItem><SelectItem value="Yearly">Anual</SelectItem><SelectItem value="OneTime">Pago único</SelectItem><SelectItem value="Hourly">Por hora</SelectItem><SelectItem value="Custom">Personalizada</SelectItem>
            </Select>
            {billingType === "Custom" && <Input controlKey="services.version.custom-interval" {...register("customIntervalDays", { valueAsNumber: true, min: 1 })} type="number" min="1" label="Intervalo (días)" />}
            <Input controlKey="services.version.base-price" {...register("basePrice", { valueAsNumber: true, min: 0 })} type="number" min="0" step="0.01" label="Precio base" />
            <Input controlKey="services.version.currency" {...register("currency", { required: true, minLength: 3, maxLength: 3 })} label="Moneda" maxLength={3} />
            <Input controlKey="services.version.discount" {...register("defaultDiscountPercent", { valueAsNumber: true, min: 0, max: 100 })} type="number" min="0" max="100" step="0.01" label="Descuento predeterminado (%)" />
            <Input controlKey="services.version.tax" {...register("defaultTaxPercent", { valueAsNumber: true, min: 0, max: 100 })} type="number" min="0" max="100" step="0.01" label="Impuesto predeterminado (%)" />
            <Select controlKey="services.version.proration" label="Prorrateo" value={prorationPolicy} onValueChange={(value) => setValue("prorationPolicy", value as "None" | "Daily")}><SelectItem value="None">Sin prorrateo</SelectItem><SelectItem value="Daily">Diario</SelectItem></Select>
          </div>
          <Textarea controlKey="services.version.description" {...register("description")} label="Descripción" rows={2} />
          <div className="rounded-card border border-border bg-surface-subtle p-4">
            <p className="mb-3 text-[12px] font-bold text-foreground">Concepto base</p>
            <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
              <Input controlKey="services.version.concept-code" {...register("concepts.0.code", { required: true })} label="Código" />
              <Input controlKey="services.version.concept-name" {...register("concepts.0.name", { required: true })} label="Concepto" />
              <Input controlKey="services.version.concept-quantity" {...register("concepts.0.quantity", { valueAsNumber: true, min: 0.0001 })} type="number" step="0.0001" label="Cantidad" />
              <Input controlKey="services.version.concept-unit-price" {...register("concepts.0.unitPrice", { valueAsNumber: true, min: 0 })} type="number" step="0.01" label="Precio unitario" />
            </div>
          </div>
          <Textarea controlKey="services.version.terms" {...register("terms")} label="Condiciones contractuales" rows={4} />
        </form>
      </DrawerContent>
    </Drawer>
  );
}
