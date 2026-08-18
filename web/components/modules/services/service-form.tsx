"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Drawer, DrawerContent, DrawerClose } from "@/components/ui/drawer";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectItem } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import * as RadixSwitch from "@radix-ui/react-switch";
import { useServiceCategories } from "@/hooks/use-service-categories";
import { useCreateService, useUpdateService } from "@/hooks/use-services";
import type { BillingType, ServiceDetailDto } from "@/types/api";

const BILLING_TYPES: { value: BillingType; label: string }[] = [
  { value: "Monthly", label: "Mensual" },
  { value: "Yearly", label: "Anual" },
  { value: "OneTime", label: "Único" },
  { value: "Hourly", label: "Por hora" },
  { value: "Custom", label: "Personalizado" },
];

const schema = z.object({
  name: z.string().min(1, "Requerido"),
  description: z.string().optional(),
  categoryId: z.string().optional(),
  billingType: z.enum(["Monthly", "Yearly", "OneTime", "Hourly", "Custom"] as const),
  defaultPrice: z.coerce.number().min(0, "Debe ser ≥ 0"),
  currency: z.string().min(1, "Requerido"),
  isActive: z.boolean(),
  isPublic: z.boolean(),
});

type FormValues = z.infer<typeof schema>;

interface ServiceFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  editingService?: ServiceDetailDto | null;
}

export function ServiceForm({ open, onOpenChange, editingService }: ServiceFormProps) {
  const isEdit = !!editingService;
  const createService = useCreateService();
  const updateService = useUpdateService();
  const { data: categories = [] } = useServiceCategories();

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
      isActive: true,
      isPublic: false,
    },
  });

  const billingType = watch("billingType");
  const categoryId = watch("categoryId");
  const isActive = watch("isActive");
  const isPublic = watch("isPublic");

  useEffect(() => {
    if (open) {
      reset(
        editingService
          ? {
              name: editingService.name,
              description: editingService.description ?? "",
              categoryId: editingService.categoryId ?? "",
              billingType: editingService.billingType as BillingType,
              defaultPrice: editingService.defaultPrice,
              currency: editingService.currency,
              isActive: editingService.isActive,
              isPublic: editingService.isPublic,
            }
          : {
              name: "",
              description: "",
              categoryId: "",
              billingType: "Monthly",
              defaultPrice: 0,
              currency: "MXN",
              isActive: true,
              isPublic: false,
            }
      );
    }
  }, [open, editingService, reset]);

  const handleClose = () => {
    if (isDirty && !confirm("¿Descartar los cambios sin guardar?")) return;
    onOpenChange(false);
  };

  const onSubmit = async (values: FormValues) => {
    const payload = {
      name: values.name,
      description: values.description || null,
      categoryId: values.categoryId || null,
      billingType: values.billingType,
      defaultPrice: values.defaultPrice,
      currency: values.currency,
      isActive: values.isActive,
      isPublic: values.isPublic,
    };

    if (isEdit && editingService) {
      await updateService.mutateAsync({ id: editingService.id, data: payload });
    } else {
      await createService.mutateAsync(payload);
    }
    onOpenChange(false);
  };

  return (
    <Drawer open={open} onOpenChange={handleClose}>
      <DrawerContent
        title={isEdit ? "Editar servicio" : "Nuevo servicio"}
        description={isEdit ? editingService?.code : "Los campos marcados son obligatorios"}
        width="md"
        footer={
          <>
            <DrawerClose asChild>
              <Button controlKey="ui.components.modules.services.service.form.button.1" type="button" variant="secondary" onClick={handleClose}>Cancelar</Button>
            </DrawerClose>
            <Button controlKey="ui.components.modules.services.service.form.button.2" form="service-form" type="submit" loading={isSubmitting}>
              {isEdit ? "Guardar cambios" : "Crear servicio"}
            </Button>
          </>
        }
      >
        <form id="service-form" onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input controlKey="ui.components.modules.services.service.form.input.1"
            {...register("name")}
            label="Nombre *"
            placeholder="Ej. Hosting mensual básico"
            autoFocus
            error={errors.name?.message}
          />
          <Textarea controlKey="ui.components.modules.services.service.form.textarea.1"
            {...register("description")}
            label="Descripción"
            placeholder="Descripción del servicio para el catálogo público…"
            rows={3}
          />

          <div className="grid grid-cols-2 gap-3">
            <Select controlKey="ui.components.modules.services.service.form.select.1"
              label="Categoría"
              value={categoryId ?? ""}
              onValueChange={(v) => setValue("categoryId", v)}
            >
              <SelectItem value="">Sin categoría</SelectItem>
              {categories.map((c) => (
                <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>
              ))}
            </Select>
            <Select controlKey="ui.components.modules.services.service.form.select.2"
              label="Tipo de facturación *"
              value={billingType}
              onValueChange={(v) => setValue("billingType", v as BillingType)}
              error={errors.billingType?.message}
            >
              {BILLING_TYPES.map((bt) => (
                <SelectItem key={bt.value} value={bt.value}>{bt.label}</SelectItem>
              ))}
            </Select>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <Input controlKey="ui.components.modules.services.service.form.input.2"
              {...register("defaultPrice")}
              label="Precio por defecto *"
              type="number"
              step="0.01"
              min="0"
              inputMode="decimal"
              placeholder="0.00"
              error={errors.defaultPrice?.message}
            />
            <Input controlKey="ui.components.modules.services.service.form.input.3"
              {...register("currency")}
              label="Moneda *"
              placeholder="MXN"
              error={errors.currency?.message}
            />
          </div>

          <hr className="border-surface-subtle" />

          <div className="space-y-3">
            <SwitchRow
              id="is-active"
              label="Servicio activo"
              description="Disponible para asociar a suscripciones"
              checked={isActive}
              onCheckedChange={(v) => setValue("isActive", v)}
            />
            <SwitchRow
              id="is-public"
              label="Visible en el portal"
              description="El cliente puede verlo en su portal"
              checked={isPublic}
              onCheckedChange={(v) => setValue("isPublic", v)}
            />
          </div>
        </form>
      </DrawerContent>
    </Drawer>
  );
}

function SwitchRow({
  id,
  label,
  description,
  checked,
  onCheckedChange,
}: {
  id: string;
  label: string;
  description: string;
  checked: boolean;
  onCheckedChange: (v: boolean) => void;
}) {
  return (
    <div className="flex items-center justify-between rounded-input border border-border px-3 py-2.5">
      <label htmlFor={id} className="cursor-pointer">
        <p className="text-[13px] font-medium text-foreground-secondary">{label}</p>
        <p className="text-[11px] text-muted">{description}</p>
      </label>
      <RadixSwitch.Root
        id={id}
        checked={checked}
        onCheckedChange={onCheckedChange}
        className="relative inline-flex h-5 w-9 shrink-0 items-center rounded-full transition-colors data-[state=checked]:bg-action data-[state=unchecked]:bg-border"
      >
        <RadixSwitch.Thumb className="block h-4 w-4 rounded-full bg-surface shadow-dp1 transition-transform data-[state=checked]:translate-x-4 data-[state=unchecked]:translate-x-0.5" />
      </RadixSwitch.Root>
    </div>
  );
}
