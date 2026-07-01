"use client";

import * as RadixDialog from "@radix-ui/react-dialog";
import { X } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export const Dialog = RadixDialog.Root;
export const DialogTrigger = RadixDialog.Trigger;

interface DialogContentProps extends RadixDialog.DialogContentProps {
  title: string;
  description?: string;
  size?: "sm" | "md" | "lg";
}

const sizeClasses = {
  sm: "max-w-sm",
  md: "max-w-lg",
  lg: "max-w-2xl",
};

export function DialogContent({
  title,
  description,
  size = "md",
  children,
  className,
  ...props
}: DialogContentProps) {
  return (
    <RadixDialog.Portal>
      <RadixDialog.Overlay className="fixed inset-0 z-50 bg-black/30 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
      <RadixDialog.Content
        className={cn(
          "fixed left-1/2 top-1/2 z-50 w-full -translate-x-1/2 -translate-y-1/2",
          "bg-white rounded-card shadow-dp3",
          "data-[state=open]:animate-in data-[state=closed]:animate-out",
          "data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0",
          "data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95",
          "focus:outline-none",
          sizeClasses[size],
          className
        )}
        {...props}
      >
        <div className="flex items-center justify-between border-b border-[#E3E6EC] px-5 py-4">
          <div>
            <RadixDialog.Title className="font-display text-[15px] font-700 text-[#16181D]">
              {title}
            </RadixDialog.Title>
            {description && (
              <RadixDialog.Description className="mt-0.5 text-[12px] text-[#5B6472]">
                {description}
              </RadixDialog.Description>
            )}
          </div>
          <RadixDialog.Close
            className="rounded-button p-1 text-[#5B6472] hover:bg-[#EFF1F7] hover:text-[#16181D] transition-colors"
            aria-label="Cerrar"
          >
            <X className="h-4 w-4" />
          </RadixDialog.Close>
        </div>
        <div className="px-5 py-4">{children}</div>
      </RadixDialog.Content>
    </RadixDialog.Portal>
  );
}

export function DialogFooter({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex justify-end gap-2 border-t border-[#E3E6EC] px-5 py-4 -mx-5 -mb-4 mt-4">
      {children}
    </div>
  );
}

export const DialogClose = RadixDialog.Close;
