"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  BarChart3,
  Building2,
  ChevronLeft,
  CreditCard,
  FileText,
  Layers,
  LayoutDashboard,
  Package,
  Receipt,
  Settings,
  Shield,
  Users,
  Wallet,
} from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { usePermissions } from "@/lib/auth/use-permissions";
import { useState } from "react";

interface NavItem {
  href: string;
  label: string;
  icon: React.ElementType;
  permission?: string;
  adminOnly?: boolean;
}

const NAV_ITEMS: NavItem[] = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/clients", label: "Clientes", icon: Building2, permission: "ViewClients" },
  { href: "/services", label: "Servicios", icon: Package },
  { href: "/subscriptions", label: "Suscripciones", icon: Layers, permission: "ViewSubscriptions" },
  { href: "/billing", label: "Facturación", icon: Receipt, permission: "ViewPayments" },
  { href: "/ledger", label: "Ledger", icon: Wallet, permission: "ViewPayments" },
  { href: "/allocations", label: "Asignaciones", icon: CreditCard, permission: "ViewPayments" },
  { href: "/account-statement", label: "Estado de Cuenta", icon: FileText, permission: "ViewPayments" },
  { href: "/reports", label: "Reportes", icon: BarChart3, permission: "ViewReports" },
  { href: "/users", label: "Usuarios", icon: Users, permission: "ManageUsers" },
  { href: "/roles", label: "Roles y Permisos", icon: Shield, permission: "ManageRoles" },
  { href: "/settings", label: "Configuración", icon: Settings, adminOnly: true },
];

export function Sidebar() {
  const pathname = usePathname();
  const { can, isAdmin } = usePermissions();
  const [collapsed, setCollapsed] = useState(false);

  const visibleItems = NAV_ITEMS.filter((item) => {
    if (item.adminOnly) return isAdmin();
    if (item.permission) return can(item.permission);
    return true;
  });

  return (
    <aside
      className={cn(
        "flex h-full flex-col border-r border-[#E3E6EC] bg-white transition-all duration-200",
        collapsed ? "w-[76px]" : "w-[248px]"
      )}
    >
      {/* Nav items */}
      <nav className="flex-1 overflow-y-auto py-3 px-2">
        {visibleItems.map((item) => {
          const Icon = item.icon;
          const active =
            item.href === "/dashboard"
              ? pathname === "/dashboard"
              : pathname.startsWith(item.href);

          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 rounded-[10px] px-3 py-2 mb-0.5",
                "text-[13px] font-500 transition-colors duration-100",
                active
                  ? "bg-[#C9E8ED] text-[#0F5C6B] font-600"
                  : "text-[#5B6472] hover:bg-[#F7F8FA] hover:text-[#16181D]"
              )}
            >
              <Icon
                className={cn(
                  "h-[18px] w-[18px] flex-shrink-0",
                  active ? "text-[#0F5C6B]" : "text-[#5B6472]"
                )}
              />
              {!collapsed && <span className="truncate">{item.label}</span>}
            </Link>
          );
        })}
      </nav>

      {/* Collapse toggle */}
      <div className="border-t border-[#E3E6EC] p-2">
        <button
          onClick={() => setCollapsed((c) => !c)}
          className="flex w-full items-center justify-center rounded-[10px] p-2 text-[#5B6472] hover:bg-[#F7F8FA] transition-colors"
          aria-label={collapsed ? "Expandir sidebar" : "Colapsar sidebar"}
        >
          <ChevronLeft
            className={cn(
              "h-4 w-4 transition-transform duration-200",
              collapsed && "rotate-180"
            )}
          />
          {!collapsed && (
            <span className="ml-2 text-[12px] font-500">Colapsar</span>
          )}
        </button>
      </div>
    </aside>
  );
}
