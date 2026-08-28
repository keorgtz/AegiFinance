"use client";

import * as RadixSwitch from "@radix-ui/react-switch";
import { useUiControl, type UiControlPermissionProps } from "@/lib/auth/ui-control";
import { cn } from "@/lib/utils/cn";

interface SwitchProps extends UiControlPermissionProps {
  id: string;
  checked: boolean;
  onCheckedChange: (checked: boolean) => void;
  disabled?: boolean;
  ariaLabel: string;
  checkedLabel?: string;
  uncheckedLabel?: string;
  showStateLabel?: boolean;
  className?: string;
}

export function Switch({
  id,
  checked,
  onCheckedChange,
  disabled,
  ariaLabel,
  checkedLabel = "Activado",
  uncheckedLabel = "Desactivado",
  showStateLabel = true,
  className,
  controlKey,
  permission,
  systemRequired,
}: SwitchProps) {
  const access = useUiControl({ controlKey, permission, systemRequired });
  if (access.hidden) return null;

  const unavailable = disabled || access.disabled || access.readOnly;
  const stateLabel = checked ? checkedLabel : uncheckedLabel;

  return (
    <div className={cn("inline-flex shrink-0 items-center gap-2", className)}>
      {showStateLabel && (
        <span className="text-xs font-medium text-muted" aria-hidden="true">
          {stateLabel}
        </span>
      )}
      <span className="inline-flex min-h-11 min-w-11 items-center justify-center">
        <RadixSwitch.Root
          id={id}
          checked={checked}
          onCheckedChange={onCheckedChange}
          disabled={unavailable}
          aria-label={`${ariaLabel}: ${stateLabel}`}
          {...access.dataAttributes}
          className={cn(
            "relative h-6 w-10 cursor-pointer rounded-full border transition-ui",
            "data-[state=checked]:border-action data-[state=checked]:bg-action",
            "data-[state=unchecked]:border-border-strong data-[state=unchecked]:bg-surface-subtle",
            "focus-visible:outline-none focus-visible:ring-[3px] focus-visible:ring-action/30 focus-visible:ring-offset-2 focus-visible:ring-offset-surface",
            "disabled:cursor-not-allowed disabled:opacity-50"
          )}
        >
          <RadixSwitch.Thumb className="block h-5 w-5 translate-x-0.5 rounded-full bg-surface shadow-dp1 transition-transform data-[state=checked]:translate-x-4" />
        </RadixSwitch.Root>
      </span>
    </div>
  );
}
