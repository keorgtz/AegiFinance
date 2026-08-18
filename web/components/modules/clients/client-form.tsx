"use client";

import { useEffect, useState } from "react";
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
import { useCreateClient, useUpdateClient } from "@/hooks/use-clients";
import type { ClientDetailDto, ClientStatus } from "@/types/api";
import { Check, Tag } from "lucide-react";
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
    defaultValues: { status: "Active" },
  });

  const status = watch("status");
  const categoryId = watch("categoryId");

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
        });
        setSelectedTagIds(new Set(editingClient.tags.map((t) => t.id)));
      } else {
        reset({ name: "", tradeName: "", taxId: "", billingEmail: "", billingAddress: "", phone: "", status: "Active", notes: "", categoryId: "" });
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
              <Button controlKey="ui.components.modules.clients.client.form.button.1" type="button" variant="secondary" onClick={handleClose}>Cancelar</Button>
            </DrawerClose>
            <Button controlKey="ui.components.modules.clients.client.form.button.2" form="client-form" type="submit" loading={isSubmitting}>
              {isEdit ? "Guardar cambios" : "Crear cliente"}
            </Button>
          </>
        }
      >
        <form id="client-form" onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Datos principales */}
          <Input controlKey="ui.components.modules.clients.client.form.input.1"
            {...register("name")}
            label="Nombre / Razón social *"
            placeholder="Ej. Restaurante El Fogón S.A."
            autoFocus
            error={errors.name?.message}
          />
          <div className="grid grid-cols-2 gap-3">
            <Input controlKey="ui.components.modules.clients.client.form.input.2"
              {...register("tradeName")}
              label="Nombre comercial"
              placeholder="Ej. El Fogón"
            />
            <Input controlKey="ui.components.modules.clients.client.form.input.3"
              {...register("taxId")}
              label="RFC / ID Fiscal"
              placeholder="Ej. XEXX010101000"
            />
          </div>

          {/* Categoría y estado */}
          <div className="grid grid-cols-2 gap-3">
            <Select controlKey="ui.components.modules.clients.client.form.select.1"
              label="Categoría"
              value={categoryId ?? ""}
              onValueChange={(v) => setValue("categoryId", v)}
            >
              <SelectItem value="">Sin categoría</SelectItem>
              {categories.map((c) => (
                <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>
              ))}
            </Select>
            <Select controlKey="ui.components.modules.clients.client.form.select.2"
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
              <button data-ui-control="ui.components.modules.clients.client.form.button.3"
                type="button"
                onClick={() => setTagPickerOpen(!tagPickerOpen)}
                className="inline-flex items-center gap-1 rounded-full border border-dashed border-border px-2.5 py-0.5 text-[11px] text-muted hover:border-action hover:text-action transition-colors"
              >
                <Tag className="h-3 w-3" />
                Editar
              </button>
            </div>
            {tagPickerOpen && (
              <div className="mt-2 rounded-table border border-border bg-surface p-2 shadow-dp1">
                <div className="flex flex-wrap gap-1.5">
                  {allTags.map((tag) => (
                    <button data-ui-control="ui.components.modules.clients.client.form.button.4"
                      key={tag.id}
                      type="button"
                      onClick={() => toggleTag(tag.id)}
                      className={cn(
                        "flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-semibold transition-colors",
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
          <Input controlKey="ui.components.modules.clients.client.form.input.4"
            {...register("billingEmail")}
            label="Correo de facturación"
            type="email"
            placeholder="facturacion@empresa.com"
            error={errors.billingEmail?.message}
          />
          <div className="grid grid-cols-2 gap-3">
            <Input controlKey="ui.components.modules.clients.client.form.input.5"
              {...register("phone")}
              label="Teléfono"
              placeholder="+52 33 0000 0000"
              inputMode="tel"
            />
          </div>
          <Input controlKey="ui.components.modules.clients.client.form.input.6"
            {...register("billingAddress")}
            label="Dirección de facturación"
            placeholder="Calle, número, ciudad"
          />

          <hr className="border-surface-subtle" />

          {/* Notas internas */}
          <Textarea controlKey="ui.components.modules.clients.client.form.textarea.1"
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
