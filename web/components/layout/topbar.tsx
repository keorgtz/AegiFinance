"use client";

import Link from "next/link";
import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import { LogOut, UserRound } from "lucide-react";
import { useAuth } from "@/lib/auth/context";
import { Avatar } from "@/components/ui/avatar";
import { Brand } from "@/components/ui/brand";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import { CommandPalette } from "./command-palette";

export function Topbar() {
  const { user, logout } = useAuth();

  return (
    <header className="app-surface-glass sticky top-0 z-40 flex h-[72px] items-center justify-between gap-3 border-b border-border px-3 sm:px-5 lg:px-8">
      <Link data-ui-control="ui.components.layout.topbar.link.1" href="/dashboard" className="lg:hidden" aria-label="Ir al resumen">
        <Brand compact />
      </Link>

      <div className="hidden lg:block">
        <p className="text-xs font-semibold text-muted">Espacio financiero</p>
        <p className="text-sm font-bold text-foreground">Control y conciliación</p>
      </div>

      <CommandPalette />

      <div className="flex items-center gap-2">
        <ThemeToggle compact />
        <DropdownMenu.Root>
          <DropdownMenu.Trigger
            data-ui-control="account.menu.open"
            className="flex min-h-11 items-center gap-2 rounded-full border border-transparent px-1.5 pr-2 transition-ui hover:border-border hover:bg-surface-subtle focus:outline-none"
            aria-label="Abrir menú de cuenta"
          >
            <Avatar name={user?.name ?? user?.userName} size="md" />
            <div className="hidden flex-col items-start sm:flex">
              <span className="max-w-32 truncate text-xs font-bold text-foreground">{user?.name || user?.userName}</span>
              <span className="max-w-32 truncate text-[10px] text-muted">{user?.roles[0] ?? user?.userType}</span>
            </div>
          </DropdownMenu.Trigger>

          <DropdownMenu.Portal>
            <DropdownMenu.Content className="z-50 min-w-[220px] overflow-hidden rounded-table border border-border bg-surface p-2 shadow-dp2" sideOffset={8} align="end">
              <div className="px-3 py-2">
                <p className="text-sm font-bold text-foreground">{user?.name || user?.userName}</p>
                <p className="text-xs text-muted">{user?.email}</p>
              </div>
              <DropdownMenu.Separator className="my-1 h-px bg-border" />
              <DropdownMenu.Item className="flex min-h-11 cursor-pointer items-center gap-2 rounded-input px-3 text-sm text-foreground-secondary outline-none focus:bg-surface-subtle">
                <UserRound className="h-4 w-4" />
                Mi cuenta
              </DropdownMenu.Item>
              <DropdownMenu.Item
                onSelect={() => logout()}
                data-ui-control="session.logout"
                className="flex min-h-11 cursor-pointer items-center gap-2 rounded-input px-3 text-sm font-semibold text-danger outline-none focus:bg-danger-soft"
              >
                <LogOut className="h-4 w-4" />
                Cerrar sesión
              </DropdownMenu.Item>
            </DropdownMenu.Content>
          </DropdownMenu.Portal>
        </DropdownMenu.Root>
      </div>
    </header>
  );
}
