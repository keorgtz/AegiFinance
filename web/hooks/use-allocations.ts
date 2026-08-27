"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { allocationsApi } from "@/lib/api/allocations";
import { toast } from "sonner";
import type { AutomaticPaymentApplicationRequest, ManualAllocateRequest, ManualPaymentApplicationRequest, PaymentApplicationPriority, PaymentApplicationStatus, PaymentApplicationSettingsDto } from "@/types/api";

const ALLOC_KEY = ["allocations"];

export function useClientAllocations(clientId: string) {
  return useQuery({
    queryKey: [...ALLOC_KEY, "client", clientId],
    queryFn: () => allocationsApi.getByClient(clientId),
    enabled: !!clientId,
  });
}

export function useBillingItemAllocations(billingItemId: string) {
  return useQuery({
    queryKey: [...ALLOC_KEY, "billing-item", billingItemId],
    queryFn: () => allocationsApi.getByBillingItem(billingItemId),
    enabled: !!billingItemId,
  });
}

export function useAutoAllocate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (ledgerEntryId: string) => allocationsApi.autoAllocate(ledgerEntryId),
    onSuccess: (result) => {
      qc.invalidateQueries({ queryKey: ALLOC_KEY });
      qc.invalidateQueries({ queryKey: ["ledger"] });
      qc.invalidateQueries({ queryKey: ["billing"] });
      if (result.success) {
        toast.success(
          `Auto-asignación completada: ${result.allocatedAmount.toFixed(2)} asignados`
        );
      } else {
        toast.warning("Asignación parcial o sin coincidencias");
      }
    },
    onError: () => toast.error("Error en la auto-asignación"),
  });
}

export function useManualAllocate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: ManualAllocateRequest) => allocationsApi.manualAllocate(data),
    onSuccess: (result) => {
      qc.invalidateQueries({ queryKey: ALLOC_KEY });
      qc.invalidateQueries({ queryKey: ["ledger"] });
      qc.invalidateQueries({ queryKey: ["billing"] });
      if (result.success) {
        toast.success("Asignación manual guardada");
      } else {
        toast.warning(result.errors.join(" / ") || "Asignación incompleta");
      }
    },
    onError: () => toast.error("Error al guardar asignación manual"),
  });
}

export function useUnallocate() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (allocationId: string) => allocationsApi.unallocate(allocationId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ALLOC_KEY });
      qc.invalidateQueries({ queryKey: ["ledger"] });
      qc.invalidateQueries({ queryKey: ["billing"] });
      toast.success("Asignación revertida");
    },
    onError: () => toast.error("Error al revertir asignación"),
  });
}

function invalidateApplications(qc: ReturnType<typeof useQueryClient>) {
  qc.invalidateQueries({ queryKey: ALLOC_KEY });
  qc.invalidateQueries({ queryKey: ["ledger"] });
  qc.invalidateQueries({ queryKey: ["billing"] });
  qc.invalidateQueries({ queryKey: ["dashboard"] });
}

export function usePaymentApplications(clientId?: string, status?: PaymentApplicationStatus) {
  return useQuery({ queryKey: [...ALLOC_KEY, "applications", clientId, status], queryFn: () => allocationsApi.applications({ clientId, status }), enabled: !!clientId });
}
export function usePaymentApplicationSettings() { return useQuery({ queryKey: [...ALLOC_KEY, "settings"], queryFn: allocationsApi.settings }); }
export function useApplyPaymentsAutomatically() { const qc = useQueryClient(); return useMutation({ mutationFn: (data: AutomaticPaymentApplicationRequest) => allocationsApi.applyAutomatically(data), onSuccess: (result) => { invalidateApplications(qc); toast.success(`Recibo ${result.receiptNumber ?? "generado"}: ${result.allocatedAmount.toFixed(2)} aplicados`); }, onError: (error: Error) => toast.error(error.message) }); }
export function useApplyPaymentsManually() { const qc = useQueryClient(); return useMutation({ mutationFn: (data: ManualPaymentApplicationRequest) => allocationsApi.applyManually(data), onSuccess: (result) => { invalidateApplications(qc); toast.success(`Recibo ${result.receiptNumber ?? "generado"} creado`); }, onError: (error: Error) => toast.error(error.message) }); }
export function useReversePaymentApplication() { const qc = useQueryClient(); return useMutation({ mutationFn: ({ id, reason }: { id: string; reason: string }) => allocationsApi.reverseApplication(id, reason), onSuccess: () => { invalidateApplications(qc); toast.success("Aplicación revertida con evidencia auditable"); }, onError: (error: Error) => toast.error(error.message) }); }
export function useReapplyPaymentApplication() { const qc = useQueryClient(); return useMutation({ mutationFn: ({ id, priority, preferredServiceId, idempotencyKey }: { id: string; priority?: PaymentApplicationPriority; preferredServiceId?: string | null; idempotencyKey: string }) => allocationsApi.reapply(id, { priority, preferredServiceId, idempotencyKey }), onSuccess: (result) => { invalidateApplications(qc); toast.success(`Reaplicación registrada en ${result.receiptNumber ?? "un nuevo recibo"}`); }, onError: (error: Error) => toast.error(error.message) }); }
export function useUpdatePaymentApplicationSettings() { const qc = useQueryClient(); return useMutation({ mutationFn: (data: PaymentApplicationSettingsDto) => allocationsApi.updateSettings(data), onSuccess: () => { qc.invalidateQueries({ queryKey: [...ALLOC_KEY, "settings"] }); toast.success("Prioridad automática actualizada"); }, onError: (error: Error) => toast.error(error.message) }); }
