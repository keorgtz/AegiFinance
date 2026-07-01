import * as RadixAvatar from "@radix-ui/react-avatar";
import { cn } from "@/lib/utils/cn";

interface AvatarProps {
  name?: string;
  src?: string | null;
  size?: "sm" | "md" | "lg";
  className?: string;
}

const sizeClasses = { sm: "h-6 w-6 text-[10px]", md: "h-8 w-8 text-xs", lg: "h-10 w-10 text-sm" };

function getInitials(name: string): string {
  return name
    .split(" ")
    .slice(0, 2)
    .map((n) => n[0])
    .join("")
    .toUpperCase();
}

export function Avatar({ name = "", src, size = "md", className }: AvatarProps) {
  return (
    <RadixAvatar.Root
      className={cn(
        "inline-flex items-center justify-center overflow-hidden rounded-full select-none",
        "bg-[#C9E8ED] text-[#0F5C6B] font-700",
        sizeClasses[size],
        className
      )}
    >
      {src && (
        <RadixAvatar.Image src={src} alt={name} className="h-full w-full object-cover" />
      )}
      <RadixAvatar.Fallback delayMs={0}>
        {getInitials(name)}
      </RadixAvatar.Fallback>
    </RadixAvatar.Root>
  );
}
