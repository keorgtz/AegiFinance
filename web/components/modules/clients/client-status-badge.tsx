import { Badge } from "@/components/ui/badge";
import type { ClientStatus } from "@/types/api";

const STATUS_MAP: Record<ClientStatus, { label: string; variant: "jade" | "terracotta" | "saffron" }> = {
  Active: { label: "Activo", variant: "jade" },
  Inactive: { label: "Inactivo", variant: "terracotta" },
  Prospective: { label: "Prospecto", variant: "saffron" },
};

export function ClientStatusBadge({ status }: { status: ClientStatus | string }) {
  const cfg = STATUS_MAP[status as ClientStatus] ?? { label: status, variant: "muted" as const };
  return <Badge variant={cfg.variant}>{cfg.label}</Badge>;
}
