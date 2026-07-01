import { Badge } from "@/components/ui/badge";
import type { BillingItemStatus } from "@/types/api";

type V = "saffron" | "periwinkle" | "jade" | "terracotta";

const MAP: Record<BillingItemStatus, { label: string; variant: V }> = {
  Pending:   { label: "Pendiente",  variant: "saffron"    },
  Partial:   { label: "Parcial",    variant: "periwinkle" },
  Paid:      { label: "Pagado",     variant: "jade"       },
  Cancelled: { label: "Cancelado",  variant: "terracotta" },
};

export function BillingItemStatusBadge({ status }: { status: BillingItemStatus | string }) {
  const cfg = MAP[status as BillingItemStatus] ?? { label: status, variant: "saffron" as V };
  return <Badge variant={cfg.variant}>{cfg.label}</Badge>;
}
