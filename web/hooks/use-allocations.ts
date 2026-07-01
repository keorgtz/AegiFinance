"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { allocationsApi } from "@/lib/api/allocations";
import { toast } from "sonner";
import type { ManualAllocateRequest } from "@/types/api";

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
