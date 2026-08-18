"use client";

import { cn } from "@/lib/utils/cn";
import { useUiControl, type UiControlPermissionProps } from "@/lib/auth/ui-control";
import React from "react";

interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement>, UiControlPermissionProps {
  label?: string;
  error?: string;
  hint?: string;
}

export const Textarea = React.forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ label, error, hint, className, id, controlKey, permission, systemRequired, ...props }, ref) => {
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
        <textarea
          {...props}
          ref={ref}
          id={inputId}
          disabled={props.disabled || access.disabled}
          readOnly={props.readOnly || access.readOnly}
          {...access.dataAttributes}
          className={cn(
            "min-h-24 w-full rounded-input border border-border-strong bg-field px-3.5 py-3 text-sm",
            "text-foreground placeholder:text-muted resize-y",
            "transition-ui",
            "focus:outline-none focus:border-action focus:ring-[3px] focus:ring-action/15",
            "disabled:bg-surface-subtle disabled:cursor-not-allowed",
            error && "border-danger focus:border-danger focus:ring-danger",
            className
          )}
        />
        {error && <p className="text-xs font-medium text-danger">{error}</p>}
        {hint && !error && <p className="text-xs text-muted">{hint}</p>}
      </div>
    );
  }
);
Textarea.displayName = "Textarea";
