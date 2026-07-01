"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Building2,
  Layers,
  LayoutDashboard,
  MoreHorizontal,
  Wallet,
} from "lucide-react";
import { cn } from "@/lib/utils/cn";

const TABS = [
  { href: "/dashboard", label: "Inicio", icon: LayoutDashboard },
  { href: "/clients", label: "Clientes", icon: Building2 },
  { href: "/subscriptions", label: "Suscrip.", icon: Layers },
  { href: "/ledger", label: "Ledger", icon: Wallet },
  { href: "/more", label: "Más", icon: MoreHorizontal },
];

export function MobileNav() {
  const pathname = usePathname();

  return (
    <nav className="fixed bottom-0 left-0 right-0 z-40 border-t border-[#E3E6EC] bg-white sm:hidden">
      <div className="flex">
        {TABS.map((tab) => {
          const Icon = tab.icon;
          const active = pathname.startsWith(tab.href);
          return (
            <Link
              key={tab.href}
              href={tab.href}
              className="flex flex-1 flex-col items-center justify-center gap-1 py-2 min-h-[56px]"
            >
              <Icon
                className={cn(
                  "h-5 w-5",
                  active ? "text-[#0F5C6B]" : "text-[#5B6472]"
                )}
              />
              <span
                className={cn(
                  "text-[10px] font-600",
                  active ? "text-[#0F5C6B]" : "text-[#5B6472]"
                )}
              >
                {tab.label}
              </span>
            </Link>
          );
        })}
      </div>
    </nav>
  );
}
