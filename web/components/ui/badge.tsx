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
  jade: "bg-[#DFFBEF] text-[#0E9F6E]",
  saffron: "bg-[#FBEACB] text-[#B7791F]",
  terracotta: "bg-[#FBE6DC] text-[#B6452C]",
  periwinkle: "bg-[#E4E9FC] text-[#5469D4]",
  plum: "bg-[#F8E1EE] text-[#A1336B]",
  muted: "bg-[#E3E6EC] text-[#5B6472]",
  primary: "bg-[#C9E8ED] text-[#0F5C6B]",
};

export function Badge({ variant = "muted", children, className }: BadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2.5 py-0.5",
        "text-[10px] font-700 uppercase tracking-wider whitespace-nowrap",
        variantClasses[variant],
        className
      )}
    >
      {children}
    </span>
  );
}
