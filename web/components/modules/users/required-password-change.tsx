"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/lib/auth/context";

const schema = z.object({ currentPassword: z.string().min(1, "Ingresá la contraseña temporal"), newPassword: z.string().min(8, "Mínimo 8 caracteres").regex(/[A-Z]/, "Incluí una mayúscula").regex(/[a-z]/, "Incluí una minúscula").regex(/[0-9]/, "Incluí un número").regex(/[^a-zA-Z0-9]/, "Incluí un símbolo") });

export function RequiredPasswordChange() {
  const { user, logout, changePassword } = useAuth();
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<z.infer<typeof schema>>({ resolver: zodResolver(schema) });
  if (!user?.mustChangePassword) return null;
  return <Dialog open onOpenChange={(next) => { if (!next) void logout(); }}><DialogContent title="Creá tu contraseña" description="La contraseña temporal sólo sirve para este primer acceso." size="sm"><form onSubmit={handleSubmit(({ currentPassword, newPassword }) => changePassword(currentPassword, newPassword))} className="space-y-4"><Input controlKey="security.required-password.current" systemRequired type="password" autoComplete="current-password" autoFocus label="Contraseña temporal" error={errors.currentPassword?.message} {...register("currentPassword")} /><Input controlKey="security.required-password.new" systemRequired type="password" autoComplete="new-password" label="Nueva contraseña" hint="8 caracteres, mayúscula, minúscula, número y símbolo." error={errors.newPassword?.message} {...register("newPassword")} /><DialogFooter><Button controlKey="security.required-password.logout" systemRequired type="button" variant="secondary" onClick={() => logout()}>Cerrar sesión</Button><Button controlKey="security.required-password.save" systemRequired type="submit" loading={isSubmitting}>Guardar contraseña</Button></DialogFooter></form></DialogContent></Dialog>;
}
