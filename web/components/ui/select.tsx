"use client";

import * as RadixSelect from "@radix-ui/react-select";
import { Check, ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { useUiControl, type UiControlPermissionProps } from "@/lib/auth/ui-control";

interface SelectProps extends UiControlPermissionProps {
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
  controlKey,
  permission,
  systemRequired,
}: SelectProps) {
  const access = useUiControl({ controlKey, permission, systemRequired });
  if (access.hidden) return null;
  return (
    <div className="flex flex-col gap-2" {...access.dataAttributes}>
      {label && (
        <label className="text-xs font-semibold text-muted">
          {label}
        </label>
      )}
      <RadixSelect.Root value={value} onValueChange={onValueChange} disabled={disabled || access.disabled || access.readOnly}>
        <RadixSelect.Trigger
          className={cn(
            "inline-flex min-h-11 w-full items-center justify-between gap-2 rounded-input",
            "border border-border-strong bg-field px-3.5 text-sm text-foreground",
            "transition-ui",
            "focus:outline-none focus:border-action focus:ring-[3px] focus:ring-action/15",
            "disabled:bg-surface-subtle disabled:cursor-not-allowed disabled:opacity-70",
            "data-[placeholder]:text-muted",
            error && "border-danger"
          )}
        >
          <RadixSelect.Value placeholder={placeholder} />
          <RadixSelect.Icon>
            <ChevronDown className="h-3.5 w-3.5 text-muted" />
          </RadixSelect.Icon>
        </RadixSelect.Trigger>
        <RadixSelect.Portal>
          <RadixSelect.Content
            className="z-50 min-w-[8rem] overflow-hidden rounded-table border border-border bg-surface shadow-dp2"
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
        <p className="text-xs font-medium text-danger">{error}</p>
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
        "relative flex min-h-11 cursor-default select-none items-center rounded-input px-9 py-2",
        "text-sm text-foreground",
        "focus:bg-action-soft focus:text-action focus:outline-none",
        "data-[disabled]:pointer-events-none data-[disabled]:opacity-50"
      )}
    >
      <RadixSelect.ItemIndicator className="absolute left-1.5 inline-flex items-center">
        <Check className="h-3 w-3 text-action" />
      </RadixSelect.ItemIndicator>
      <RadixSelect.ItemText>{children}</RadixSelect.ItemText>
    </RadixSelect.Item>
  );
}
