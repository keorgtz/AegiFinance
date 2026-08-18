import { cn } from "@/lib/utils/cn";
import { Loader2 } from "lucide-react";

interface SpinnerProps {
  className?: string;
  size?: "sm" | "md" | "lg";
}

const sizes = { sm: "h-4 w-4", md: "h-5 w-5", lg: "h-8 w-8" };

export function Spinner({ className, size = "md" }: SpinnerProps) {
  return (
    <Loader2
      className={cn("animate-spin text-muted", sizes[size], className)}
    />
  );
}

export function FullPageSpinner() {
  return (
    <div className="flex h-screen items-center justify-center bg-canvas">
      <Spinner size="lg" className="text-action" />
    </div>
  );
}
