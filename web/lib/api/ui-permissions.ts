import { api } from "./client";
import type { UiAccessMode, UiControlCatalogItem, UiControlPolicyRequest } from "@/types/api";

export const uiPermissionsApi = {
  catalog: () => api.get<UiControlCatalogItem[]>("/ui-permissions/catalog"),
  setPolicy: (policy: UiControlPolicyRequest) => api.put<void>("/ui-permissions/policy", policy),
  simulate: (request: { roleId: string; userId?: string | null; clientId?: string | null; subscriptionId?: string | null }) =>
    api.post<Record<string, UiAccessMode>>("/ui-permissions/simulate", request),
};
