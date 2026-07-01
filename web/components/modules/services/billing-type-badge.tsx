import { Badge } from "@/components/ui/badge";
import { getBillingTypeLabel } from "@/lib/utils/format";

const VARIANT_MAP: Record<string, "periwinkle" | "jade" | "saffron" | "plum" | "muted"> = {
  Monthly: "periwinkle",
  Yearly: "plum",
  OneTime: "jade",
  Hourly: "saffron",
  Custom: "muted",
};

export function BillingTypeBadge({ billingType }: { billingType: string }) {
  const variant = VARIANT_MAP[billingType] ?? "muted";
  return <Badge variant={variant}>{getBillingTypeLabel(billingType)}</Badge>;
}
