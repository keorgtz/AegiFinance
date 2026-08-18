"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useAuth } from "@/lib/auth/context";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Brand } from "@/components/ui/brand";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import { ApiRequestError } from "@/lib/api/client";
import { ArrowRight, Eye, EyeOff, GitCompareArrows, Lock, ShieldCheck, Smartphone, User } from "lucide-react";

const schema = z.object({
  usernameOrEmail: z.string().min(1, "Campo requerido"),
  password: z.string().min(1, "Campo requerido"),
});

type FormValues = z.infer<typeof schema>;

export default function LoginPage() {
  const router = useRouter();
  const { login } = useAuth();
  const [showPassword, setShowPassword] = useState(false);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema) });

  const onSubmit = async (data: FormValues) => {
    setServerError(null);
    try {
      await login(data.usernameOrEmail, data.password);
      router.push("/dashboard");
    } catch (err) {
      if (err instanceof ApiRequestError) {
        setServerError(err.error.message ?? err.error.detail ?? "Credenciales incorrectas.");
      } else {
        setServerError("Error de conexión. Intenta de nuevo.");
      }
    }
  };

  return (
    <>
      <section className="relative hidden overflow-hidden bg-action-soft p-10 lg:flex lg:flex-col lg:justify-between">
        <div className="absolute -right-28 -top-28 h-80 w-80 rounded-full bg-action/15" />
        <div className="absolute -bottom-40 -left-28 h-96 w-96 rounded-full bg-accent/10" />
        <Brand className="relative z-10" />
        <div className="relative z-10 max-w-lg">
          <p className="text-xs font-bold uppercase tracking-[0.14em] text-action">Control financiero sereno</p>
          <h1 className="mt-4 text-4xl font-bold leading-[1.1] tracking-[-0.045em] text-foreground">
            Conciliá pagos con claridad y trazabilidad.
          </h1>
          <p className="mt-5 max-w-md text-base leading-7 text-muted">
            Cuentas, clientes, planes, cargos y abonos conectados a un Major Ledger auditable.
          </p>
          <div className="mt-8 grid gap-3">
            {[
              { icon: GitCompareArrows, text: "Conciliación bancaria guiada" },
              { icon: ShieldCheck, text: "Permisos y auditoría en cada operación" },
              { icon: Smartphone, text: "Misma tarea, bien resuelta en teléfono y computadora" },
            ].map(({ icon: Icon, text }) => (
              <div key={text} className="flex items-center gap-3 text-sm font-semibold text-foreground-secondary">
                <span className="grid h-10 w-10 place-items-center rounded-input bg-surface text-action shadow-dp1">
                  <Icon className="h-5 w-5" />
                </span>
                {text}
              </div>
            ))}
          </div>
        </div>
        <p className="relative z-10 text-xs text-muted">Keorsoft · AegiFinance</p>
      </section>

      <section className="flex min-h-full flex-col p-5 sm:p-8 lg:p-12">
        <div className="flex items-center justify-between lg:justify-end">
          <Brand className="lg:hidden" />
          <ThemeToggle compact />
        </div>

        <div className="my-auto w-full max-w-md self-center py-10">
          <p className="text-xs font-bold uppercase tracking-[0.14em] text-action">Acceso seguro</p>
          <h2 className="mt-3 text-3xl font-bold tracking-[-0.04em] text-foreground">Bienvenido de nuevo</h2>
          <p className="mt-2 text-sm leading-6 text-muted">Ingresá para continuar con tu operación financiera.</p>

          <form onSubmit={handleSubmit(onSubmit)} className="mt-8 space-y-5">
          <Input
            {...register("usernameOrEmail")}
            label="Usuario o correo"
            placeholder="usuario@empresa.com"
            autoComplete="username"
            autoFocus
            error={errors.usernameOrEmail?.message}
            leftIcon={<User className="h-4 w-4" />}
            id="login-identity"
            data-ui-control="login.identity"
          />

          <div className="relative">
            <Input
              {...register("password")}
              label="Contraseña"
              type={showPassword ? "text" : "password"}
              placeholder="••••••••"
              autoComplete="current-password"
              error={errors.password?.message}
              leftIcon={<Lock className="h-4 w-4" />}
              id="login-password"
              data-ui-control="login.password"
            />
            <button
              type="button"
              onClick={() => setShowPassword((v) => !v)}
              data-ui-control="login.password.toggle-visibility"
              className="absolute bottom-0 right-0 grid h-11 w-11 place-items-center rounded-full text-muted transition-ui hover:text-foreground"
              aria-label={showPassword ? "Ocultar contraseña" : "Mostrar contraseña"}
            >
              {showPassword ? (
                <EyeOff className="h-4 w-4" />
              ) : (
                <Eye className="h-4 w-4" />
              )}
            </button>
          </div>

          {serverError && (
            <div role="alert" className="rounded-input border border-danger/20 bg-danger-soft px-4 py-3 text-sm text-danger">
              {serverError}
            </div>
          )}

          <Button
            type="submit"
            className="mt-2 w-full"
            size="lg"
            loading={isSubmitting}
            data-ui-control="login.submit"
          >
            Iniciar sesión
            {!isSubmitting && <ArrowRight className="h-4 w-4" />}
          </Button>
        </form>
          <p className="mt-6 text-center text-xs text-muted">Tus credenciales viajan cifradas y la sesión se renueva de forma segura.</p>
        </div>
      </section>
    </>
  );
}
