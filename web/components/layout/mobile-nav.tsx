"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Menu, X } from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { NAVIGATION_ITEMS } from "@/lib/navigation";
import { usePermissions } from "@/lib/auth/use-permissions";
import { useState } from "react";

export function MobileNav() {
  const pathname = usePathname();
  const { can } = usePermissions();
  const [moreOpen, setMoreOpen] = useState(false);
  const visible = NAVIGATION_ITEMS.filter((item) => can(item.permission));
  const preferredPrimaryRoutes = ["/dashboard", "/clients", "/ledger", "/billing"];
  const primary = preferredPrimaryRoutes
    .map((href) => visible.find((item) => item.href === href))
    .filter((item): item is (typeof visible)[number] => Boolean(item));
  const overflow = visible.filter((item) => !primary.some((primaryItem) => primaryItem.href === item.href));

  return (
    <>
      {moreOpen && (
        <div className="fixed inset-0 z-40 bg-black/35 backdrop-blur-[2px] lg:hidden" onClick={() => setMoreOpen(false)}>
          <div className="absolute inset-x-3 bottom-[84px] max-h-[65vh] overflow-y-auto rounded-hero border border-border bg-surface p-3 shadow-dp3" onClick={(event) => event.stopPropagation()}>
            <div className="mb-2 flex items-center justify-between px-2">
              <p className="text-sm font-bold text-foreground">Todas las secciones</p>
              <button data-ui-control="ui.components.layout.mobile.nav.button.1" type="button" onClick={() => setMoreOpen(false)} className="grid h-11 w-11 place-items-center rounded-full text-muted hover:bg-surface-subtle" aria-label="Cerrar menú">
                <X className="h-5 w-5" />
              </button>
            </div>
            <div className="grid gap-1">
              {overflow.map((item) => {
                const Icon = item.icon;
                const active = pathname.startsWith(item.href);
                return (
                  <Link data-ui-control="ui.components.layout.mobile.nav.link.1" key={item.href} href={item.href} onClick={() => setMoreOpen(false)} className={cn("flex min-h-12 items-center gap-3 rounded-input px-3 text-sm font-semibold", active ? "bg-action-soft text-action" : "text-foreground-secondary hover:bg-surface-subtle")}>
                    <Icon className="h-5 w-5" />
                    {item.label}
                  </Link>
                );
              })}
            </div>
          </div>
        </div>
      )}

      <nav className="app-surface-glass fixed inset-x-0 bottom-0 z-50 border-t border-border px-1 pb-[max(6px,env(safe-area-inset-bottom))] pt-1 lg:hidden" aria-label="Navegación móvil">
        <div className="mx-auto flex max-w-lg">
          {primary.map((item) => {
            const Icon = item.icon;
            const active = item.href === "/dashboard" ? pathname === item.href : pathname.startsWith(item.href);
            return (
              <Link data-ui-control="ui.components.layout.mobile.nav.link.2" key={item.href} href={item.href} className={cn("flex min-h-16 flex-1 flex-col items-center justify-center gap-1 rounded-input px-1 text-[10px] font-bold", active ? "text-action" : "text-muted")}>
                <Icon className="h-5 w-5" />
                <span className="max-w-full truncate">{item.shortLabel ?? item.label}</span>
              </Link>
            );
          })}
          {overflow.length > 0 && (
            <button type="button" onClick={() => setMoreOpen((current) => !current)} data-ui-control="navigation.mobile.more" className={cn("flex min-h-16 flex-1 flex-col items-center justify-center gap-1 rounded-input px-1 text-[10px] font-bold", moreOpen ? "text-action" : "text-muted")}>
              <Menu className="h-5 w-5" />
              <span>Más</span>
            </button>
          )}
        </div>
      </nav>
    </>
  );
}
