"use client";

import { cn } from "@/lib/utils/cn";
import { createContext, useContext, useEffect, useState } from "react";
import { useUiControl, type UiControlPermissionProps } from "@/lib/auth/ui-control";

interface TabsContextValue {
  active: string;
  setActive: (id: string) => void;
}

const TabsContext = createContext<TabsContextValue | null>(null);

interface TabsProps {
  defaultTab: string;
  children: React.ReactNode;
  className?: string;
}

export function Tabs({ defaultTab, children, className }: TabsProps) {
  const [active, setActive] = useState(defaultTab);
  useEffect(() => setActive(defaultTab), [defaultTab]);
  return (
    <TabsContext.Provider value={{ active, setActive }}>
      <div className={className}>{children}</div>
    </TabsContext.Provider>
  );
}

interface TabListProps {
  children: React.ReactNode;
  className?: string;
}

export function TabList({ children, className }: TabListProps) {
  return (
    <div
      role="tablist"
      className={cn(
        "flex gap-1 overflow-x-auto rounded-button bg-surface-subtle p-1",
        className
      )}
    >
      {children}
    </div>
  );
}

interface TabProps extends UiControlPermissionProps {
  id: string;
  children: React.ReactNode;
}

export function Tab({ id, children, controlKey, permission, systemRequired }: TabProps) {
  const ctx = useContext(TabsContext);
  if (!ctx) throw new Error("Tab must be inside Tabs");
  const active = ctx.active === id;
  const access = useUiControl({ controlKey, permission, systemRequired });
  if (access.hidden) return null;

  return (
    <button
      {...access.dataAttributes}
      disabled={access.disabled || access.readOnly}
      role="tab"
      id={`tab-${id}`}
      aria-selected={active}
      aria-controls={`panel-${id}`}
      onClick={() => ctx.setActive(id)}
      className={cn(
        "min-h-11 whitespace-nowrap rounded-input px-4 py-2.5 text-sm font-semibold transition-ui",
        "focus:outline-none focus-visible:ring-2 focus-visible:ring-action",
        active
          ? "bg-surface text-action shadow-dp1"
          : "text-muted hover:bg-surface/60 hover:text-foreground"
      )}
    >
      {children}
    </button>
  );
}

interface TabPanelProps {
  id: string;
  children: React.ReactNode;
  className?: string;
}

export function TabPanel({ id, children, className }: TabPanelProps) {
  const ctx = useContext(TabsContext);
  if (!ctx) throw new Error("TabPanel must be inside Tabs");
  if (ctx.active !== id) return null;
  return (
    <div id={`panel-${id}`} role="tabpanel" aria-labelledby={`tab-${id}`} className={cn("pt-5", className)}>
      {children}
    </div>
  );
}
