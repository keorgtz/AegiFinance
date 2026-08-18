"use client";

import { useEffect, useRef } from "react";
import { useAuth } from "./context";
import { api } from "@/lib/api/client";
import manifest from "@/generated/ui-control-manifest.json";
import type { UiAccessMode } from "@/types/api";

export function UiPermissionProvider({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  const synced = useRef(false);

  useEffect(() => {
    if (!user?.permissions.includes("ManageRoles") || synced.current) return;
    synced.current = true;
    void api.post("/ui-permissions/catalog/sync", manifest).catch(() => {
      synced.current = false;
    });
  }, [user]);

  useEffect(() => {
    const applyPolicies = () => {
      document.querySelectorAll<HTMLElement>("[data-ui-control]").forEach((element) => {
        const key = element.dataset.uiControl;
        if (!key) return;
        const requiredPermission = element.dataset.uiPermission;
        const fallback: UiAccessMode = requiredPermission && !user?.permissions.includes(requiredPermission) ? "Hidden" : "Enabled";
        const mode = element.dataset.uiSystemRequired === "true" ? "Enabled" : user?.uiPolicies?.[key] ?? fallback;
        element.hidden = mode === "Hidden";
        const formControl = element as HTMLButtonElement | HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement;
        if (mode === "Disabled" || (mode === "ReadOnly" && element.tagName !== "INPUT" && element.tagName !== "TEXTAREA")) {
          formControl.disabled = true;
          element.dataset.uiPolicyDisabled = "true";
        } else if (element.dataset.uiPolicyDisabled === "true") {
          formControl.disabled = false;
          delete element.dataset.uiPolicyDisabled;
        }
        if (element.tagName === "INPUT" || element.tagName === "TEXTAREA") {
          (formControl as HTMLInputElement).readOnly = mode === "ReadOnly";
        }
      });
    };
    applyPolicies();
    const observer = new MutationObserver(applyPolicies);
    observer.observe(document.body, { childList: true, subtree: true });
    return () => observer.disconnect();
  }, [user]);

  return children;
}
