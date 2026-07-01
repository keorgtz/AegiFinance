"use client";

import { useEffect, useState } from "react";
import { Wifi, WifiOff } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export function StatusStrip() {
  const [online, setOnline] = useState(true);

  useEffect(() => {
    const on = () => setOnline(true);
    const off = () => setOnline(false);
    setOnline(navigator.onLine);
    window.addEventListener("online", on);
    window.addEventListener("offline", off);
    return () => {
      window.removeEventListener("online", on);
      window.removeEventListener("offline", off);
    };
  }, []);

  return (
    <div
      className={cn(
        "flex h-8 items-center justify-between border-t px-4",
        "text-[10px] font-600 uppercase tracking-wider transition-colors duration-300",
        online
          ? "border-[#E3E6EC] bg-[#F7F8FA] text-[#5B6472]"
          : "border-[#FBE6DC] bg-[#FFF6F1] text-[#B6452C]"
      )}
    >
      <span>AegiFinance v0.1</span>
      <div className="flex items-center gap-1.5">
        {online ? (
          <>
            <Wifi className="h-3 w-3 text-[#0E9F6E]" />
            <span className="text-[#0E9F6E]">Conectado</span>
          </>
        ) : (
          <>
            <WifiOff className="h-3 w-3" />
            <span>Sin conexión</span>
          </>
        )}
      </div>
    </div>
  );
}
