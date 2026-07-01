"use client";

import { useAuth } from "@/lib/auth/context";
import { Avatar } from "@/components/ui/avatar";
import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import { CommandPalette } from "./command-palette";
import { LogOut, Settings, User } from "lucide-react";
import Link from "next/link";

export function Topbar() {
  const { user, logout } = useAuth();

  return (
    <header className="sticky top-0 z-40 flex h-14 items-center justify-between border-b border-[#1f2937] bg-[#12141A] px-6">
      {/* Logo */}
      <div className="flex items-center gap-3">
        <span className="font-display text-[18px] font-700 text-white tracking-tight">
          Aegi<span className="text-[#5BAEBC]">Finance</span>
        </span>
      </div>

      {/* Center — Command palette trigger */}
      <CommandPalette />

      {/* Right — Avatar + menu */}
      <div className="flex items-center gap-3">
        <DropdownMenu.Root>
          <DropdownMenu.Trigger
            className="flex items-center gap-2 rounded-button px-2 py-1 hover:bg-white/10 transition-colors focus:outline-none"
            aria-label="Menú de usuario"
          >
            <Avatar name={user?.name ?? user?.userName} size="sm" />
            <div className="hidden sm:flex flex-col items-start">
              <span className="text-[12px] font-600 text-white leading-tight">
                {user?.name || user?.userName}
              </span>
              <span className="text-[10px] text-white/50 leading-tight">
                {user?.roles[0] ?? user?.userType}
              </span>
            </div>
          </DropdownMenu.Trigger>

          <DropdownMenu.Portal>
            <DropdownMenu.Content
              className="z-50 min-w-[180px] overflow-hidden rounded-table border border-[#E3E6EC] bg-white shadow-dp2"
              sideOffset={8}
              align="end"
            >
              <div className="px-3 py-2 border-b border-[#F7F8FA]">
                <p className="text-[13px] font-600 text-[#16181D]">{user?.name}</p>
                <p className="text-[11px] text-[#5B6472]">{user?.email}</p>
              </div>

              <DropdownMenu.Item asChild>
                <Link
                  href="/settings"
                  className="flex items-center gap-2 px-3 py-2 text-[13px] text-[#3A3F4B] hover:bg-[#F7F8FA] cursor-pointer focus:outline-none focus:bg-[#F7F8FA]"
                >
                  <Settings className="h-4 w-4" />
                  Configuración
                </Link>
              </DropdownMenu.Item>

              <DropdownMenu.Item asChild>
                <Link
                  href="/profile"
                  className="flex items-center gap-2 px-3 py-2 text-[13px] text-[#3A3F4B] hover:bg-[#F7F8FA] cursor-pointer focus:outline-none focus:bg-[#F7F8FA]"
                >
                  <User className="h-4 w-4" />
                  Mi perfil
                </Link>
              </DropdownMenu.Item>

              <DropdownMenu.Separator className="my-1 h-px bg-[#F7F8FA]" />

              <DropdownMenu.Item
                onSelect={() => logout()}
                className="flex items-center gap-2 px-3 py-2 text-[13px] text-[#B6452C] hover:bg-[#FFF6F1] cursor-pointer focus:outline-none focus:bg-[#FFF6F1]"
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
