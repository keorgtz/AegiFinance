"use client";

import { usePermissions } from "./use-permissions";

interface CanProps {
  permission: string;
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export function Can({ permission, children, fallback = null }: CanProps) {
  const { can } = usePermissions();
  return can(permission) ? <>{children}</> : <>{fallback}</>;
}

interface IsAdminProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export function IsAdmin({ children, fallback = null }: IsAdminProps) {
  const { isAdmin } = usePermissions();
  return isAdmin() ? <>{children}</> : <>{fallback}</>;
}

export function IsClient({ children, fallback = null }: IsAdminProps) {
  const { isClient } = usePermissions();
  return isClient() ? <>{children}</> : <>{fallback}</>;
}
