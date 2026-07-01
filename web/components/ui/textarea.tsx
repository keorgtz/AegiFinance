import { cn } from "@/lib/utils/cn";
import React from "react";

interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string;
  error?: string;
  hint?: string;
}

export const Textarea = React.forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ label, error, hint, className, id, ...props }, ref) => {
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
        <textarea
          ref={ref}
          id={inputId}
          className={cn(
            "min-h-[80px] w-full rounded-input border border-[#E3E6EC] bg-white px-3 py-2 text-[13px]",
            "text-[#16181D] placeholder:text-[#5B6472] resize-y",
            "transition-colors duration-150",
            "focus:outline-none focus:border-[#5BAEBC] focus:ring-1 focus:ring-[#5BAEBC]",
            "disabled:bg-[#F7F8FA] disabled:cursor-not-allowed",
            error && "border-[#B6452C] focus:border-[#B6452C] focus:ring-[#B6452C]",
            className
          )}
          {...props}
        />
        {error && <p className="text-[11px] text-[#B6452C] font-500">{error}</p>}
        {hint && !error && <p className="text-[11px] text-[#5B6472]">{hint}</p>}
      </div>
    );
  }
);
Textarea.displayName = "Textarea";
