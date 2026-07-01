"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/context";
import { Topbar } from "@/components/layout/topbar";
import { Sidebar } from "@/components/layout/sidebar";
import { StatusStrip } from "@/components/layout/status-strip";
import { MobileNav } from "@/components/layout/mobile-nav";
import { FullPageSpinner } from "@/components/ui/spinner";

export default function AppLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { isAuthenticated, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push("/login");
    }
  }, [isLoading, isAuthenticated, router]);

  if (isLoading) return <FullPageSpinner />;
  if (!isAuthenticated) return null;

  return (
    <div className="flex h-screen flex-col overflow-hidden">
      <Topbar />

      <div className="flex flex-1 overflow-hidden">
        {/* Sidebar (solo desktop) */}
        <div className="hidden sm:flex">
          <Sidebar />
        </div>

        {/* Contenido principal */}
        <main className="flex flex-1 flex-col overflow-hidden">
          <div className="flex-1 overflow-y-auto p-6 pb-16 sm:pb-6">
            {children}
          </div>
          <StatusStrip />
        </main>
      </div>

      {/* Bottom nav (solo mobile) */}
      <MobileNav />
    </div>
  );
}
