"use client";

import * as RadixDialog from "@radix-ui/react-dialog";
import { X } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export const Drawer = RadixDialog.Root;
export const DrawerTrigger = RadixDialog.Trigger;
export const DrawerClose = RadixDialog.Close;

interface DrawerContentProps extends RadixDialog.DialogContentProps {
  title: string;
  description?: string;
  width?: "sm" | "md" | "lg";
  footer?: React.ReactNode;
}

const widthClasses = { sm: "max-w-sm", md: "max-w-lg", lg: "max-w-2xl" };

export function DrawerContent({
  title,
  description,
  width = "md",
  footer,
  children,
  className,
  ...props
}: DrawerContentProps) {
  return (
    <RadixDialog.Portal>
      <RadixDialog.Overlay className="fixed inset-0 z-50 bg-black/30 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
      <RadixDialog.Content
        className={cn(
          "fixed right-0 top-0 z-50 flex h-full flex-col bg-white shadow-dp3",
          "data-[state=open]:animate-in data-[state=closed]:animate-out",
          "data-[state=closed]:slide-out-to-right data-[state=open]:slide-in-from-right",
          "duration-200 focus:outline-none w-full",
          widthClasses[width],
          className
        )}
        {...props}
      >
        {/* Header */}
        <div className="flex items-center justify-between border-b border-[#E3E6EC] px-6 py-4 flex-shrink-0">
          <div>
            <RadixDialog.Title className="font-display text-[16px] font-700 text-[#16181D]">
              {title}
            </RadixDialog.Title>
            {description && (
              <RadixDialog.Description className="mt-0.5 text-[12px] text-[#5B6472]">
                {description}
              </RadixDialog.Description>
            )}
          </div>
          <RadixDialog.Close
            className="rounded-button p-1.5 text-[#5B6472] hover:bg-[#EFF1F7] hover:text-[#16181D] transition-colors"
            aria-label="Cerrar"
          >
            <X className="h-4 w-4" />
          </RadixDialog.Close>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto px-6 py-5">{children}</div>

        {/* Footer */}
        {footer && (
          <div className="flex items-center justify-end gap-2 border-t border-[#E3E6EC] px-6 py-4 flex-shrink-0">
            {footer}
          </div>
        )}
      </RadixDialog.Content>
    </RadixDialog.Portal>
  );
}
