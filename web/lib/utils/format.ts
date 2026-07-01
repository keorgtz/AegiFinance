import { format, formatDistanceToNow } from "date-fns";
import { es } from "date-fns/locale";

export function formatDate(date: string | Date | null | undefined): string {
  if (!date) return "—";
  const d = typeof date === "string" ? new Date(date) : date;
  return format(d, "dd/MMM/yyyy", { locale: es });
}

export function formatDateTime(date: string | Date | null | undefined): string {
  if (!date) return "—";
  const d = typeof date === "string" ? new Date(date) : date;
  return format(d, "dd/MMM/yyyy HH:mm", { locale: es });
}

export function formatRelative(date: string | Date | null | undefined): string {
  if (!date) return "—";
  const d = typeof date === "string" ? new Date(date) : date;
  return formatDistanceToNow(d, { addSuffix: true, locale: es });
}

export function formatMXN(amount: number): string {
  return new Intl.NumberFormat("es-MX", {
    style: "currency",
    currency: "MXN",
    minimumFractionDigits: 2,
  }).format(amount);
}

export function getUserTypeLabel(type: string): string {
  return type === "Administrator" ? "Administrador" : "Cliente";
}

const BILLING_TYPE_LABELS: Record<string, string> = {
  Monthly: "Mensual",
  Yearly: "Anual",
  OneTime: "Único",
  Hourly: "Por hora",
  Custom: "Personalizado",
};

export function getBillingTypeLabel(type: string): string {
  return BILLING_TYPE_LABELS[type] ?? type;
}

export function formatAmount(amount: number, currency = "MXN"): string {
  return new Intl.NumberFormat("es-MX", {
    style: "currency",
    currency,
    minimumFractionDigits: 2,
  }).format(amount);
}
