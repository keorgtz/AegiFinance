import { cn } from "@/lib/utils/cn";
import { Loader2 } from "lucide-react";
import React from "react";

type Variant = "primary" | "secondary" | "ghost" | "danger" | "outline";
type Size = "sm" | "md" | "lg" | "icon";

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  size?: Size;
  loading?: boolean;
}

const variantClasses: Record<Variant, string> = {
  primary:
    "bg-[#0F5C6B] text-white hover:bg-[#16798C] active:bg-[#0F5C6B] shadow-dp1",
  secondary:
    "bg-[#EFF1F7] text-[#16181D] hover:bg-[#E3E6EC] active:bg-[#d8dbe4]",
  ghost:
    "bg-transparent text-[#5B6472] hover:bg-[#EFF1F7] hover:text-[#16181D]",
  danger:
    "bg-[#B6452C] text-white hover:bg-[#D06A4A] active:bg-[#B6452C] shadow-dp1",
  outline:
    "border border-[#E3E6EC] bg-white text-[#16181D] hover:bg-[#F7F8FA]",
};

const sizeClasses: Record<Size, string> = {
  sm: "h-7 px-3 text-xs",
  md: "h-8 px-4 text-xs",
  lg: "h-10 px-5 text-sm",
  icon: "h-8 w-8 p-0",
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
      ...props
    },
    ref
  ) => {
    return (
      <button
        ref={ref}
        disabled={disabled || loading}
        className={cn(
          "inline-flex items-center justify-center gap-1.5 font-semibold rounded-button",
          "transition-colors duration-150 focus-visible:outline-none focus-visible:ring-2",
          "focus-visible:ring-[#5BAEBC] focus-visible:ring-offset-1",
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
