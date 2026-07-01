"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Select, SelectItem } from "@/components/ui/select";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { useCreateUser, useUpdateUser } from "@/hooks/use-users";
import { useRoles } from "@/hooks/use-roles";
import type { UserDto } from "@/types/api";

const createSchema = z.object({
  name: z.string().min(1, "Requerido"),
  userName: z.string().min(3, "Mínimo 3 caracteres"),
  email: z.string().email("Correo inválido"),
  password: z.string().min(6, "Mínimo 6 caracteres"),
  userType: z.enum(["Administrator", "Client"] as const),
});

const updateSchema = z.object({
  name: z.string().min(1, "Requerido"),
  email: z.string().email("Correo inválido"),
  userType: z.enum(["Administrator", "Client"] as const),
});

type CreateValues = z.infer<typeof createSchema>;
type UpdateValues = z.infer<typeof updateSchema>;

interface UserFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  editingUser?: UserDto | null;
}

export function UserForm({ open, onOpenChange, editingUser }: UserFormProps) {
  const isEdit = !!editingUser;
  const createUser = useCreateUser();
  const updateUser = useUpdateUser();

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<CreateValues>({
    resolver: zodResolver(isEdit ? updateSchema : createSchema) as never,
    defaultValues: { userType: "Administrator" },
  });

  const userType = watch("userType");

  useEffect(() => {
    if (open) {
      if (editingUser) {
        reset({
          name: editingUser.name,
          email: editingUser.email,
          userType: editingUser.userType,
          userName: editingUser.userName,
          password: "",
        });
      } else {
        reset({ name: "", userName: "", email: "", password: "", userType: "Administrator" });
      }
    }
  }, [open, editingUser, reset]);

  const onSubmit = async (values: CreateValues) => {
    if (isEdit && editingUser) {
      await updateUser.mutateAsync({
        id: editingUser.id,
        data: {
          name: values.name,
          email: values.email,
          userType: values.userType,
        },
      });
    } else {
      await createUser.mutateAsync(values);
    }
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        title={isEdit ? "Editar usuario" : "Nuevo usuario"}
        description={isEdit ? `Editando ${editingUser?.userName}` : undefined}
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input
            {...register("name")}
            label="Nombre completo"
            placeholder="Ej. Juan Pérez"
            autoFocus
            error={errors.name?.message}
          />

          {!isEdit && (
            <>
              <Input
                {...register("userName")}
                label="Nombre de usuario"
                placeholder="Ej. jperez"
                autoComplete="off"
                error={(errors as { userName?: { message?: string } }).userName?.message}
              />
              <Input
                {...register("password")}
                label="Contraseña"
                type="password"
                placeholder="••••••••"
                autoComplete="new-password"
                error={(errors as { password?: { message?: string } }).password?.message}
              />
            </>
          )}

          <Input
            {...register("email")}
            label="Correo electrónico"
            type="email"
            placeholder="correo@empresa.com"
            error={errors.email?.message}
          />

          <Select
            label="Tipo de usuario"
            value={userType}
            onValueChange={(v) => setValue("userType", v as "Administrator" | "Client")}
            error={errors.userType?.message}
          >
            <SelectItem value="Administrator">Administrador</SelectItem>
            <SelectItem value="Client">Cliente</SelectItem>
          </Select>

          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="secondary">
                Cancelar
              </Button>
            </DialogClose>
            <Button type="submit" loading={isSubmitting}>
              {isEdit ? "Guardar cambios" : "Crear usuario"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
