"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import {
  useService,
  useServicePriceHistory,
  useToggleServiceStatus,
  useServiceVersions,
} from "@/hooks/use-services";
import { Tabs, TabList, Tab, TabPanel } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Spinner } from "@/components/ui/spinner";
import { BillingTypeBadge } from "@/components/modules/services/billing-type-badge";
import { ServiceForm } from "@/components/modules/services/service-form";
import { PriceHistoryForm } from "@/components/modules/services/price-history-form";
import { ServiceVersionForm } from "@/components/modules/services/service-version-form";
import { formatAmount, formatDate } from "@/lib/utils/format";
import { type ColumnDef } from "@tanstack/react-table";
import { DataTable } from "@/components/ui/data-table";
import type { ServicePriceHistoryDto } from "@/types/api";
import {
  ArrowLeft,
  Globe,
  Package,
  Pencil,
  Plus,
  ToggleLeft,
  ToggleRight,
} from "lucide-react";
import { cn } from "@/lib/utils/cn";

export default function ServiceDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const { data: service, isLoading } = useService(params.id);
  const { data: priceHistory = [], isLoading: historyLoading } = useServicePriceHistory(params.id);
  const { data: versions = [], isLoading: versionsLoading } = useServiceVersions(params.id);

  const toggleStatus = useToggleServiceStatus();

  const [editOpen, setEditOpen] = useState(false);
  const [priceFormOpen, setPriceFormOpen] = useState(false);
  const [versionFormOpen, setVersionFormOpen] = useState(false);

  if (isLoading) {
    return (
      <div className="flex h-40 items-center justify-center">
        <Spinner className="text-action" />
      </div>
    );
  }

  if (!service) {
    return (
      <div className="flex h-40 flex-col items-center justify-center gap-3">
        <p className="text-[14px] text-muted">Servicio no encontrado.</p>
        <Button controlKey="ui.app.app.services.id.page.button.1" variant="ghost" onClick={() => router.back()}>
          <ArrowLeft className="h-4 w-4" />
          Volver
        </Button>
      </div>
    );
  }

  const priceColumns: ColumnDef<ServicePriceHistoryDto, unknown>[] = [
    {
      accessorKey: "effectiveDate",
      header: "Fecha efectiva",
      cell: ({ getValue }) => (
        <span className="text-[13px] text-foreground-secondary">{formatDate(getValue() as string)}</span>
      ),
    },
    {
      accessorKey: "price",
      header: "Precio",
      cell: ({ row }) => (
        <span className="font-semibold text-foreground">
          {formatAmount(row.original.price, row.original.currency)}
        </span>
      ),
    },
    {
      accessorKey: "reason",
      header: "Motivo",
      cell: ({ getValue }) => (
        <span className="text-[13px] text-muted">{(getValue() as string | null) ?? "—"}</span>
      ),
    },
    {
      accessorKey: "createdAt",
      header: "Registrado",
      size: 130,
      cell: ({ getValue }) => (
        <span className="text-[12px] text-muted">{formatDate(getValue() as string)}</span>
      ),
    },
  ];

  return (
    <div>
      {/* Back + header */}
      <div className="mb-5">
        <button data-ui-control="services.detail.back"
          onClick={() => router.back()}
          className="mb-3 inline-flex items-center gap-1.5 text-[12px] text-muted hover:text-action transition-colors"
        >
          <ArrowLeft className="h-3.5 w-3.5" />
          Catálogo de servicios
        </button>

        <div className="flex flex-col items-start justify-between gap-4 md:flex-row">
          <div className="flex items-center gap-3 min-w-0">
            <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-card bg-action-soft text-action">
              <Package className="h-6 w-6" />
            </div>
            <div className="min-w-0">
              <div className="flex items-center gap-2 flex-wrap">
                <h1 className="font-display text-[22px] font-bold text-foreground truncate">
                  {service.name}
                </h1>
                <Badge variant={service.isActive ? "jade" : "terracotta"}>
                  {service.isActive ? "Activo" : "Inactivo"}
                </Badge>
                {service.isPublic && (
                  <Badge variant="periwinkle">
                    <Globe className="mr-1 h-3 w-3" />
                    Portal
                  </Badge>
                )}
              </div>
              <div className="mt-0.5 flex items-center gap-2 flex-wrap">
                <span className="font-mono text-[12px] text-muted">{service.code}</span>
                {service.categoryName && (
                  <span className="text-[12px] text-muted">· {service.categoryName}</span>
                )}
                <BillingTypeBadge billingType={service.billingType} />
              </div>
            </div>
          </div>

          <div className="grid w-full grid-cols-2 gap-2 md:flex md:w-auto md:shrink-0">
            <Button controlKey="services.detail.toggle-status"
              variant="secondary"
              size="sm"
              onClick={() => toggleStatus.mutate({ id: service.id, active: !service.isActive })}
            >
              {service.isActive ? (
                <><ToggleLeft className="h-3.5 w-3.5 text-warning" /> Desactivar</>
              ) : (
                <><ToggleRight className="h-3.5 w-3.5 text-success" /> Activar</>
              )}
            </Button>
            <Button controlKey="services.detail.edit" variant="secondary" size="sm" onClick={() => setEditOpen(true)}>
              <Pencil className="h-3.5 w-3.5" />
              Editar
            </Button>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <Tabs defaultTab="info">
        <TabList>
          <Tab controlKey="services.detail.tab.info" id="info">Información</Tab>
          <Tab controlKey="services.detail.tab.versions" id="versions">Versiones ({versions.length})</Tab>
          <Tab controlKey="services.detail.tab.prices" id="prices">
            Historial de precios
            {priceHistory.length > 0 && (
              <span className="ml-1.5 rounded-full bg-border px-1.5 py-0.5 text-[10px] font-bold text-muted">
                {priceHistory.length}
              </span>
            )}
          </Tab>
        </TabList>

        {/* ── TAB: Información ── */}
        <TabPanel id="info">
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <InfoCard title="Precio y facturación">
              <InfoRow label="Precio base">
                <span className="text-[20px] font-bold text-foreground">
                  {formatAmount(service.defaultPrice, service.currency)}
                </span>
                <span className="ml-1 text-[12px] text-muted">{service.currency}</span>
              </InfoRow>
              <InfoRow label="Tipo de facturación">
                <BillingTypeBadge billingType={service.billingType} />
              </InfoRow>
            </InfoCard>

            <InfoCard title="Configuración">
              <InfoRow label="Visible en portal del cliente">
                <Badge variant={service.isPublic ? "periwinkle" : "muted"}>
                  {service.isPublic ? "Sí" : "No"}
                </Badge>
              </InfoRow>
              <InfoRow label="Estado">
                <Badge variant={service.isActive ? "jade" : "terracotta"}>
                  {service.isActive ? "Activo" : "Inactivo"}
                </Badge>
              </InfoRow>
              {service.categoryName && (
                <InfoRow label="Categoría">
                  <span className="text-[13px] text-foreground-secondary">{service.categoryName}</span>
                </InfoRow>
              )}
            </InfoCard>

            {service.description && (
              <div className="col-span-full">
                <InfoCard title="Descripción">
                  <p className="whitespace-pre-wrap text-[13px] text-foreground-secondary">
                    {service.description}
                  </p>
                </InfoCard>
              </div>
            )}
          </div>
        </TabPanel>

        <TabPanel id="versions">
          <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div><p className="text-[13px] font-semibold text-foreground">Historial inmutable de condiciones</p><p className="text-[12px] text-muted">Cada versión define precio, vigencia, impuestos, descuento y conceptos.</p></div>
            <Button controlKey="services.version.open-create" size="sm" onClick={() => setVersionFormOpen(true)}><Plus className="h-3.5 w-3.5" />Nueva versión</Button>
          </div>
          {versionsLoading ? <Spinner className="text-action" /> : versions.length === 0 ? (
            <div className="rounded-card border border-dashed border-border p-8 text-center text-[13px] text-muted">Este plan todavía no tiene versiones.</div>
          ) : (
            <div className="space-y-3">
              {versions.map((version) => <div key={version.id} className="rounded-card border border-border bg-surface p-4 shadow-dp1">
                <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-start"><div><div className="flex items-center gap-2"><p className="font-semibold text-foreground">v{version.versionNumber} · {version.name}</p><Badge variant={version.isPublished ? "jade" : "muted"}>{version.isPublished ? "Publicada" : "Borrador"}</Badge></div><p className="mt-1 text-[12px] text-muted">Desde {formatDate(version.effectiveFrom)}{version.effectiveTo ? ` hasta ${formatDate(version.effectiveTo)}` : " · vigente"}</p></div><p className="text-[18px] font-bold text-foreground">{formatAmount(version.basePrice, version.currency)}</p></div>
                <div className="mt-3 grid grid-cols-2 gap-2 text-[12px] md:grid-cols-4"><span className="rounded-input bg-surface-subtle p-2">Descuento {version.defaultDiscountPercent}%</span><span className="rounded-input bg-surface-subtle p-2">Impuesto {version.defaultTaxPercent}%</span><span className="rounded-input bg-surface-subtle p-2">Prorrateo {version.prorationPolicy === "Daily" ? "diario" : "no"}</span><span className="rounded-input bg-surface-subtle p-2">{version.concepts.length} concepto(s)</span></div>
              </div>)}
            </div>
          )}
        </TabPanel>

        {/* ── TAB: Historial de precios ── */}
        <TabPanel id="prices">
          <div className="mb-4 flex items-center justify-between">
            <p className="text-[13px] text-muted">
              {priceHistory.length === 0
                ? "Sin cambios de precio registrados."
                : `${priceHistory.length} entrada${priceHistory.length !== 1 ? "s" : ""} en el historial`}
            </p>
            <Button controlKey="ui.app.app.services.id.page.button.6" size="sm" onClick={() => setPriceFormOpen(true)}>
              <Plus className="h-3.5 w-3.5" />
              Registrar cambio
            </Button>
          </div>

          {historyLoading ? (
            <Spinner className="text-action" />
          ) : (
            <DataTable
              columns={priceColumns}
              data={priceHistory}
              isLoading={historyLoading}
              emptyMessage="Sin historial de precios."
              getRowId={(row) => row.id}
            />
          )}
        </TabPanel>
      </Tabs>

      {/* Drawers / Dialogs */}
      <ServiceForm open={editOpen} onOpenChange={setEditOpen} editingService={service} />
      <PriceHistoryForm
        open={priceFormOpen}
        onOpenChange={setPriceFormOpen}
        serviceId={params.id}
        currentCurrency={service.currency}
      />
      <ServiceVersionForm open={versionFormOpen} onOpenChange={setVersionFormOpen} service={service} />
    </div>
  );
}

function InfoCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="rounded-card border border-border bg-surface p-4 shadow-dp1">
      <p className="mb-3 text-[11px] font-bold uppercase tracking-wider text-muted">{title}</p>
      <div className="space-y-3">{children}</div>
    </div>
  );
}

function InfoRow({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-4">
      <span className="text-[12px] text-muted">{label}</span>
      <div className="flex items-baseline gap-1">{children}</div>
    </div>
  );
}
