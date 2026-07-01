"use client";

import { Command } from "cmdk";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import {
  Building2,
  CreditCard,
  Layers,
  LayoutDashboard,
  Package,
  Receipt,
  Search,
  Settings,
  Shield,
  Users,
  Wallet,
} from "lucide-react";

const COMMANDS = [
  { label: "Dashboard", icon: LayoutDashboard, href: "/dashboard" },
  { label: "Clientes", icon: Building2, href: "/clients" },
  { label: "Servicios", icon: Package, href: "/services" },
  { label: "Suscripciones", icon: Layers, href: "/subscriptions" },
  { label: "Facturación", icon: Receipt, href: "/billing" },
  { label: "Ledger", icon: Wallet, href: "/ledger" },
  { label: "Asignaciones", icon: CreditCard, href: "/allocations" },
  { label: "Usuarios", icon: Users, href: "/users" },
  { label: "Roles y Permisos", icon: Shield, href: "/roles" },
  { label: "Configuración", icon: Settings, href: "/settings" },
];

export function CommandPalette() {
  const router = useRouter();
  const [open, setOpen] = useState(false);

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if ((e.metaKey || e.ctrlKey) && e.key === "k") {
        e.preventDefault();
        setOpen((o) => !o);
      }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, []);

  const handleSelect = (href: string) => {
    router.push(href);
    setOpen(false);
  };

  return (
    <>
      {/* Trigger button visible en topbar */}
      <button
        onClick={() => setOpen(true)}
        className="hidden sm:flex items-center gap-2 h-8 rounded-input border border-white/20 bg-white/10 px-3 text-[12px] text-white/60 hover:bg-white/15 transition-colors focus:outline-none focus:ring-1 focus:ring-[#5BAEBC]"
        aria-label="Abrir paleta de comandos"
      >
        <Search className="h-3.5 w-3.5" />
        <span>Buscar…</span>
        <kbd className="ml-2 rounded bg-white/10 px-1.5 py-0.5 text-[10px] font-600">
          ⌘K
        </kbd>
      </button>

      {open && (
        <div
          className="fixed inset-0 z-50 flex items-start justify-center pt-[20vh] bg-black/40"
          onClick={(e) => {
            if (e.target === e.currentTarget) setOpen(false);
          }}
        >
          <Command
            className="w-full max-w-lg overflow-hidden rounded-card bg-white shadow-dp3"
            loop
          >
            <div className="flex items-center border-b border-[#E3E6EC] px-4">
              <Search className="h-4 w-4 text-[#5B6472] flex-shrink-0" />
              <Command.Input
                autoFocus
                placeholder="Navegar a…"
                className="flex-1 bg-transparent py-3 px-3 text-[13px] text-[#16181D] placeholder:text-[#5B6472] focus:outline-none"
                onKeyDown={(e) => {
                  if (e.key === "Escape") setOpen(false);
                }}
              />
            </div>
            <Command.List className="max-h-80 overflow-y-auto p-2">
              <Command.Empty className="py-8 text-center text-[13px] text-[#5B6472]">
                Sin resultados.
              </Command.Empty>
              {COMMANDS.map((cmd) => {
                const Icon = cmd.icon;
                return (
                  <Command.Item
                    key={cmd.href}
                    value={cmd.label}
                    onSelect={() => handleSelect(cmd.href)}
                    className="flex items-center gap-3 rounded-[8px] px-3 py-2 text-[13px] text-[#3A3F4B] cursor-pointer data-[selected=true]:bg-[#EFF1F7] data-[selected=true]:text-[#16181D]"
                  >
                    <Icon className="h-[18px] w-[18px] text-[#5B6472]" />
                    {cmd.label}
                  </Command.Item>
                );
              })}
            </Command.List>
          </Command>
        </div>
      )}
    </>
  );
}
