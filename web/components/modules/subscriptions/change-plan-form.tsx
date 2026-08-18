"use client";

import { useMemo, useState } from "react";
import { Drawer, DrawerContent, DrawerClose } from "@/components/ui/drawer";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import { Select, SelectItem } from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useServices, useServiceVersions } from "@/hooks/use-services";
import { useChangeSubscriptionPlan } from "@/hooks/use-subscriptions";

interface Props { open: boolean; onOpenChange: (value: boolean) => void; subscriptionId: string; currentServiceId: string; }

export function ChangePlanForm({ open, onOpenChange, subscriptionId, currentServiceId }: Props) {
  const [serviceId, setServiceId] = useState(currentServiceId);
  const [versionId, setVersionId] = useState("");
  const [effectiveDate, setEffectiveDate] = useState(new Date().toISOString().slice(0, 10));
  const [reason, setReason] = useState("");
  const { data: services } = useServices({ pageSize: 100, isActive: true });
  const { data: versions = [] } = useServiceVersions(serviceId);
  const changePlan = useChangeSubscriptionPlan();
  const options = useMemo(() => services?.items.map(s => ({ value: s.id, label: s.name, description: s.code })) ?? [], [services]);

  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!versionId) return;
    await changePlan.mutateAsync({ id: subscriptionId, data: { serviceVersionId: versionId, effectiveDate: new Date(`${effectiveDate}T00:00:00Z`).toISOString(), reason: reason || null } });
    onOpenChange(false);
  };

  return <Drawer open={open} onOpenChange={onOpenChange}><DrawerContent title="Cambiar plan" description="El cambio se aplica por vigencia y no modifica cargos emitidos." width="md" footer={<><DrawerClose asChild><Button controlKey="subscriptions.plan-change.cancel" variant="secondary">Cancelar</Button></DrawerClose><Button controlKey="subscriptions.plan-change.schedule" form="change-plan-form" type="submit" disabled={!versionId} loading={changePlan.isPending}>Programar cambio</Button></>}>
    <form id="change-plan-form" onSubmit={submit} className="space-y-4">
      <Combobox controlKey="subscriptions.plan-change.service" label="Plan" value={serviceId} onValueChange={(value) => { setServiceId(value); setVersionId(""); }} options={options} />
      <Select controlKey="subscriptions.plan-change.version" label="Versión publicada" value={versionId} onValueChange={setVersionId}>
        {versions.filter(v => v.isPublished).map(v => <SelectItem key={v.id} value={v.id}>v{v.versionNumber} · {v.name} · {v.currency} {v.basePrice}</SelectItem>)}
      </Select>
      <Input controlKey="subscriptions.plan-change.effective-date" type="date" label="Vigente desde" value={effectiveDate} onChange={(event) => setEffectiveDate(event.target.value)} required />
      <Textarea controlKey="subscriptions.plan-change.reason" label="Motivo" value={reason} onChange={(event) => setReason(event.target.value)} rows={3} />
    </form>
  </DrawerContent></Drawer>;
}
