"use client";

import { cn } from "@/lib/utils/cn";
import { useUiControl, type UiControlPermissionProps } from "@/lib/auth/ui-control";
import { Loader2 } from "lucide-react";
import React from "react";

type Variant = "primary" | "secondary" | "ghost" | "danger" | "outline";
type Size = "sm" | "md" | "lg" | "icon";

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement>, UiControlPermissionProps {
  variant?: Variant;
  size?: Size;
  loading?: boolean;
}

const variantClasses: Record<Variant, string> = {
  primary:
    "bg-action text-on-action hover:bg-action-hover active:scale-[0.98] shadow-dp1",
  secondary:
    "border border-border bg-action-soft text-action hover:border-border-strong active:scale-[0.98]",
  ghost:
    "bg-transparent text-muted hover:bg-surface-subtle hover:text-foreground active:scale-[0.98]",
  danger:
    "bg-danger-soft text-danger hover:bg-danger hover:text-on-action active:scale-[0.98]",
  outline:
    "border border-border-strong bg-surface text-foreground hover:bg-surface-subtle active:scale-[0.98]",
};

const sizeClasses: Record<Size, string> = {
  sm: "min-h-11 px-4 text-xs",
  md: "min-h-11 px-5 text-sm",
  lg: "min-h-12 px-6 text-sm",
  icon: "h-11 w-11 p-0",
};

export const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      variant = "primary",
      size = "md",
      loading = false,
      disabled,
      className,
      children,
      controlKey,
      permission,
      systemRequired,
      ...props
    },
    ref
  ) => {
    const access = useUiControl({ controlKey, permission, systemRequired });
    if (access.hidden) return null;

    return (
      <button
        ref={ref}
        disabled={disabled || loading || access.disabled || access.readOnly}
        aria-readonly={access.readOnly || undefined}
        {...access.dataAttributes}
        className={cn(
          "inline-flex items-center justify-center gap-2 rounded-full font-semibold",
          "transition-ui focus-visible:outline-none focus-visible:ring-2",
          "focus-visible:ring-action focus-visible:ring-offset-2 focus-visible:ring-offset-canvas",
          "disabled:opacity-50 disabled:cursor-not-allowed",
          variantClasses[variant],
          sizeClasses[size],
          className
        )}
        {...props}
      >
        {loading ? (
          <Loader2 className="h-3.5 w-3.5 animate-spin" />
        ) : null}
        {children}
      </button>
    );
  }
);

Button.displayName = "Button";
