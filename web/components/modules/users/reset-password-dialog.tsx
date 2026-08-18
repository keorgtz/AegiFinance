"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useResetUserPassword } from "@/hooks/use-users";
import type { UserDto } from "@/types/api";

const schema = z.object({ temporaryPassword: z.string().min(8, "Mínimo 8 caracteres").regex(/[A-Z]/, "Incluí una mayúscula").regex(/[a-z]/, "Incluí una minúscula").regex(/[0-9]/, "Incluí un número").regex(/[^a-zA-Z0-9]/, "Incluí un símbolo") });

export function ResetPasswordDialog({ open, onOpenChange, user }: { open: boolean; onOpenChange: (open: boolean) => void; user: UserDto | null }) {
  const resetPassword = useResetUserPassword();
  const { register, handleSubmit, reset, formState: { errors } } = useForm<z.infer<typeof schema>>({ resolver: zodResolver(schema) });
  useEffect(() => { if (open) reset({ temporaryPassword: "" }); }, [open, reset]);
  return <Dialog open={open} onOpenChange={onOpenChange}><DialogContent title="Restablecer contraseña" description={user ? `${user.name} deberá cambiarla al ingresar.` : undefined} size="sm"><form onSubmit={handleSubmit(async ({ temporaryPassword }) => { if (!user) return; await resetPassword.mutateAsync({ id: user.id, temporaryPassword }); onOpenChange(false); })}><Input controlKey="users.reset-password.value" permission="ManageUsers" type="password" autoComplete="new-password" autoFocus label="Contraseña temporal" hint="8 caracteres, mayúscula, minúscula, número y símbolo." error={errors.temporaryPassword?.message} {...register("temporaryPassword")} /><DialogFooter><DialogClose asChild><Button controlKey="users.reset-password.cancel" systemRequired type="button" variant="secondary">Cancelar</Button></DialogClose><Button controlKey="users.reset-password.save" permission="ManageUsers" type="submit" loading={resetPassword.isPending}>Restablecer</Button></DialogFooter></form></DialogContent></Dialog>;
}
