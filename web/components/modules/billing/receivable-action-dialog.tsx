"use client";

import { useEffect, useState } from "react";
import { Dialog, DialogClose, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useAddBillingAdjustment, useCreatePaymentPromise } from "@/hooks/use-billing";
import type { BillingItemListDto } from "@/types/api";

type Action = "credit" | "lateFee" | "promise";
const iso = (value: string) => new Date(`${value}T00:00:00Z`).toISOString();

export function ReceivableActionDialog({ item, action, onClose }: { item: BillingItemListDto; action: Action; onClose: () => void }) {
  return action === "promise" ? <PromiseDialog item={item} onClose={onClose} /> : <AdjustmentDialog item={item} type={action} onClose={onClose} />;
}

function AdjustmentDialog({ item, type, onClose }: { item: BillingItemListDto; type: "credit" | "lateFee"; onClose: () => void }) {
  const mutation = useAddBillingAdjustment(item.id); const today = new Date().toISOString().slice(0, 10);
  const [amount, setAmount] = useState(String(item.balance)); const [date, setDate] = useState(today); const [reason, setReason] = useState(""); const [key, setKey] = useState("");
  useEffect(() => { setAmount(String(item.balance)); setDate(today); setReason(""); setKey(crypto.randomUUID()); mutation.reset(); }, [item.id, type]); // eslint-disable-line react-hooks/exhaustive-deps
  const submit = async (event: React.FormEvent) => { event.preventDefault(); const value = Number(amount); if (!(value > 0) || !reason.trim()) return; try { await mutation.mutateAsync({ type: type === "credit" ? "CreditNote" : "LateFee", amount: value, effectiveDate: iso(date), reason, idempotencyKey: key }); onClose(); } catch { /* Inline error below. */ } };
  return <Dialog open onOpenChange={(open) => { if (!open) onClose(); }}><DialogContent title={type === "credit" ? "Emitir nota de crédito" : "Agregar recargo"} description={`${item.clientName} · ${item.description}`} size="sm"><form onSubmit={submit} className="space-y-4">
    <p className="rounded-input bg-surface-subtle p-3 text-sm text-foreground-secondary">Saldo actual: <strong>{item.currency} {item.balance.toFixed(2)}</strong></p>{mutation.error && <p role="alert" className="rounded-input bg-danger-soft p-3 text-sm text-danger">{mutation.error.message}</p>}
    <Input controlKey="billing.adjustment.amount" permission="AdjustBillingItems" type="number" min="0.01" max={type === "lateFee" ? undefined : item.balance} step="0.01" label="Importe *" value={amount} onChange={(e) => setAmount(e.target.value)} />
    <Input controlKey="billing.adjustment.date" permission="AdjustBillingItems" type="date" label="Fecha efectiva *" value={date} onChange={(e) => setDate(e.target.value)} />
    <Textarea controlKey="billing.adjustment.reason" permission="AdjustBillingItems" label="Motivo *" value={reason} onChange={(e) => setReason(e.target.value)} rows={3} />
    <DialogFooter><DialogClose asChild><Button controlKey="billing.adjustment.cancel" systemRequired type="button" variant="secondary">Cancelar</Button></DialogClose><Button controlKey="billing.adjustment.save" permission="AdjustBillingItems" type="submit" loading={mutation.isPending}>Guardar</Button></DialogFooter>
  </form></DialogContent></Dialog>;
}

function PromiseDialog({ item, onClose }: { item: BillingItemListDto; onClose: () => void }) {
  const mutation = useCreatePaymentPromise(item.id); const today = new Date().toISOString().slice(0, 10);
  const [amount, setAmount] = useState(String(item.balance)); const [date, setDate] = useState(today); const [notes, setNotes] = useState("");
  useEffect(() => { setAmount(String(item.balance)); setDate(today); setNotes(""); mutation.reset(); }, [item.id]); // eslint-disable-line react-hooks/exhaustive-deps
  const submit = async (event: React.FormEvent) => { event.preventDefault(); const value = Number(amount); if (!(value > 0) || !notes.trim()) return; try { await mutation.mutateAsync({ promisedAmount: value, promiseDate: iso(date), notes }); onClose(); } catch { /* Inline error below. */ } };
  return <Dialog open onOpenChange={(open) => { if (!open) onClose(); }}><DialogContent title="Registrar promesa de pago" description={`${item.clientName} · ${item.description}`} size="sm"><form onSubmit={submit} className="space-y-4">
    <p className="rounded-input bg-surface-subtle p-3 text-sm text-foreground-secondary">Saldo actual: <strong>{item.currency} {item.balance.toFixed(2)}</strong></p>{mutation.error && <p role="alert" className="rounded-input bg-danger-soft p-3 text-sm text-danger">{mutation.error.message}</p>}
    <Input controlKey="billing.promise.amount" permission="ManagePaymentPromises" type="number" min="0.01" max={item.balance} step="0.01" label="Importe prometido *" value={amount} onChange={(e) => setAmount(e.target.value)} />
    <Input controlKey="billing.promise.date" permission="ManagePaymentPromises" type="date" min={today} label="Fecha prometida *" value={date} onChange={(e) => setDate(e.target.value)} />
    <Textarea controlKey="billing.promise.notes" permission="ManagePaymentPromises" label="Notas *" value={notes} onChange={(e) => setNotes(e.target.value)} rows={3} />
    <DialogFooter><DialogClose asChild><Button controlKey="billing.promise.cancel" systemRequired type="button" variant="secondary">Cancelar</Button></DialogClose><Button controlKey="billing.promise.save" permission="ManagePaymentPromises" type="submit" loading={mutation.isPending}>Guardar promesa</Button></DialogFooter>
  </form></DialogContent></Dialog>;
}
