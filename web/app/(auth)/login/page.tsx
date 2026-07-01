"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useAuth } from "@/lib/auth/context";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ApiRequestError } from "@/lib/api/client";
import { Eye, EyeOff, Lock, User } from "lucide-react";

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
    <div className="w-full max-w-sm">
      {/* Card */}
      <div className="rounded-card bg-white shadow-dp3 overflow-hidden">
        {/* Header con color primario */}
        <div className="bg-[#0F5C6B] px-8 py-6">
          <h1 className="font-display text-[22px] font-700 text-white tracking-tight">
            Aegi<span className="text-[#5BAEBC]">Finance</span>
          </h1>
          <p className="mt-1 text-[12px] text-white/60">
            Plataforma de gestión financiera
          </p>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="px-8 py-6 space-y-4">
          <Input
            {...register("usernameOrEmail")}
            label="Usuario o correo"
            placeholder="usuario@empresa.com"
            autoComplete="username"
            autoFocus
            error={errors.usernameOrEmail?.message}
            leftIcon={<User className="h-3.5 w-3.5" />}
          />

          <div className="relative">
            <Input
              {...register("password")}
              label="Contraseña"
              type={showPassword ? "text" : "password"}
              placeholder="••••••••"
              autoComplete="current-password"
              error={errors.password?.message}
              leftIcon={<Lock className="h-3.5 w-3.5" />}
            />
            <button
              type="button"
              onClick={() => setShowPassword((v) => !v)}
              className="absolute right-2.5 bottom-2 text-[#5B6472] hover:text-[#16181D] transition-colors"
              aria-label={showPassword ? "Ocultar contraseña" : "Mostrar contraseña"}
              tabIndex={-1}
            >
              {showPassword ? (
                <EyeOff className="h-3.5 w-3.5" />
              ) : (
                <Eye className="h-3.5 w-3.5" />
              )}
            </button>
          </div>

          {serverError && (
            <div className="rounded-input bg-[#FFF6F1] border border-[#FBE6DC] px-3 py-2 text-[12px] text-[#B6452C]">
              {serverError}
            </div>
          )}

          <Button
            type="submit"
            className="w-full mt-2"
            size="lg"
            loading={isSubmitting}
          >
            Iniciar sesión
          </Button>
        </form>
      </div>

      <p className="mt-4 text-center text-[11px] text-[#5B6472]">
        Keorsoft · AegiFinance v0.1
      </p>
    </div>
  );
}
