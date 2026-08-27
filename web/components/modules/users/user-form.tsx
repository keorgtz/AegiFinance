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
import { useClients } from "@/hooks/use-clients";
import type { UserDto } from "@/types/api";

const createSchema = z.object({
  name: z.string().min(1, "Requerido"),
  userName: z.string().min(3, "Mínimo 3 caracteres"),
  email: z.string().email("Correo inválido"),
  password: z.string().min(8, "Mínimo 8 caracteres").regex(/[A-Z]/, "Incluí una mayúscula").regex(/[a-z]/, "Incluí una minúscula").regex(/[0-9]/, "Incluí un número").regex(/[^a-zA-Z0-9]/, "Incluí un símbolo"),
  userType: z.enum(["Administrator", "Client"] as const),
  clientId: z.string().optional(),
}).superRefine((value, context) => { if (value.userType === "Client" && !value.clientId) context.addIssue({ code: "custom", path: ["clientId"], message: "Seleccioná un cliente" }); });

const updateSchema = z.object({
  name: z.string().min(1, "Requerido"),
  email: z.string().email("Correo inválido"),
  userType: z.enum(["Administrator", "Client"] as const),
  clientId: z.string().optional(),
}).superRefine((value, context) => { if (value.userType === "Client" && !value.clientId) context.addIssue({ code: "custom", path: ["clientId"], message: "Seleccioná un cliente" }); });

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
  const { data: clients } = useClients({ pageSize: 200 });

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
          clientId: editingUser.clientId ?? "",
        });
      } else {
        reset({ name: "", userName: "", email: "", password: "", userType: "Administrator", clientId: "" });
      }
    }
  }, [open, editingUser, reset]);

  const onSubmit = async (values: CreateValues) => {
    try {
      if (isEdit && editingUser) {
        await updateUser.mutateAsync({
        id: editingUser.id,
        data: {
          name: values.name,
          email: values.email,
          userType: values.userType,
          clientId: values.userType === "Client" ? values.clientId : null,
        },
        });
      } else {
        await createUser.mutateAsync({
          ...values,
          clientId: values.userType === "Client" ? values.clientId : null,
        });
      }
      onOpenChange(false);
    } catch {
      // The mutation error remains visible in the dialog.
    }
  };

  const mutationError = createUser.error ?? updateUser.error;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        title={isEdit ? "Editar usuario" : "Nuevo usuario"}
        description={isEdit ? `Editando ${editingUser?.userName}` : undefined}
      >
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {mutationError && <p role="alert" className="rounded-input bg-danger-soft p-3 text-sm font-medium text-danger">{mutationError.message || "No se pudo guardar el usuario."}</p>}
          <Input controlKey="ui.components.modules.users.user.form.input.1"
            permission="ManageUsers"
            {...register("name")}
            label="Nombre completo"
            placeholder="Ej. Juan Pérez"
            autoFocus
            error={errors.name?.message}
          />

          {!isEdit && (
            <>
              <Input controlKey="ui.components.modules.users.user.form.input.2"
                permission="ManageUsers"
                {...register("userName")}
                label="Nombre de usuario"
                placeholder="Ej. jperez"
                autoComplete="off"
                error={(errors as { userName?: { message?: string } }).userName?.message}
              />
              <Input controlKey="ui.components.modules.users.user.form.input.3"
                permission="ManageUsers"
                {...register("password")}
                label="Contraseña"
                type="password"
                placeholder="••••••••"
                autoComplete="new-password"
                error={(errors as { password?: { message?: string } }).password?.message}
              />
            </>
          )}

          <Input controlKey="ui.components.modules.users.user.form.input.4"
            permission="ManageUsers"
            {...register("email")}
            label="Correo electrónico"
            type="email"
            placeholder="correo@empresa.com"
            error={errors.email?.message}
          />

          <Select controlKey="ui.components.modules.users.user.form.select.1"
            permission="ManageUsers"
            label="Tipo de usuario"
            value={userType}
            onValueChange={(v) => setValue("userType", v as "Administrator" | "Client")}
            error={errors.userType?.message}
          >
            <SelectItem value="Administrator">Administrador</SelectItem>
            <SelectItem value="Client">Cliente</SelectItem>
          </Select>

          {userType === "Client" && (
            <Select controlKey="users.form.client" permission="ManageUsers" label="Cliente" value={watch("clientId") || ""} onValueChange={(value) => setValue("clientId", value)} error={(errors as { clientId?: { message?: string } }).clientId?.message} placeholder="Seleccioná un cliente">
              {clients?.items.map((client) => <SelectItem key={client.id} value={client.id}>{client.name}</SelectItem>)}
            </Select>
          )}

          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.users.user.form.button.1" systemRequired type="button" variant="secondary">
                Cancelar
              </Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.users.user.form.button.2" permission="ManageUsers" type="submit" loading={isSubmitting}>
              {isEdit ? "Guardar cambios" : "Crear usuario"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
