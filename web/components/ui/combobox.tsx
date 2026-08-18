"use client";

import { useState, useRef } from "react";
import * as Popover from "@radix-ui/react-popover";
import { Check, ChevronDown, Search } from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { useUiControl, type UiControlPermissionProps } from "@/lib/auth/ui-control";

export interface ComboOption {
  value: string;
  label: string;
  description?: string;
}

interface ComboboxProps extends UiControlPermissionProps {
  value: string;
  onValueChange: (value: string) => void;
  options: ComboOption[];
  placeholder?: string;
  searchPlaceholder?: string;
  label?: string;
  error?: string;
  disabled?: boolean;
}

export function Combobox({
  value,
  onValueChange,
  options,
  placeholder = "Seleccionar…",
  searchPlaceholder = "Buscar…",
  label,
  error,
  disabled,
  controlKey,
  permission,
  systemRequired,
}: ComboboxProps) {
  const access = useUiControl({ controlKey, permission, systemRequired });
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  const selected = options.find((o) => o.value === value);
  if (access.hidden) return null;

  const filtered =
    query.trim() === ""
      ? options
      : options.filter(
          (o) =>
            o.label.toLowerCase().includes(query.toLowerCase()) ||
            o.description?.toLowerCase().includes(query.toLowerCase())
        );

  const handleSelect = (optValue: string) => {
    onValueChange(optValue === value ? "" : optValue);
    setOpen(false);
    setQuery("");
  };

  return (
    <div className="flex flex-col gap-1">
      {label && (
        <span className="text-[11px] font-semibold uppercase tracking-wider text-muted">
          {label}
        </span>
      )}
      <Popover.Root
        open={open}
        onOpenChange={(v) => {
          setOpen(v);
          if (v) setTimeout(() => inputRef.current?.focus(), 30);
          else setQuery("");
        }}
      >
        <Popover.Trigger asChild>
          <button
            type="button"
            disabled={disabled || access.disabled || access.readOnly}
            {...access.dataAttributes}
            className={cn(
              "flex min-h-11 w-full items-center justify-between rounded-input border px-3 text-[13px] transition-colors",
              "focus:outline-none focus-visible:ring-2 focus-visible:ring-action",
              "disabled:cursor-not-allowed disabled:bg-surface-subtle",
              error
                ? "border-danger bg-surface text-foreground"
                : "border-border bg-surface text-foreground hover:border-action",
              !selected && "text-muted"
            )}
          >
            <span className="truncate">{selected ? selected.label : placeholder}</span>
            <ChevronDown className="h-3.5 w-3.5 shrink-0 text-muted" />
          </button>
        </Popover.Trigger>

        <Popover.Portal>
          <Popover.Content
            className="z-50 w-[var(--radix-popover-trigger-width)] overflow-hidden rounded-table border border-border bg-surface shadow-dp2"
            sideOffset={4}
            align="start"
          >
            <div className="flex items-center gap-2 border-b border-border px-3 py-2">
              <Search className="h-3.5 w-3.5 shrink-0 text-muted" />
              <input
                ref={inputRef}
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                placeholder={searchPlaceholder}
                className="flex-1 bg-transparent text-[13px] text-foreground placeholder:text-muted focus:outline-none"
              />
            </div>
            <div className="max-h-52 overflow-y-auto p-1">
              {filtered.length === 0 ? (
                <p className="px-3 py-4 text-center text-[12px] text-muted">Sin resultados.</p>
              ) : (
                filtered.map((opt) => (
                  <button
                    key={opt.value}
                    type="button"
                    onClick={() => handleSelect(opt.value)}
                    className="flex w-full items-center justify-between gap-2 rounded px-3 py-1.5 text-left text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:bg-surface-subtle focus:outline-none"
                  >
                    <span className="min-w-0">
                      <span className="block truncate">{opt.label}</span>
                      {opt.description && (
                        <span className="block truncate text-[11px] text-muted">
                          {opt.description}
                        </span>
                      )}
                    </span>
                    {opt.value === value && (
                      <Check className="h-3.5 w-3.5 shrink-0 text-action" />
                    )}
                  </button>
                ))
              )}
            </div>
          </Popover.Content>
        </Popover.Portal>
      </Popover.Root>
      {error && <p className="text-[11px] font-medium text-danger">{error}</p>}
    </div>
  );
}
