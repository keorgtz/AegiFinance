"use client";

import { useMemo } from "react";
import { useAuth } from "./context";

export function usePermissions() {
  const { user } = useAuth();

  const permissions = useMemo(() => new Set(user?.permissions ?? []), [user]);
  const roles = useMemo(() => new Set(user?.roles ?? []), [user]);

  const can = (permission: string): boolean => permissions.has(permission);
  const hasRole = (role: string): boolean => roles.has(role);
  const isAdmin = (): boolean => user?.userType === "Administrator";
  const isClient = (): boolean => user?.userType === "Client";

  return { can, hasRole, isAdmin, isClient, permissions, roles };
}
