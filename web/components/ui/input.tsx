"use client";

import { cn } from "@/lib/utils/cn";
import { useUiControl, type UiControlPermissionProps } from "@/lib/auth/ui-control";
import React from "react";

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement>, UiControlPermissionProps {
  label?: string;
  error?: string;
  hint?: string;
  leftIcon?: React.ReactNode;
}

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, hint, leftIcon, className, id, controlKey, permission, systemRequired, ...props }, ref) => {
    const inputId = id ?? label?.toLowerCase().replace(/\s+/g, "-");
    const access = useUiControl({ controlKey, permission, systemRequired });
    if (access.hidden) return null;

    return (
      <div className="flex flex-col gap-2">
        {label && (
          <label
            htmlFor={inputId}
            className="text-xs font-semibold text-muted"
          >
            {label}
          </label>
        )}
        <div className="relative">
          {leftIcon && (
            <div className="absolute left-2.5 top-1/2 -translate-y-1/2 text-muted">
              {leftIcon}
            </div>
          )}
          <input
            {...props}
            ref={ref}
            id={inputId}
            disabled={props.disabled || access.disabled}
            readOnly={props.readOnly || access.readOnly}
            {...access.dataAttributes}
            className={cn(
              "min-h-11 w-full rounded-input border border-border-strong bg-field px-3.5 text-sm",
              "text-foreground placeholder:text-muted",
              "transition-ui",
              "focus:outline-none focus:border-action focus:ring-[3px] focus:ring-action/15",
              "disabled:bg-surface-subtle disabled:cursor-not-allowed disabled:opacity-70",
              error && "border-danger focus:border-danger focus:ring-danger",
              leftIcon && "pl-8",
              className
            )}
          />
        </div>
        {error && (
          <p className="text-xs font-medium text-danger">{error}</p>
        )}
        {hint && !error && (
          <p className="text-xs text-muted">{hint}</p>
        )}
      </div>
    );
  }
);

Input.displayName = "Input";
