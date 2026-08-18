import {
  Building2,
  CreditCard,
  FileText,
  Layers,
  LayoutDashboard,
  Package,
  Receipt,
  Shield,
  Users,
  Wallet,
} from "lucide-react";

export interface NavigationItem {
  href: string;
  label: string;
  shortLabel?: string;
  icon: React.ElementType;
  permission: string;
  group: "overview" | "operations" | "administration";
}

export const NAVIGATION_ITEMS: NavigationItem[] = [
  { href: "/dashboard", label: "Resumen", shortLabel: "Inicio", icon: LayoutDashboard, permission: "ViewDashboard", group: "overview" },
  { href: "/clients", label: "Clientes", icon: Building2, permission: "ViewClients", group: "operations" },
  { href: "/services", label: "Planes y servicios", shortLabel: "Planes", icon: Package, permission: "ViewServices", group: "operations" },
  { href: "/subscriptions", label: "Suscripciones", shortLabel: "Suscrip.", icon: Layers, permission: "ViewSubscriptions", group: "operations" },
  { href: "/billing", label: "Cargos", icon: Receipt, permission: "ViewPayments", group: "operations" },
  { href: "/ledger", label: "Major Ledger", shortLabel: "Ledger", icon: Wallet, permission: "ViewPayments", group: "operations" },
  { href: "/allocations", label: "Aplicación de pagos", shortLabel: "Pagos", icon: CreditCard, permission: "ViewPayments", group: "operations" },
  { href: "/account-statement", label: "Estados de cuenta", icon: FileText, permission: "ViewPayments", group: "operations" },
  { href: "/users", label: "Usuarios", icon: Users, permission: "ManageUsers", group: "administration" },
  { href: "/roles", label: "Roles y permisos", icon: Shield, permission: "ManageRoles", group: "administration" },
];

export const NAVIGATION_GROUP_LABELS: Record<NavigationItem["group"], string> = {
  overview: "Espacio de trabajo",
  operations: "Operación",
  administration: "Administración",
};
