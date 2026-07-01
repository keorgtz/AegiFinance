import { Badge } from "@/components/ui/badge";
import type { SubscriptionStatus } from "@/types/api";

type Variant = "jade" | "terracotta" | "saffron" | "muted" | "plum";

const STATUS_MAP: Record<SubscriptionStatus, { label: string; variant: Variant }> = {
  Pending:   { label: "Pendiente",   variant: "saffron"   },
  Active:    { label: "Activa",      variant: "jade"      },
  Suspended: { label: "Suspendida",  variant: "terracotta" },
  Cancelled: { label: "Cancelada",   variant: "muted"     },
  Expired:   { label: "Vencida",     variant: "plum"      },
};

export function SubscriptionStatusBadge({ status }: { status: SubscriptionStatus | string }) {
  const cfg = STATUS_MAP[status as SubscriptionStatus] ?? { label: status, variant: "muted" as Variant };
  return <Badge variant={cfg.variant}>{cfg.label}</Badge>;
}
