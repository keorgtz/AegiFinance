import { Activity } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export function Brand({ compact = false, className }: { compact?: boolean; className?: string }) {
  return (
    <div className={cn("flex items-center gap-3", className)} aria-label="AegiFinance">
      <span className="grid h-10 w-10 flex-none place-items-center rounded-[14px] bg-action text-on-action shadow-dp1">
        <Activity className="h-5 w-5" strokeWidth={2.4} />
      </span>
      {!compact && (
        <span className="font-display text-lg font-bold tracking-[-0.03em] text-foreground">
          Aegi<span className="text-action">Finance</span>
        </span>
      )}
    </div>
  );
}
