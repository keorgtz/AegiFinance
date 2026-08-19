"use client";

import { useMemo, useState } from "react";
import { Cog, FolderTree, Coins, RefreshCw, Tags, Pencil, Plus, Trash2, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils/cn";
import { useUiControl } from "@/lib/auth/ui-control";
import { useClientCategories, useCreateCategory, useDeleteCategory, useUpdateCategory } from "@/hooks/use-client-categories";
import { useClientTags, useCreateTag, useDeleteTag, useUpdateTag } from "@/hooks/use-client-tags";
import { useCreateServiceCategory, useDeleteServiceCategory, useServiceCategories, useUpdateServiceCategory } from "@/hooks/use-service-categories";
import { useCreateCurrency, useCurrencies, useDeleteCurrency, useUpdateCurrency } from "@/hooks/use-currencies";
import { useCreateExchangeRate, useDeleteExchangeRate, useExchangeRates, useSyncExchangeRate, useUpdateExchangeRate } from "@/hooks/use-exchange-rates";

type Section = "client-categories" | "client-tags" | "service-categories" | "currencies" | "exchange-rates";
type CatalogItem = { id: string; name: string; description?: string | null; color?: string };

const SECTIONS: Array<{ id: Section; label: string; description: string; icon: typeof Cog }> = [
  { id: "client-categories", label: "Categorías de clientes", description: "Clasificación comercial y operativa.", icon: FolderTree },
  { id: "client-tags", label: "Etiquetas de clientes", description: "Marcadores visuales para segmentar clientes.", icon: Tags },
  { id: "service-categories", label: "Categorías de servicios", description: "Organización del catálogo de planes y servicios.", icon: FolderTree },
  { id: "currencies", label: "Monedas", description: "Monedas disponibles y moneda predeterminada.", icon: Coins },
  { id: "exchange-rates", label: "Tipos de cambio", description: "Tasas manuales o sincronizadas con fecha efectiva.", icon: RefreshCw },
];

function Empty({ text }: { text: string }) {
  return <div className="rounded-card border border-dashed border-border-strong bg-surface-subtle px-5 py-10 text-center text-sm text-muted">{text}</div>;
}

function PermissionCheckbox({ controlKey, label, checked, onChange }: { controlKey: string; label: string; checked: boolean; onChange: (checked: boolean) => void }) {
  const access = useUiControl({ controlKey, permission: "ManageCurrencies" });
  if (access.hidden) return null;
  return <label className="flex min-h-11 items-center gap-2 text-sm text-foreground"><input data-ui-control="ui.app.app.settings.page.input.1" {...access.dataAttributes} type="checkbox" checked={checked} disabled={access.disabled || access.readOnly} onChange={(e) => onChange(e.target.checked)} className="h-5 w-5 accent-action" />{label}</label>;
}

function SectionButton({ item, active, onClick }: { item: (typeof SECTIONS)[number]; active: boolean; onClick: () => void }) {
  const access = useUiControl({ controlKey: `settings.section.${item.id}.open`, permission: "ViewSettings" });
  if (access.hidden) return null;
  const Icon = item.icon;
  return <button data-ui-control="ui.app.app.settings.page.button.1" {...access.dataAttributes} type="button" disabled={access.disabled || access.readOnly} onClick={onClick} className={cn("flex min-h-14 items-center gap-3 rounded-input px-3 text-left transition-ui focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-action disabled:opacity-50", active ? "bg-action-soft text-action" : "text-foreground-secondary hover:bg-surface-subtle")}><Icon className="h-5 w-5 flex-none"/><span><strong className="block text-sm">{item.label}</strong><span className="mt-0.5 hidden text-[11px] text-muted xl:block">{item.description}</span></span></button>;
}

function CatalogManager({ kind, title, description, items, loading, permission, withColor, onCreate, onUpdate, onDelete }: {
  kind: string; title: string; description: string; items: CatalogItem[]; loading: boolean; permission: string; withColor?: boolean;
  onCreate: (value: { name: string; description?: string; color?: string }) => Promise<unknown>;
  onUpdate: (id: string, value: { name: string; description?: string; color?: string }) => Promise<unknown>;
  onDelete: (id: string) => Promise<unknown>;
}) {
  const [editing, setEditing] = useState<CatalogItem | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [name, setName] = useState("");
  const [detail, setDetail] = useState(withColor ? "#6548EB" : "");
  const [saving, setSaving] = useState(false);
  const open = (item?: CatalogItem) => { setEditing(item ?? null); setName(item?.name ?? ""); setDetail(withColor ? item?.color ?? "#6548EB" : item?.description ?? ""); setFormOpen(true); };
  const close = () => { setFormOpen(false); setEditing(null); };
  const submit = async (event: React.FormEvent) => { event.preventDefault(); if (!name.trim()) return; setSaving(true); try { const value = withColor ? { name: name.trim(), color: detail } : { name: name.trim(), description: detail.trim() || undefined }; if (editing) await onUpdate(editing.id, value); else await onCreate(value); close(); } catch { /* Mutation hooks render the API error and the form stays open. */ } finally { setSaving(false); } };

  return <section aria-labelledby={`${kind}-title`}>
    <div className="mb-5 flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
      <div><h2 id={`${kind}-title`} className="font-display text-xl font-bold text-foreground">{title}</h2><p className="mt-1 text-sm text-muted">{description}</p></div>
      <Button controlKey={`settings.${kind}.create`} permission={permission} onClick={() => open()}><Plus className="h-4 w-4" />Agregar</Button>
    </div>
    {formOpen && <form onSubmit={submit} className="mb-5 rounded-card border border-action/30 bg-action-soft p-4">
      <div className="grid gap-4 sm:grid-cols-[1fr_1.5fr_auto] sm:items-end">
        <Input controlKey={`settings.${kind}.field.name`} permission={permission} label="Nombre" value={name} onChange={(e) => setName(e.target.value)} maxLength={100} required autoFocus />
        {withColor ? <Input controlKey={`settings.${kind}.field.color`} permission={permission} label="Color" type="color" value={detail} onChange={(e) => setDetail(e.target.value)} className="h-11 p-1" /> : <Input controlKey={`settings.${kind}.field.description`} permission={permission} label="Descripción" value={detail} onChange={(e) => setDetail(e.target.value)} maxLength={500} />}
        <div className="flex gap-2"><Button controlKey={`settings.${kind}.save`} permission={permission} type="submit" loading={saving}>Guardar</Button><Button controlKey={`settings.${kind}.cancel`} permission={permission} type="button" variant="ghost" size="icon" onClick={close} aria-label="Cancelar"><X className="h-4 w-4" /></Button></div>
      </div>
    </form>}
    {loading ? <div className="h-32 animate-pulse rounded-card bg-surface-subtle" /> : items.length === 0 ? <Empty text="Todavía no hay registros. Agregá el primero para habilitarlo en los formularios." /> : <div className="grid gap-3 md:grid-cols-2">
      {items.map((item) => <article key={item.id} className="flex min-h-20 items-center gap-3 rounded-card border border-border bg-surface p-4 shadow-dp1">
        {withColor && <span className="h-9 w-9 flex-none rounded-full border border-border" style={{ backgroundColor: item.color }} aria-label={`Color ${item.color}`} />}
        <div className="min-w-0 flex-1"><p className="truncate font-semibold text-foreground">{item.name}</p><p className="mt-0.5 line-clamp-2 text-xs text-muted">{withColor ? item.color : item.description || "Sin descripción"}</p></div>
        <Button controlKey={`settings.${kind}.edit`} permission={permission} variant="ghost" size="icon" onClick={() => open(item)} aria-label={`Editar ${item.name}`}><Pencil className="h-4 w-4" /></Button>
        <Button controlKey={`settings.${kind}.delete`} permission={permission} variant="danger" size="icon" onClick={() => { if (confirm(`¿Eliminar "${item.name}"?`)) void onDelete(item.id).catch(() => undefined); }} aria-label={`Eliminar ${item.name}`}><Trash2 className="h-4 w-4" /></Button>
      </article>)}
    </div>}
  </section>;
}

function CurrenciesPanel() {
  const { data = [], isLoading } = useCurrencies(); const create = useCreateCurrency(); const update = useUpdateCurrency(); const remove = useDeleteCurrency();
  const [editing, setEditing] = useState<(typeof data)[number] | null>(null); const [open, setOpen] = useState(false);
  const [form, setForm] = useState({ code: "", name: "", symbol: "", isActive: true, isDefault: false });
  const show = (item?: (typeof data)[number]) => { setEditing(item ?? null); setForm(item ? { code: item.code, name: item.name, symbol: item.symbol, isActive: item.isActive, isDefault: item.isDefault } : { code: "", name: "", symbol: "", isActive: true, isDefault: false }); setOpen(true); };
  const submit = async (e: React.FormEvent) => { e.preventDefault(); try { if (editing) await update.mutateAsync({ id: editing.id, data: { name: form.name.trim(), symbol: form.symbol.trim(), isActive: form.isActive, isDefault: form.isDefault } }); else await create.mutateAsync({ ...form, code: form.code.trim().toUpperCase(), name: form.name.trim(), symbol: form.symbol.trim() }); setOpen(false); } catch { /* Mutation hooks render the API error and the form stays open. */ } };
  return <section><div className="mb-5 flex items-start justify-between gap-3"><div><h2 className="font-display text-xl font-bold text-foreground">Monedas</h2><p className="mt-1 text-sm text-muted">Definí monedas activas y una predeterminada para importes nuevos.</p></div><Button controlKey="settings.currencies.create" permission="ManageCurrencies" onClick={() => show()}><Plus className="h-4 w-4" />Agregar</Button></div>
    {open && <form onSubmit={submit} className="mb-5 grid gap-4 rounded-card border border-action/30 bg-action-soft p-4 sm:grid-cols-3"><Input controlKey="settings.currencies.field.code" permission="ManageCurrencies" label="Código ISO" value={form.code} disabled={!!editing} maxLength={3} onChange={(e) => setForm({ ...form, code: e.target.value })} required /><Input controlKey="settings.currencies.field.name" permission="ManageCurrencies" label="Nombre" value={form.name} maxLength={100} onChange={(e) => setForm({ ...form, name: e.target.value })} required /><Input controlKey="settings.currencies.field.symbol" permission="ManageCurrencies" label="Símbolo" value={form.symbol} maxLength={10} onChange={(e) => setForm({ ...form, symbol: e.target.value })} required /><PermissionCheckbox controlKey="settings.currencies.field.active" label="Activa" checked={form.isActive} onChange={(isActive) => setForm({ ...form, isActive })} /><PermissionCheckbox controlKey="settings.currencies.field.default" label="Predeterminada" checked={form.isDefault} onChange={(isDefault) => setForm({ ...form, isDefault })} /><div className="flex gap-2"><Button controlKey="settings.currencies.save" permission="ManageCurrencies" type="submit" loading={create.isPending || update.isPending}>Guardar</Button><Button controlKey="settings.currencies.cancel" permission="ManageCurrencies" type="button" variant="ghost" onClick={() => setOpen(false)}>Cancelar</Button></div></form>}
    {isLoading ? <div className="h-32 animate-pulse rounded-card bg-surface-subtle" /> : data.length === 0 ? <Empty text="No hay monedas configuradas." /> : <div className="grid gap-3 md:grid-cols-2">{data.map((item) => <article key={item.id} className="flex items-center gap-3 rounded-card border border-border bg-surface p-4"><div className="grid h-10 w-10 place-items-center rounded-full bg-action-soft font-bold text-action">{item.symbol}</div><div className="min-w-0 flex-1"><p className="font-semibold text-foreground">{item.code} · {item.name}</p><div className="mt-1 flex gap-2"><Badge variant={item.isActive ? "jade" : "muted"}>{item.isActive ? "Activa" : "Inactiva"}</Badge>{item.isDefault && <Badge variant="periwinkle">Predeterminada</Badge>}</div></div><Button controlKey="settings.currencies.edit" permission="ManageCurrencies" variant="ghost" size="icon" onClick={() => show(item)} aria-label={`Editar ${item.code}`}><Pencil className="h-4 w-4" /></Button><Button controlKey="settings.currencies.delete" permission="ManageCurrencies" variant="danger" size="icon" onClick={() => confirm(`¿Eliminar ${item.code}?`) && remove.mutate(item.id)} aria-label={`Eliminar ${item.code}`}><Trash2 className="h-4 w-4" /></Button></article>)}</div>}
  </section>;
}

function ExchangeRatesPanel() {
  const { data = [], isLoading } = useExchangeRates(); const create = useCreateExchangeRate(); const update = useUpdateExchangeRate(); const remove = useDeleteExchangeRate(); const sync = useSyncExchangeRate();
  const [editing, setEditing] = useState<(typeof data)[number] | null>(null); const [open, setOpen] = useState(false); const [form, setForm] = useState({ currencyCode: "USD", rateToMXN: 0, rateFromMXN: 0, effectiveDate: new Date().toISOString().slice(0, 10) });
  const show = (item?: (typeof data)[number]) => { setEditing(item ?? null); setForm(item ? { currencyCode: item.currencyCode, rateToMXN: item.rateToMXN, rateFromMXN: item.rateFromMXN, effectiveDate: item.effectiveDate.slice(0, 10) } : { currencyCode: "USD", rateToMXN: 0, rateFromMXN: 0, effectiveDate: new Date().toISOString().slice(0, 10) }); setOpen(true); };
  const submit = async (e: React.FormEvent) => { e.preventDefault(); const payload = { ...form, currencyCode: form.currencyCode.trim().toUpperCase(), effectiveDate: new Date(`${form.effectiveDate}T00:00:00Z`).toISOString() }; try { if (editing) await update.mutateAsync({ id: editing.id, data: { rateToMXN: payload.rateToMXN, rateFromMXN: payload.rateFromMXN, effectiveDate: payload.effectiveDate } }); else await create.mutateAsync(payload); setOpen(false); } catch { /* Mutation hooks render the API error and the form stays open. */ } };
  return <section><div className="mb-5 flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between"><div><h2 className="font-display text-xl font-bold text-foreground">Tipos de cambio</h2><p className="mt-1 text-sm text-muted">Tasas respecto a MXN con origen y fecha efectiva auditables.</p></div><Button controlKey="settings.exchange-rates.create" permission="ManageExchangeRates" onClick={() => show()}><Plus className="h-4 w-4" />Registrar tasa</Button></div>
    {open && <form onSubmit={submit} className="mb-5 grid gap-4 rounded-card border border-action/30 bg-action-soft p-4 sm:grid-cols-2 lg:grid-cols-4"><Input controlKey="settings.exchange-rates.field.currency" permission="ManageExchangeRates" label="Moneda" value={form.currencyCode} disabled={!!editing} maxLength={3} onChange={(e) => setForm({ ...form, currencyCode: e.target.value.toUpperCase() })} required /><Input controlKey="settings.exchange-rates.field.to-mxn" permission="ManageExchangeRates" label="Tasa a MXN" type="number" min="0.000001" step="0.000001" value={form.rateToMXN} onChange={(e) => setForm({ ...form, rateToMXN: Number(e.target.value) })} required /><Input controlKey="settings.exchange-rates.field.from-mxn" permission="ManageExchangeRates" label="Tasa desde MXN" type="number" min="0.000001" step="0.000001" value={form.rateFromMXN} onChange={(e) => setForm({ ...form, rateFromMXN: Number(e.target.value) })} required /><Input controlKey="settings.exchange-rates.field.date" permission="ManageExchangeRates" label="Fecha efectiva" type="date" value={form.effectiveDate} onChange={(e) => setForm({ ...form, effectiveDate: e.target.value })} required /><div className="flex gap-2 lg:col-span-4"><Button controlKey="settings.exchange-rates.save" permission="ManageExchangeRates" type="submit" loading={create.isPending || update.isPending}>Guardar</Button><Button controlKey="settings.exchange-rates.cancel" permission="ManageExchangeRates" type="button" variant="ghost" onClick={() => setOpen(false)}>Cancelar</Button></div></form>}
    {isLoading ? <div className="h-32 animate-pulse rounded-card bg-surface-subtle" /> : data.length === 0 ? <Empty text="No hay tipos de cambio registrados." /> : <div className="grid gap-3">{data.map((item) => <article key={item.id} className="grid gap-3 rounded-card border border-border bg-surface p-4 sm:grid-cols-[1fr_auto_auto_auto] sm:items-center"><div><p className="font-semibold text-foreground">{item.currencyCode} ↔ MXN</p><p className="mt-1 text-xs text-muted">{new Date(item.effectiveDate).toLocaleDateString("es-MX")} · {item.source}</p></div><div className="text-sm"><span className="text-muted">A MXN </span><strong>{item.rateToMXN}</strong><br/><span className="text-muted">Desde MXN </span><strong>{item.rateFromMXN}</strong></div><div className="flex gap-1"><Button controlKey="settings.exchange-rates.sync" permission="SyncExchangeRates" variant="secondary" size="icon" onClick={() => sync.mutate(item.currencyCode)} aria-label={`Sincronizar ${item.currencyCode}`}><RefreshCw className="h-4 w-4" /></Button><Button controlKey="settings.exchange-rates.edit" permission="ManageExchangeRates" variant="ghost" size="icon" onClick={() => show(item)} aria-label={`Editar ${item.currencyCode}`}><Pencil className="h-4 w-4" /></Button><Button controlKey="settings.exchange-rates.delete" permission="ManageExchangeRates" variant="danger" size="icon" onClick={() => confirm(`¿Eliminar la tasa de ${item.currencyCode}?`) && remove.mutate(item.id)} aria-label={`Eliminar ${item.currencyCode}`}><Trash2 className="h-4 w-4" /></Button></div></article>)}</div>}
  </section>;
}

export default function SettingsPage() {
  const [section, setSection] = useState<Section>("client-categories");
  const clientCategories = useClientCategories(); const createCategory = useCreateCategory(); const updateCategory = useUpdateCategory(); const deleteCategory = useDeleteCategory();
  const tags = useClientTags(); const createTag = useCreateTag(); const updateTag = useUpdateTag(); const deleteTag = useDeleteTag();
  const serviceCategories = useServiceCategories(); const createServiceCategory = useCreateServiceCategory(); const updateServiceCategory = useUpdateServiceCategory(); const deleteServiceCategory = useDeleteServiceCategory();
  const active = useMemo(() => SECTIONS.find((item) => item.id === section)!, [section]);
  return <div><header className="mb-6"><div className="flex items-center gap-3"><div className="grid h-11 w-11 place-items-center rounded-card bg-action-soft text-action"><Cog className="h-5 w-5" /></div><div><h1 className="font-display text-2xl font-bold text-foreground">Configuración</h1><p className="mt-0.5 text-sm text-muted">Catálogos base que alimentan los formularios y reglas financieras.</p></div></div></header>
    <div className="grid gap-6 lg:grid-cols-[280px_minmax(0,1fr)]"><nav aria-label="Secciones de configuración" className="grid h-fit gap-1 rounded-card border border-border bg-surface p-2 lg:sticky lg:top-0">{SECTIONS.map((item) => <SectionButton key={item.id} item={item} active={section === item.id} onClick={() => setSection(item.id)} />)}</nav>
      <div className="min-w-0 rounded-card border border-border bg-surface p-4 shadow-dp1 sm:p-6" aria-label={active.label}>
        {section === "client-categories" && <CatalogManager kind="client-categories" title="Categorías de clientes" description={active.description} items={clientCategories.data ?? []} loading={clientCategories.isLoading} permission="ManageClients" onCreate={(v) => createCategory.mutateAsync(v)} onUpdate={(id, v) => updateCategory.mutateAsync({ id, data: v })} onDelete={(id) => deleteCategory.mutateAsync(id)} />}
        {section === "client-tags" && <CatalogManager kind="client-tags" title="Etiquetas de clientes" description={active.description} items={tags.data ?? []} loading={tags.isLoading} permission="ManageClients" withColor onCreate={(v) => createTag.mutateAsync({ name: v.name, color: v.color! })} onUpdate={(id, v) => updateTag.mutateAsync({ id, data: { name: v.name, color: v.color! } })} onDelete={(id) => deleteTag.mutateAsync(id)} />}
        {section === "service-categories" && <CatalogManager kind="service-categories" title="Categorías de servicios" description={active.description} items={serviceCategories.data ?? []} loading={serviceCategories.isLoading} permission="ManageServices" onCreate={(v) => createServiceCategory.mutateAsync(v)} onUpdate={(id, v) => updateServiceCategory.mutateAsync({ id, data: v })} onDelete={(id) => deleteServiceCategory.mutateAsync(id)} />}
        {section === "currencies" && <CurrenciesPanel />}{section === "exchange-rates" && <ExchangeRatesPanel />}
      </div></div>
  </div>;
}
