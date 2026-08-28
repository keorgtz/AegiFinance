"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import { useAddContact, useUpdateContact } from "@/hooks/use-clients";
import type { ClientContactDto } from "@/types/api";

const schema = z.object({
  name: z.string().min(1, "Requerido"),
  email: z.string().email("Correo inválido").optional().or(z.literal("")),
  phone: z.string().optional(),
  position: z.string().optional(),
  isPrimary: z.boolean().optional(),
});

type FormValues = z.infer<typeof schema>;

interface ContactFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  clientId: string;
  editingContact?: ClientContactDto | null;
}

export function ContactForm({ open, onOpenChange, clientId, editingContact }: ContactFormProps) {
  const isEdit = !!editingContact;
  const addContact = useAddContact();
  const updateContact = useUpdateContact();

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { isPrimary: false },
  });

  const isPrimary = watch("isPrimary");

  useEffect(() => {
    if (open) {
      reset(
        editingContact
          ? {
              name: editingContact.name,
              email: editingContact.email ?? "",
              phone: editingContact.phone ?? "",
              position: editingContact.position ?? "",
              isPrimary: editingContact.isPrimary,
            }
          : { name: "", email: "", phone: "", position: "", isPrimary: false }
      );
    }
  }, [open, editingContact, reset]);

  const onSubmit = async (values: FormValues) => {
    const data = {
      name: values.name,
      email: values.email || null,
      phone: values.phone || null,
      position: values.position || null,
      isPrimary: values.isPrimary ?? false,
    };

    if (isEdit && editingContact) {
      await updateContact.mutateAsync({ clientId, contactId: editingContact.id, data });
    } else {
      await addContact.mutateAsync({ clientId, data });
    }
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title={isEdit ? "Editar contacto" : "Nuevo contacto"} size="sm">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input controlKey="ui.components.modules.clients.contact.form.input.1"
            {...register("name")}
            label="Nombre *"
            placeholder="Ej. María García"
            autoFocus
            error={errors.name?.message}
          />
          <Input controlKey="ui.components.modules.clients.contact.form.input.2"
            {...register("position")}
            label="Cargo"
            placeholder="Ej. Gerente General"
          />
          <Input controlKey="ui.components.modules.clients.contact.form.input.3"
            {...register("email")}
            label="Correo"
            type="email"
            placeholder="contacto@empresa.com"
            error={errors.email?.message}
          />
          <Input controlKey="ui.components.modules.clients.contact.form.input.4"
            {...register("phone")}
            label="Teléfono"
            placeholder="+52 33 0000 0000"
            inputMode="tel"
          />

          <div className="flex items-center justify-between rounded-input border border-border px-3 py-2">
            <label htmlFor="is-primary" className="text-[13px] font-medium text-foreground-secondary">
              Contacto principal
            </label>
            <Switch
              controlKey="clients.contacts.primary"
              permission="UpdateClients"
              id="is-primary"
              checked={isPrimary ?? false}
              onCheckedChange={(v) => setValue("isPrimary", v, { shouldDirty: true })}
              ariaLabel="Contacto principal"
            />
          </div>

          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.clients.contact.form.button.1" type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.clients.contact.form.button.2" type="submit" loading={isSubmitting}>
              {isEdit ? "Guardar" : "Agregar contacto"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
