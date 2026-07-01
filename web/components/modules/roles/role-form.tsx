"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Select, SelectItem } from "@/components/ui/select";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { useCreateRole, useUpdateRole } from "@/hooks/use-roles";
import type { RoleDto } from "@/types/api";

const schema = z.object({
  name: z.string().min(1, "Requerido"),
  description: z.string().optional(),
  userType: z.enum(["Administrator", "Client", ""]).optional(),
});

type FormValues = z.infer<typeof schema>;

interface RoleFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  editingRole?: RoleDto | null;
}

export function RoleForm({ open, onOpenChange, editingRole }: RoleFormProps) {
  const isEdit = !!editingRole;
  const createRole = useCreateRole();
  const updateRole = useUpdateRole();

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: "", description: "", userType: "" },
  });

  const userType = watch("userType");

  useEffect(() => {
    if (open) {
      reset(
        editingRole
          ? {
              name: editingRole.name,
              description: editingRole.description ?? "",
              userType: (editingRole.userType as "" | "Administrator" | "Client") ?? "",
            }
          : { name: "", description: "", userType: "" }
      );
    }
  }, [open, editingRole, reset]);

  const onSubmit = async (values: FormValues) => {
    const payload = {
      name: values.name,
      description: values.description || null,
      userType: values.userType || null,
    };

    if (isEdit && editingRole) {
      await updateRole.mutateAsync({ id: editingRole.id, data: payload });
    } else {
      await createRole.mutateAsync(payload);
    }
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title={isEdit ? "Editar rol" : "Nuevo rol"} size="sm">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input
            {...register("name")}
            label="Nombre del rol"
            placeholder="Ej. Admin, Supervisor"
            autoFocus
            error={errors.name?.message}
          />
          <Input
            {...register("description")}
            label="Descripción (opcional)"
            placeholder="Ej. Acceso total al sistema"
          />
          <Select
            label="Aplica para (opcional)"
            value={userType ?? ""}
            onValueChange={(v) => setValue("userType", v as "" | "Administrator" | "Client")}
          >
            <SelectItem value="">Sin restricción</SelectItem>
            <SelectItem value="Administrator">Solo administradores</SelectItem>
            <SelectItem value="Client">Solo clientes</SelectItem>
          </Select>

          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button type="submit" loading={isSubmitting}>
              {isEdit ? "Guardar" : "Crear rol"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
