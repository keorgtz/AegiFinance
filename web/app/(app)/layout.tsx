"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth/context";
import { Topbar } from "@/components/layout/topbar";
import { Sidebar } from "@/components/layout/sidebar";
import { StatusStrip } from "@/components/layout/status-strip";
import { MobileNav } from "@/components/layout/mobile-nav";
import { FullPageSpinner } from "@/components/ui/spinner";
import { RoutePermissionGuard } from "@/components/layout/route-permission-guard";
import { RequiredPasswordChange } from "@/components/modules/users/required-password-change";

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
    <div className="flex h-dvh overflow-hidden bg-canvas">
      <div className="hidden lg:block">
        <Sidebar />
      </div>

      <div className="flex min-w-0 flex-1 flex-col overflow-hidden">
        <Topbar />
        <main className="flex min-h-0 flex-1 flex-col overflow-hidden">
          <div className="flex-1 overflow-y-auto px-3 pb-24 pt-4 sm:px-5 sm:pt-6 lg:px-8 lg:pb-8">
            <div className="page-shell"><RoutePermissionGuard>{children}</RoutePermissionGuard></div>
          </div>
          <StatusStrip />
        </main>
      </div>
      <MobileNav />
      <RequiredPasswordChange />
    </div>
  );
}
