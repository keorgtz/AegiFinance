"use client";

import { cn } from "@/lib/utils/cn";
import { createContext, useContext, useState } from "react";

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
        "flex gap-0 border-b border-[#E3E6EC] overflow-x-auto",
        className
      )}
    >
      {children}
    </div>
  );
}

interface TabProps {
  id: string;
  children: React.ReactNode;
}

export function Tab({ id, children }: TabProps) {
  const ctx = useContext(TabsContext);
  if (!ctx) throw new Error("Tab must be inside Tabs");
  const active = ctx.active === id;

  return (
    <button
      role="tab"
      aria-selected={active}
      onClick={() => ctx.setActive(id)}
      className={cn(
        "whitespace-nowrap px-4 py-2.5 text-[13px] font-600 transition-colors duration-100",
        "border-b-2 -mb-px focus:outline-none focus-visible:ring-2 focus-visible:ring-[#5BAEBC]",
        active
          ? "border-[#0F5C6B] text-[#0F5C6B]"
          : "border-transparent text-[#5B6472] hover:text-[#16181D] hover:border-[#E3E6EC]"
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
    <div role="tabpanel" className={cn("pt-5", className)}>
      {children}
    </div>
  );
}
