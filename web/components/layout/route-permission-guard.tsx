"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { ShieldAlert } from "lucide-react";
import { NAVIGATION_ITEMS } from "@/lib/navigation";
import { usePermissions } from "@/lib/auth/use-permissions";

export function RoutePermissionGuard({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const { can } = usePermissions();
  const route = NAVIGATION_ITEMS
    .filter((item) => pathname === item.href || pathname.startsWith(`${item.href}/`))
    .sort((left, right) => right.href.length - left.href.length)[0];

  if (route && !can(route.permission)) {
    return (
      <div className="mx-auto flex min-h-[60vh] max-w-lg flex-col items-center justify-center text-center">
        <div className="mb-4 grid h-14 w-14 place-items-center rounded-full bg-danger-soft text-danger"><ShieldAlert className="h-6 w-6" /></div>
        <h1 className="font-display text-xl font-bold text-foreground">Acceso restringido</h1>
        <p className="mt-2 text-sm text-muted">Tu rol no tiene permiso para abrir este módulo.</p>
        <Link data-ui-control="security.access-denied.return" href="/dashboard" className="mt-5 inline-flex min-h-11 items-center rounded-full bg-action px-5 text-sm font-semibold text-on-action">
          Volver al resumen
        </Link>
      </div>
    );
  }

  return children;
}
