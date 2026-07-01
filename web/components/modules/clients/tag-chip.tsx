import type { ClientTagDto } from "@/types/api";
import { cn } from "@/lib/utils/cn";

interface TagChipProps {
  tag: ClientTagDto;
  className?: string;
}

function isLight(hex: string): boolean {
  const c = hex.replace("#", "");
  const r = parseInt(c.substring(0, 2), 16);
  const g = parseInt(c.substring(2, 4), 16);
  const b = parseInt(c.substring(4, 6), 16);
  return (r * 299 + g * 587 + b * 114) / 1000 > 155;
}

export function TagChip({ tag, className }: TagChipProps) {
  const light = isLight(tag.color || "#888888");
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2 py-0.5 text-[10px] font-700",
        className
      )}
      style={{
        backgroundColor: tag.color + "33",
        color: tag.color,
        border: `1px solid ${tag.color}55`,
      }}
    >
      {tag.name}
    </span>
  );
}
