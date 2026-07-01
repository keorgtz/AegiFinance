import { Badge } from "@/components/ui/badge";
import type { BillingCycleStatus } from "@/types/api";

const MAP: Record<BillingCycleStatus, { label: string; variant: "jade" | "muted" | "saffron" }> = {
  Open:         { label: "Abierto",       variant: "jade"    },
  Closed:       { label: "Cerrado",       variant: "muted"   },
  Reprocessing: { label: "Reprocesando",  variant: "saffron" },
};

export function BillingCycleStatusBadge({ status }: { status: BillingCycleStatus | string }) {
  const cfg = MAP[status as BillingCycleStatus] ?? { label: status, variant: "muted" as const };
  return <Badge variant={cfg.variant}>{cfg.label}</Badge>;
}
