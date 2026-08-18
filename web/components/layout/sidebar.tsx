"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { ChevronLeft, ShieldCheck } from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { usePermissions } from "@/lib/auth/use-permissions";
import { NAVIGATION_GROUP_LABELS, NAVIGATION_ITEMS, type NavigationItem } from "@/lib/navigation";
import { useState } from "react";
import { Brand } from "@/components/ui/brand";

const GROUPS: NavigationItem["group"][] = ["overview", "operations", "administration"];

export function Sidebar() {
  const pathname = usePathname();
  const { can } = usePermissions();
  const [collapsed, setCollapsed] = useState(false);
  const visibleItems = NAVIGATION_ITEMS.filter((item) => can(item.permission));

  return (
    <aside
      className={cn(
        "flex h-dvh flex-none flex-col border-r border-border bg-surface transition-[width] duration-200",
        collapsed ? "w-[84px]" : "w-[248px]"
      )}
    >
      <div className={cn("flex h-[72px] items-center border-b border-border px-5", collapsed && "justify-center px-3")}>
        <Brand compact={collapsed} />
      </div>

      <nav className="flex-1 overflow-y-auto px-3 py-5" aria-label="Navegación principal">
        {GROUPS.map((group) => {
          const items = visibleItems.filter((item) => item.group === group);
          if (items.length === 0) return null;

          return (
            <div key={group} className="mb-6 last:mb-0">
              {!collapsed && (
                <p className="mb-2 px-3 text-[11px] font-bold uppercase tracking-[0.12em] text-muted">
                  {NAVIGATION_GROUP_LABELS[group]}
                </p>
              )}
              <div className="grid gap-1">
                {items.map((item) => {
                  const Icon = item.icon;
                  const active = item.href === "/dashboard" ? pathname === item.href : pathname.startsWith(item.href);

                  return (
                    <Link
                      key={item.href}
                      href={item.href}
                      data-ui-control={`navigation.${item.href.slice(1).replaceAll("/", ".")}.open`}
                      title={collapsed ? item.label : undefined}
                      aria-current={active ? "page" : undefined}
                      className={cn(
                        "flex min-h-11 items-center gap-3 rounded-input px-3 text-sm font-semibold transition-ui",
                        active
                          ? "bg-action-soft text-action"
                          : "text-muted hover:bg-surface-subtle hover:text-foreground",
                        collapsed && "justify-center px-0"
                      )}
                    >
                      <Icon className="h-[19px] w-[19px] flex-none" />
                      {!collapsed && <span className="truncate">{item.label}</span>}
                    </Link>
                  );
                })}
              </div>
            </div>
          );
        })}
      </nav>

      <div className="border-t border-border p-3">
        {!collapsed && (
          <div className="mb-3 flex items-start gap-3 rounded-input bg-success-soft p-3 text-success">
            <ShieldCheck className="mt-0.5 h-4 w-4 flex-none" />
            <div>
              <p className="text-xs font-bold">Ledger protegido</p>
              <p className="mt-0.5 text-[11px] leading-4 opacity-80">Movimientos auditables y saldos calculados.</p>
            </div>
          </div>
        )}
        <button
          type="button"
          onClick={() => setCollapsed((current) => !current)}
          data-ui-control="navigation.sidebar.collapse"
          className="flex min-h-11 w-full items-center justify-center gap-2 rounded-input text-sm font-semibold text-muted transition-ui hover:bg-surface-subtle hover:text-foreground"
          aria-label={collapsed ? "Expandir navegación" : "Contraer navegación"}
        >
          <ChevronLeft className={cn("h-4 w-4 transition-transform duration-200", collapsed && "rotate-180")} />
          {!collapsed && <span>Contraer</span>}
        </button>
      </div>
    </aside>
  );
}
