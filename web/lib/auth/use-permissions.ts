"use client";

import { useMemo } from "react";
import { useAuth } from "./context";
import type { UiAccessMode } from "@/types/api";

export function usePermissions() {
  const { user } = useAuth();

  const permissions = useMemo(() => new Set(user?.permissions ?? []), [user]);
  const roles = useMemo(() => new Set(user?.roles ?? []), [user]);

  const can = (permission: string): boolean => permissions.has(permission);
  const accessFor = (controlKey: string, permission?: string): UiAccessMode => {
    const explicit = user?.uiPolicies?.[controlKey];
    if (explicit) return explicit;
    return permission && !permissions.has(permission) ? "Hidden" : "Enabled";
  };
  const hasRole = (role: string): boolean => roles.has(role);
  const isAdmin = (): boolean => user?.userType === "Administrator";
  const isClient = (): boolean => user?.userType === "Client";

  return { can, accessFor, hasRole, isAdmin, isClient, permissions, roles };
}
