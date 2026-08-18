"use client";

import { Command } from "cmdk";
import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { Search } from "lucide-react";
import { NAVIGATION_ITEMS } from "@/lib/navigation";
import { usePermissions } from "@/lib/auth/use-permissions";

export function CommandPalette() {
  const router = useRouter();
  const { can } = usePermissions();
  const [open, setOpen] = useState(false);
  const commands = useMemo(() => NAVIGATION_ITEMS.filter((item) => can(item.permission)), [can]);

  useEffect(() => {
    const handler = (event: KeyboardEvent) => {
      if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        setOpen((current) => !current);
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
      <button
        type="button"
        onClick={() => setOpen(true)}
        data-ui-control="navigation.command-palette.open"
        className="hidden min-h-11 min-w-[240px] items-center gap-2 rounded-full border border-border bg-surface-subtle px-4 text-sm text-muted transition-ui hover:border-border-strong hover:text-foreground md:flex"
        aria-label="Buscar secciones y acciones"
      >
        <Search className="h-4 w-4" />
        <span>Buscar en AegiFinance</span>
        <kbd className="ml-auto rounded-md border border-border bg-surface px-1.5 py-0.5 text-[10px] font-bold">⌘K</kbd>
      </button>

      {open && (
        <div
          className="fixed inset-0 z-50 flex items-start justify-center bg-black/45 px-3 pt-[12vh] backdrop-blur-[2px]"
          onClick={(event) => event.target === event.currentTarget && setOpen(false)}
        >
          <Command className="w-full max-w-xl overflow-hidden rounded-card border border-border bg-surface shadow-dp3" loop>
            <div className="flex min-h-14 items-center border-b border-border px-4">
              <Search className="h-5 w-5 flex-none text-muted" />
              <Command.Input
                autoFocus
                placeholder="Escribí una sección…"
                className="min-h-14 flex-1 bg-transparent px-3 text-base text-foreground placeholder:text-muted focus:outline-none"
                onKeyDown={(event) => event.key === "Escape" && setOpen(false)}
              />
            </div>
            <Command.List className="max-h-[55vh] overflow-y-auto p-2">
              <Command.Empty className="py-10 text-center text-sm text-muted">No encontramos esa sección.</Command.Empty>
              {commands.map((item) => {
                const Icon = item.icon;
                return (
                  <Command.Item
                    key={item.href}
                    value={item.label}
                    onSelect={() => handleSelect(item.href)}
                    className="flex min-h-12 cursor-pointer items-center gap-3 rounded-input px-3 text-sm font-semibold text-foreground-secondary data-[selected=true]:bg-action-soft data-[selected=true]:text-action"
                  >
                    <Icon className="h-[18px] w-[18px]" />
                    {item.label}
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
