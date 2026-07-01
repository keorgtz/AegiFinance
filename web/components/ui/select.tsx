"use client";

import * as RadixSelect from "@radix-ui/react-select";
import { Check, ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils/cn";

interface SelectProps {
  value?: string;
  onValueChange?: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
  label?: string;
  error?: string;
  children: React.ReactNode;
}

export function Select({
  value,
  onValueChange,
  placeholder,
  disabled,
  label,
  error,
  children,
}: SelectProps) {
  return (
    <div className="flex flex-col gap-1">
      {label && (
        <label className="text-[11px] font-600 uppercase tracking-wider text-[#5B6472]">
          {label}
        </label>
      )}
      <RadixSelect.Root value={value} onValueChange={onValueChange} disabled={disabled}>
        <RadixSelect.Trigger
          className={cn(
            "inline-flex h-8 w-full items-center justify-between gap-2 rounded-input",
            "border border-[#E3E6EC] bg-white px-3 text-[13px] text-[#16181D]",
            "transition-colors duration-150",
            "focus:outline-none focus:border-[#5BAEBC] focus:ring-1 focus:ring-[#5BAEBC]",
            "disabled:bg-[#F7F8FA] disabled:cursor-not-allowed disabled:opacity-70",
            "data-[placeholder]:text-[#5B6472]",
            error && "border-[#B6452C]"
          )}
        >
          <RadixSelect.Value placeholder={placeholder} />
          <RadixSelect.Icon>
            <ChevronDown className="h-3.5 w-3.5 text-[#5B6472]" />
          </RadixSelect.Icon>
        </RadixSelect.Trigger>
        <RadixSelect.Portal>
          <RadixSelect.Content
            className="z-50 min-w-[8rem] overflow-hidden rounded-table border border-[#E3E6EC] bg-white shadow-dp2"
            position="popper"
            sideOffset={4}
          >
            <RadixSelect.Viewport className="p-1">
              {children}
            </RadixSelect.Viewport>
          </RadixSelect.Content>
        </RadixSelect.Portal>
      </RadixSelect.Root>
      {error && (
        <p className="text-[11px] text-[#B6452C] font-500">{error}</p>
      )}
    </div>
  );
}

interface SelectItemProps {
  value: string;
  children: React.ReactNode;
}

export function SelectItem({ value, children }: SelectItemProps) {
  return (
    <RadixSelect.Item
      value={value}
      className={cn(
        "relative flex cursor-default select-none items-center rounded-[6px] px-6 py-1.5",
        "text-[13px] text-[#16181D]",
        "focus:bg-[#EFF1F7] focus:outline-none",
        "data-[disabled]:pointer-events-none data-[disabled]:opacity-50"
      )}
    >
      <RadixSelect.ItemIndicator className="absolute left-1.5 inline-flex items-center">
        <Check className="h-3 w-3 text-[#0F5C6B]" />
      </RadixSelect.ItemIndicator>
      <RadixSelect.ItemText>{children}</RadixSelect.ItemText>
    </RadixSelect.Item>
  );
}
