"use client";

import { useEffect, useState } from "react";
import { RefreshCw, WifiOff } from "lucide-react";
import { Button } from "@/components/ui/button";

export function PwaStatus() {
  const [online, setOnline] = useState(true);
  const [updateReady, setUpdateReady] = useState(false);

  useEffect(() => {
    setOnline(navigator.onLine);
    const on = () => setOnline(true);
    const off = () => setOnline(false);
    window.addEventListener("online", on);
    window.addEventListener("offline", off);

    if ("serviceWorker" in navigator && process.env.NODE_ENV === "production") {
      navigator.serviceWorker.register("/sw.js").then((registration) => {
        if (registration.waiting) setUpdateReady(true);
        registration.addEventListener("updatefound", () => {
          registration.installing?.addEventListener("statechange", () => {
            if (registration.waiting) setUpdateReady(true);
          });
        });
      }).catch(() => undefined);
    }
    return () => { window.removeEventListener("online", on); window.removeEventListener("offline", off); };
  }, []);

  if (online && !updateReady) return null;
  return (
    <div role="status" aria-live="polite" className="sticky top-0 z-[80] flex min-h-11 flex-wrap items-center justify-center gap-3 border-b border-warning bg-warning-soft px-4 py-2 text-sm text-warning">
      {!online ? <><WifiOff className="h-4 w-4" aria-hidden="true" /><span><strong>Sin conexión.</strong> Podés consultar el shell disponible; ninguna operación financiera se enviará en segundo plano.</span></> : <span>Hay una versión nueva y segura de AegiFinance disponible.</span>}
      {updateReady ? <Button controlKey="system.pwa.update" systemRequired size="sm" variant="outline" onClick={() => window.location.reload()}><RefreshCw className="h-4 w-4" />Actualizar</Button> : null}
    </div>
  );
}
