import { Badge } from "@/components/ui/badge";
import type { LedgerEntryType } from "@/types/api";

const CONFIG: Record<LedgerEntryType, { label: string; variant: "jade" | "terracotta" | "periwinkle" | "saffron" | "plum" | "muted" }> = {
  Income:      { label: "Ingreso",       variant: "jade" },
  Expense:     { label: "Egreso",        variant: "terracotta" },
  TransferIn:  { label: "Transferencia entrada", variant: "periwinkle" },
  TransferOut: { label: "Transferencia salida",  variant: "plum" },
  Adjustment:  { label: "Ajuste",        variant: "saffron" },
};

export function EntryTypeBadge({ type }: { type: LedgerEntryType }) {
  const { label, variant } = CONFIG[type] ?? { label: type, variant: "muted" };
  return <Badge variant={variant}>{label}</Badge>;
}

export function getEntryTypeLabel(type: string): string {
  return CONFIG[type as LedgerEntryType]?.label ?? type;
}
