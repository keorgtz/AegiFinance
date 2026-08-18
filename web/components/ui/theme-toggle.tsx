"use client";

import { Moon, Sun } from "lucide-react";
import { useTheme } from "@/lib/theme/context";

export function ThemeToggle({ compact = false }: { compact?: boolean }) {
  const { theme, toggleTheme } = useTheme();
  const isDark = theme === "dark";

  return (
    <button
      type="button"
      onClick={toggleTheme}
      data-ui-control="appearance.theme.toggle"
      className="inline-flex min-h-11 min-w-11 items-center justify-center gap-2 rounded-full border border-border bg-surface-subtle px-3 text-muted transition-ui hover:border-border-strong hover:text-foreground"
      aria-label={isDark ? "Usar tema claro" : "Usar tema oscuro"}
      title={isDark ? "Tema claro" : "Tema oscuro"}
    >
      {isDark ? <Sun className="h-[18px] w-[18px]" /> : <Moon className="h-[18px] w-[18px]" />}
      {!compact && <span className="hidden text-sm font-semibold sm:inline">{isDark ? "Claro" : "Oscuro"}</span>}
    </button>
  );
}
