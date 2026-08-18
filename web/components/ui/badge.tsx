import { cn } from "@/lib/utils/cn";

type BadgeVariant =
  | "jade"
  | "saffron"
  | "terracotta"
  | "periwinkle"
  | "plum"
  | "muted"
  | "primary";

interface BadgeProps {
  variant?: BadgeVariant;
  children: React.ReactNode;
  className?: string;
}

const variantClasses: Record<BadgeVariant, string> = {
  jade: "bg-success-soft text-success",
  saffron: "bg-warning-soft text-warning",
  terracotta: "bg-danger-soft text-danger",
  periwinkle: "bg-info-soft text-info",
  plum: "bg-accent-soft text-accent",
  muted: "bg-border text-muted",
  primary: "bg-action-soft text-action",
};

export function Badge({ variant = "muted", children, className }: BadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5",
        "text-[10px] font-bold uppercase tracking-wider whitespace-nowrap",
        variantClasses[variant],
        className
      )}
    >
      {children}
    </span>
  );
}
