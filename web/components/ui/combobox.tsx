"use client";

import { useState, useRef } from "react";
import * as Popover from "@radix-ui/react-popover";
import { Check, ChevronDown, Search } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export interface ComboOption {
  value: string;
  label: string;
  description?: string;
}

interface ComboboxProps {
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
}: ComboboxProps) {
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  const selected = options.find((o) => o.value === value);

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
        <span className="text-[11px] font-600 uppercase tracking-wider text-[#5B6472]">
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
            disabled={disabled}
            className={cn(
              "flex h-9 w-full items-center justify-between rounded-input border px-3 text-[13px] transition-colors",
              "focus:outline-none focus-visible:ring-2 focus-visible:ring-[#5BAEBC]",
              "disabled:cursor-not-allowed disabled:bg-[#F7F8FA]",
              error
                ? "border-[#B6452C] bg-white text-[#16181D]"
                : "border-[#E3E6EC] bg-white text-[#16181D] hover:border-[#5BAEBC]",
              !selected && "text-[#5B6472]"
            )}
          >
            <span className="truncate">{selected ? selected.label : placeholder}</span>
            <ChevronDown className="h-3.5 w-3.5 shrink-0 text-[#5B6472]" />
          </button>
        </Popover.Trigger>

        <Popover.Portal>
          <Popover.Content
            className="z-50 w-[var(--radix-popover-trigger-width)] overflow-hidden rounded-table border border-[#E3E6EC] bg-white shadow-dp2"
            sideOffset={4}
            align="start"
          >
            <div className="flex items-center gap-2 border-b border-[#E3E6EC] px-3 py-2">
              <Search className="h-3.5 w-3.5 shrink-0 text-[#5B6472]" />
              <input
                ref={inputRef}
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                placeholder={searchPlaceholder}
                className="flex-1 bg-transparent text-[13px] text-[#16181D] placeholder:text-[#5B6472] focus:outline-none"
              />
            </div>
            <div className="max-h-52 overflow-y-auto p-1">
              {filtered.length === 0 ? (
                <p className="px-3 py-4 text-center text-[12px] text-[#5B6472]">Sin resultados.</p>
              ) : (
                filtered.map((opt) => (
                  <button
                    key={opt.value}
                    type="button"
                    onClick={() => handleSelect(opt.value)}
                    className="flex w-full items-center justify-between gap-2 rounded px-3 py-1.5 text-left text-[13px] text-[#3A3F4B] hover:bg-[#F7F8FA] focus:bg-[#F7F8FA] focus:outline-none"
                  >
                    <span className="min-w-0">
                      <span className="block truncate">{opt.label}</span>
                      {opt.description && (
                        <span className="block truncate text-[11px] text-[#5B6472]">
                          {opt.description}
                        </span>
                      )}
                    </span>
                    {opt.value === value && (
                      <Check className="h-3.5 w-3.5 shrink-0 text-[#0F5C6B]" />
                    )}
                  </button>
                ))
              )}
            </div>
          </Popover.Content>
        </Popover.Portal>
      </Popover.Root>
      {error && <p className="text-[11px] font-500 text-[#B6452C]">{error}</p>}
    </div>
  );
}
