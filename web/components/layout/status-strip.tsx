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
        "hidden min-h-8 items-center justify-between border-t px-4 lg:flex",
        "text-[10px] font-semibold uppercase tracking-wider transition-colors duration-300",
        online
          ? "border-border bg-surface-subtle text-muted"
          : "border-danger-soft bg-danger-soft text-danger"
      )}
    >
      <span>Major Ledger · fuente de verdad financiera</span>
      <div className="flex items-center gap-1.5">
        {online ? (
          <>
            <Wifi className="h-3 w-3 text-success" />
            <span className="text-success">Servicio disponible</span>
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
