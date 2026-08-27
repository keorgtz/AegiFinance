"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogClose, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectItem } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { useSubscriptions } from "@/hooks/use-subscriptions";
import { useCreateManualCharge } from "@/hooks/use-billing";

const schema = z.object({ subscriptionId: z.string().min(1, "Selecciona una suscripción"), amount: z.coerce.number().positive("Debe ser mayor a cero"),
  currency: z.string().length(3), description: z.string().min(3).max(500), chargeDate: z.string().min(1), dueDate: z.string().min(1), periodStart: z.string().min(1), periodEnd: z.string().min(1) })
  .superRefine((v, c) => { if (v.dueDate < v.chargeDate) c.addIssue({ code: "custom", path: ["dueDate"], message: "No puede ser anterior al cargo" }); if (v.periodEnd < v.periodStart) c.addIssue({ code: "custom", path: ["periodEnd"], message: "No puede ser anterior al inicio" }); });
type Values = z.infer<typeof schema>;
const iso = (value: string) => new Date(`${value}T00:00:00Z`).toISOString();

export function ManualChargeDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (open: boolean) => void }) {
  const today = new Date().toISOString().slice(0, 10);
  const [requestKey, setRequestKey] = useState("");
  const subscriptions = useSubscriptions({ pageSize: 200 });
  const mutation = useCreateManualCharge();
  const { register, handleSubmit, reset, watch, setValue, formState: { errors, isSubmitting } } = useForm<Values>({ resolver: zodResolver(schema) });
  useEffect(() => { if (open) { reset({ subscriptionId: "", amount: 0, currency: "MXN", description: "", chargeDate: today, dueDate: today, periodStart: today, periodEnd: today }); setRequestKey(crypto.randomUUID()); mutation.reset(); } }, [open]); // eslint-disable-line react-hooks/exhaustive-deps
  const selected = subscriptions.data?.items.find((item) => item.id === watch("subscriptionId"));
  useEffect(() => { if (selected) setValue("currency", selected.currency); }, [selected, setValue]);
  const submit = async (values: Values) => { try { await mutation.mutateAsync({ ...values, chargeDate: iso(values.chargeDate), dueDate: iso(values.dueDate), periodStart: iso(values.periodStart), periodEnd: iso(values.periodEnd), idempotencyKey: requestKey }); onOpenChange(false); } catch { /* Inline error below. */ } };
  return <Dialog open={open} onOpenChange={onOpenChange}><DialogContent title="Nuevo cargo manual" description="Registra un cargo excepcional sin alterar las condiciones del plan." size="md"><form onSubmit={handleSubmit(submit)} className="space-y-4">
    {mutation.error && <p role="alert" className="rounded-input bg-danger-soft p-3 text-sm text-danger">{mutation.error.message}</p>}
    <Select controlKey="billing.manual.subscription" permission="CreateBillingItems" label="Suscripción *" value={watch("subscriptionId") || ""} onValueChange={(v) => setValue("subscriptionId", v, { shouldDirty: true })} error={errors.subscriptionId?.message} placeholder="Selecciona una suscripción">{subscriptions.data?.items.map((item) => <SelectItem key={item.id} value={item.id}>{item.code} · {item.clientName}</SelectItem>)}</Select>
    <Textarea controlKey="billing.manual.description" permission="CreateBillingItems" {...register("description")} label="Concepto *" rows={2} error={errors.description?.message} />
    <div className="grid gap-3 sm:grid-cols-2"><Input controlKey="billing.manual.amount" permission="CreateBillingItems" {...register("amount")} type="number" min="0.01" step="0.01" label="Importe *" error={errors.amount?.message} /><Input controlKey="billing.manual.currency" permission="CreateBillingItems" {...register("currency")} label="Moneda" readOnly error={errors.currency?.message} /></div>
    <div className="grid gap-3 sm:grid-cols-2"><Input controlKey="billing.manual.charge-date" permission="CreateBillingItems" {...register("chargeDate")} type="date" label="Fecha del cargo *" /><Input controlKey="billing.manual.due-date" permission="CreateBillingItems" {...register("dueDate")} type="date" label="Vencimiento *" error={errors.dueDate?.message} /></div>
    <div className="grid gap-3 sm:grid-cols-2"><Input controlKey="billing.manual.period-start" permission="CreateBillingItems" {...register("periodStart")} type="date" label="Periodo desde *" /><Input controlKey="billing.manual.period-end" permission="CreateBillingItems" {...register("periodEnd")} type="date" label="Periodo hasta *" error={errors.periodEnd?.message} /></div>
    <DialogFooter><DialogClose asChild><Button controlKey="billing.manual.cancel" systemRequired type="button" variant="secondary">Cancelar</Button></DialogClose><Button controlKey="billing.manual.save" permission="CreateBillingItems" type="submit" loading={isSubmitting}>Crear cargo</Button></DialogFooter>
  </form></DialogContent></Dialog>;
}
