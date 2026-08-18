"use client";

import { useDeferredValue, useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Drawer, DrawerContent, DrawerClose } from "@/components/ui/drawer";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectItem } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { TagChip } from "./tag-chip";
import { useClientCategories } from "@/hooks/use-client-categories";
import { useClientTags } from "@/hooks/use-client-tags";
import { useClientDuplicates, useCreateClient, useUpdateClient } from "@/hooks/use-clients";
import { useCurrencies } from "@/hooks/use-currencies";
import { useUsers } from "@/hooks/use-users";
import type { ClientDetailDto, ClientStatus } from "@/types/api";
import { AlertTriangle, Check, Tag } from "lucide-react";
import { cn } from "@/lib/utils/cn";

const schema = z.object({
  name: z.string().min(1, "Requerido"),
  tradeName: z.string().optional(),
  taxId: z.string().optional(),
  billingEmail: z.string().email("Correo inválido").optional().or(z.literal("")),
  billingAddress: z.string().optional(),
  phone: z.string().optional(),
  status: z.enum(["Active", "Inactive", "Prospective"] as const),
  notes: z.string().optional(),
  categoryId: z.string().optional(),
  presentationCurrency: z.string().length(3, "Seleccioná una moneda"),
  paymentTermsDays: z.number().min(0).max(365),
  creditLimit: z.number().min(0, "El límite no puede ser negativo"),
  commercialTerms: z.string().max(2000).optional(),
  accountManagerUserId: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

interface ClientFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  editingClient?: ClientDetailDto | null;
}

export function ClientForm({ open, onOpenChange, editingClient }: ClientFormProps) {
  const isEdit = !!editingClient;
  const createClient = useCreateClient();
  const updateClient = useUpdateClient();
  const { data: categories = [] } = useClientCategories();
  const { data: allTags = [] } = useClientTags();
  const { data: currencies = [] } = useCurrencies();
  const { data: users } = useUsers({ userType: "Administrator", isActive: true, pageSize: 100 });
  const [selectedTagIds, setSelectedTagIds] = useState<Set<string>>(new Set());
  const [tagPickerOpen, setTagPickerOpen] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { status: "Active", presentationCurrency: "MXN", paymentTermsDays: 0, creditLimit: 0 },
  });

  const status = watch("status");
  const categoryId = watch("categoryId");
  const name = watch("name") ?? "";
  const taxId = watch("taxId");
  const billingEmail = watch("billingEmail");
  const presentationCurrency = watch("presentationCurrency");
  const accountManagerUserId = watch("accountManagerUserId");
  const deferredName = useDeferredValue(name);
  const { data: duplicates = [], isFetching: checkingDuplicates } = useClientDuplicates(
    deferredName,
    taxId,
    billingEmail,
    editingClient?.id
  );
  const formPermission = isEdit ? "UpdateClients" : "CreateClients";

  useEffect(() => {
    if (open) {
      if (editingClient) {
        reset({
          name: editingClient.name,
          tradeName: editingClient.tradeName ?? "",
          taxId: editingClient.taxId ?? "",
          billingEmail: editingClient.billingEmail ?? "",
          billingAddress: editingClient.billingAddress ?? "",
          phone: editingClient.phone ?? "",
          status: editingClient.status as ClientStatus,
          notes: editingClient.notes ?? "",
          categoryId: editingClient.categoryId ?? "",
          presentationCurrency: editingClient.presentationCurrency,
          paymentTermsDays: editingClient.paymentTermsDays,
          creditLimit: editingClient.creditLimit,
          commercialTerms: editingClient.commercialTerms ?? "",
          accountManagerUserId: editingClient.accountManagerUserId ?? "",
        });
        setSelectedTagIds(new Set(editingClient.tags.map((t) => t.id)));
      } else {
        reset({ name: "", tradeName: "", taxId: "", billingEmail: "", billingAddress: "", phone: "", status: "Active", notes: "", categoryId: "", presentationCurrency: "MXN", paymentTermsDays: 0, creditLimit: 0, commercialTerms: "", accountManagerUserId: "" });
        setSelectedTagIds(new Set());
      }
    }
  }, [open, editingClient, reset]);

  const handleClose = () => {
    if (isDirty && !confirm("¿Descartar los cambios sin guardar?")) return;
    onOpenChange(false);
  };

  const onSubmit = async (values: FormValues) => {
    const payload = {
      name: values.name,
      tradeName: values.tradeName || null,
      taxId: values.taxId || null,
      billingEmail: values.billingEmail || null,
      billingAddress: values.billingAddress || null,
      phone: values.phone || null,
      status: values.status,
      notes: values.notes || null,
      categoryId: values.categoryId || null,
      tagIds: Array.from(selectedTagIds),
      presentationCurrency: values.presentationCurrency,
      paymentTermsDays: values.paymentTermsDays,
      creditLimit: values.creditLimit,
      commercialTerms: values.commercialTerms || null,
      accountManagerUserId: values.accountManagerUserId || null,
    };

    if (isEdit && editingClient) {
      await updateClient.mutateAsync({ id: editingClient.id, data: payload });
    } else {
      await createClient.mutateAsync(payload);
    }
    onOpenChange(false);
  };

  const toggleTag = (id: string) => {
    setSelectedTagIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const selectedTags = allTags.filter((t) => selectedTagIds.has(t.id));

  return (
    <Drawer open={open} onOpenChange={handleClose}>
      <DrawerContent
        title={isEdit ? "Editar cliente" : "Nuevo cliente"}
        description={isEdit ? editingClient?.code : "Los campos marcados son obligatorios"}
        width="md"
        footer={
          <>
            <DrawerClose asChild>
              <Button controlKey="clients.form.cancel" systemRequired type="button" variant="secondary" onClick={handleClose}>Cancelar</Button>
            </DrawerClose>
            <Button controlKey="clients.form.submit" permission={formPermission} form="client-form" type="submit" loading={isSubmitting}>
              {isEdit ? "Guardar cambios" : "Crear cliente"}
            </Button>
          </>
        }
      >
        <form id="client-form" onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Datos principales */}
          <Input controlKey="clients.form.name" permission={formPermission}
            {...register("name")}
            label="Nombre / Razón social *"
            placeholder="Ej. Restaurante El Fogón S.A."
            autoFocus
            error={errors.name?.message}
          />
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Input controlKey="clients.form.trade-name" permission={formPermission}
              {...register("tradeName")}
              label="Nombre comercial"
              placeholder="Ej. El Fogón"
            />
            <Input controlKey="clients.form.tax-id" permission={formPermission}
              {...register("taxId")}
              label="RFC / ID Fiscal"
              placeholder="Ej. XEXX010101000"
            />
          </div>

          {/* Categoría y estado */}
          {duplicates.length > 0 && (
            <div role="alert" className="rounded-card border border-warning/40 bg-warning-soft p-3 text-sm text-foreground">
              <div className="flex gap-2"><AlertTriangle className="mt-0.5 h-4 w-4 shrink-0 text-warning" /><div><p className="font-semibold">Posible cliente duplicado</p>{duplicates.map((item) => <p key={item.id} className="mt-1 text-xs text-muted">{item.code} · {item.name} ({item.matchedFields.join(", ")})</p>)}</div></div>
            </div>
          )}
          {checkingDuplicates && <p className="text-xs text-muted" aria-live="polite">Verificando duplicados…</p>}

          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Select controlKey="clients.form.category" permission={formPermission}
              label="Categoría"
              value={categoryId ?? ""}
              onValueChange={(v) => setValue("categoryId", v)}
            >
              <SelectItem value="">Sin categoría</SelectItem>
              {categories.map((c) => (
                <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>
              ))}
            </Select>
            <Select controlKey="clients.form.status" permission={formPermission}
              label="Estado"
              value={status}
              onValueChange={(v) => setValue("status", v as ClientStatus)}
              error={errors.status?.message}
            >
              <SelectItem value="Active">Activo</SelectItem>
              <SelectItem value="Prospective">Prospecto</SelectItem>
              <SelectItem value="Inactive">Inactivo</SelectItem>
            </Select>
          </div>

          {/* Etiquetas */}
          <div className="flex flex-col gap-1">
            <span className="text-[11px] font-semibold uppercase tracking-wider text-muted">Etiquetas</span>
            <div className="flex flex-wrap items-center gap-1.5">
              {selectedTags.map((t) => (
                <TagChip key={t.id} tag={t} />
              ))}
              <button data-ui-control="clients.form.tags.toggle" data-ui-permission={formPermission}
                type="button"
                onClick={() => setTagPickerOpen(!tagPickerOpen)}
                className="inline-flex min-h-11 items-center gap-1 rounded-full border border-dashed border-border px-3 text-xs text-muted transition-colors hover:border-action hover:text-action focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-action"
              >
                <Tag className="h-3 w-3" />
                Editar
              </button>
            </div>
            {tagPickerOpen && (
              <div className="mt-2 rounded-table border border-border bg-surface p-2 shadow-dp1">
                <div className="flex flex-wrap gap-1.5">
                  {allTags.map((tag) => (
                    <button data-ui-control="clients.form.tags.option" data-ui-permission={formPermission}
                      key={tag.id}
                      type="button"
                      onClick={() => toggleTag(tag.id)}
                      className={cn(
                        "flex min-h-11 items-center gap-1 rounded-full px-3 py-2 text-xs font-semibold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-action",
                        selectedTagIds.has(tag.id) ? "ring-2 ring-offset-1" : "opacity-70"
                      )}
                      style={{
                        backgroundColor: tag.color + "33",
                        color: tag.color,
                        border: `1px solid ${tag.color}55`,
                        outline: selectedTagIds.has(tag.id) ? `2px solid ${tag.color}` : undefined,
                        outlineOffset: selectedTagIds.has(tag.id) ? "2px" : undefined,
                      }}
                    >
                      {selectedTagIds.has(tag.id) && <Check className="h-3 w-3" />}
                      {tag.name}
                    </button>
                  ))}
                  {allTags.length === 0 && (
                    <p className="text-[12px] text-muted p-1">No hay etiquetas. Créalas desde Configuración.</p>
                  )}
                </div>
              </div>
            )}
          </div>

          <hr className="border-surface-subtle" />

          {/* Datos de contacto */}
          <p className="text-[11px] font-bold uppercase tracking-wider text-muted">Datos de contacto</p>
          <Input controlKey="clients.form.billing-email" permission={formPermission}
            {...register("billingEmail")}
            label="Correo de facturación"
            type="email"
            placeholder="facturacion@empresa.com"
            error={errors.billingEmail?.message}
          />
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Input controlKey="clients.form.phone" permission={formPermission}
              {...register("phone")}
              label="Teléfono"
              placeholder="+52 33 0000 0000"
              inputMode="tel"
            />
          </div>
          <Input controlKey="clients.form.billing-address" permission={formPermission}
            {...register("billingAddress")}
            label="Dirección de facturación"
            placeholder="Calle, número, ciudad"
          />

          <hr className="border-surface-subtle" />

          <p className="text-[11px] font-bold uppercase tracking-wider text-muted">Condiciones comerciales</p>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <Select controlKey="clients.form.presentation-currency" permission={formPermission} label="Moneda de presentación" value={presentationCurrency} onValueChange={(value) => setValue("presentationCurrency", value, { shouldDirty: true })} error={errors.presentationCurrency?.message}>
              {currencies.filter((currency) => currency.isActive).map((currency) => <SelectItem key={currency.id} value={currency.code}>{currency.code} · {currency.name}</SelectItem>)}
            </Select>
            <Input controlKey="clients.form.payment-terms-days" permission={formPermission} {...register("paymentTermsDays", { valueAsNumber: true })} label="Días de crédito" type="number" min={0} max={365} inputMode="numeric" error={errors.paymentTermsDays?.message} />
            <Input controlKey="clients.form.credit-limit" permission={formPermission} {...register("creditLimit", { valueAsNumber: true })} label="Límite de crédito" type="number" min={0} step="0.01" inputMode="decimal" error={errors.creditLimit?.message} />
            <Select controlKey="clients.form.account-manager" permission={formPermission} label="Responsable" value={accountManagerUserId ?? ""} onValueChange={(value) => setValue("accountManagerUserId", value, { shouldDirty: true })}>
              <SelectItem value="">Sin responsable</SelectItem>
              {users?.items.map((user) => <SelectItem key={user.id} value={user.id}>{user.name}</SelectItem>)}
            </Select>
          </div>
          <Textarea controlKey="clients.form.commercial-terms" permission={formPermission} {...register("commercialTerms")} label="Condiciones comerciales" placeholder="Acuerdos, excepciones o instrucciones de cobro" rows={3} error={errors.commercialTerms?.message} />

          <hr className="border-surface-subtle" />

          <Textarea controlKey="clients.form.internal-notes" permission={formPermission}
            {...register("notes")}
            label="Notas internas"
            placeholder="Observaciones sobre el cliente…"
            rows={3}
          />
        </form>
      </DrawerContent>
    </Drawer>
  );
}
