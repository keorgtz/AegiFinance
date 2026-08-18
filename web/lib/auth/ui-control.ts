"use client";

import { usePermissions } from "./use-permissions";

export interface UiControlPermissionProps {
  controlKey?: string;
  permission?: string;
  systemRequired?: boolean;
}

export function useUiControl({ controlKey, permission, systemRequired }: UiControlPermissionProps) {
  const { accessFor } = usePermissions();
  const mode = systemRequired || !controlKey ? "Enabled" : accessFor(controlKey, permission);
  return {
    mode,
    hidden: mode === "Hidden",
    disabled: mode === "Disabled",
    readOnly: mode === "ReadOnly",
    dataAttributes: controlKey
      ? {
          "data-ui-control": controlKey,
          "data-ui-permission": permission,
          "data-ui-system-required": systemRequired ? "true" : undefined,
        }
      : {},
  } as const;
}
