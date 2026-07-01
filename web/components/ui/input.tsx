import { cn } from "@/lib/utils/cn";
import React from "react";

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  hint?: string;
  leftIcon?: React.ReactNode;
}

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, hint, leftIcon, className, id, ...props }, ref) => {
    const inputId = id ?? label?.toLowerCase().replace(/\s+/g, "-");

    return (
      <div className="flex flex-col gap-1">
        {label && (
          <label
            htmlFor={inputId}
            className="text-[11px] font-600 uppercase tracking-wider text-[#5B6472]"
          >
            {label}
          </label>
        )}
        <div className="relative">
          {leftIcon && (
            <div className="absolute left-2.5 top-1/2 -translate-y-1/2 text-[#5B6472]">
              {leftIcon}
            </div>
          )}
          <input
            ref={ref}
            id={inputId}
            className={cn(
              "h-8 w-full rounded-input border border-[#E3E6EC] bg-white px-3 text-[13px]",
              "text-[#16181D] placeholder:text-[#5B6472]",
              "transition-colors duration-150",
              "focus:outline-none focus:border-[#5BAEBC] focus:ring-1 focus:ring-[#5BAEBC]",
              "disabled:bg-[#F7F8FA] disabled:cursor-not-allowed disabled:opacity-70",
              error && "border-[#B6452C] focus:border-[#B6452C] focus:ring-[#B6452C]",
              leftIcon && "pl-8",
              className
            )}
            {...props}
          />
        </div>
        {error && (
          <p className="text-[11px] text-[#B6452C] font-500">{error}</p>
        )}
        {hint && !error && (
          <p className="text-[11px] text-[#5B6472]">{hint}</p>
        )}
      </div>
    );
  }
);

Input.displayName = "Input";
