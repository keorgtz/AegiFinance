"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "@/lib/auth/context";
import { Toaster } from "sonner";
import { useState } from "react";
import { ThemeProvider } from "@/lib/theme/context";
import { UiPermissionProvider } from "@/lib/auth/ui-permission-provider";
import { PwaStatus } from "@/components/system/pwa-status";

export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 30_000,
            retry: 1,
          },
        },
      })
  );

  return (
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <UiPermissionProvider><PwaStatus />{children}</UiPermissionProvider>
          <Toaster
            position="bottom-right"
            toastOptions={{
              classNames: {
                toast: "font-ui text-sm rounded-table border border-border bg-surface text-foreground shadow-dp2",
              },
            }}
          />
        </AuthProvider>
      </QueryClientProvider>
    </ThemeProvider>
  );
}
