"use client";

import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import { cn } from "@/lib/utils/cn";
import { useUiControl, type UiControlPermissionProps } from "@/lib/auth/ui-control";

type Props = React.ComponentPropsWithoutRef<typeof DropdownMenu.Item> & UiControlPermissionProps;

export function PermissionMenuItem({ controlKey, permission, systemRequired, className, children, disabled, ...props }: Props) {
  const access = useUiControl({ controlKey, permission, systemRequired });
  if (access.hidden) return null;

  return (
    <DropdownMenu.Item
      {...props}
      {...access.dataAttributes}
      disabled={disabled || access.disabled || access.readOnly}
      className={cn(
        "flex min-h-11 cursor-pointer items-center gap-2 rounded-input px-3 py-2 text-sm text-foreground-secondary",
        "focus:bg-surface-subtle focus:outline-none data-[disabled]:cursor-not-allowed data-[disabled]:opacity-50",
        className
      )}
    >
      {children}
    </DropdownMenu.Item>
  );
}
