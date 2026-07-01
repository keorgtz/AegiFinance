"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { billingApi } from "@/lib/api/billing";
import type {
  CancelBillingItemRequest,
  GenerateBillingRequest,
  GenerateForSubscriptionRequest,
  GetBillingCyclesParams,
  GetBillingItemsParams,
  GetBillingLogsParams,
} from "@/types/api";
import { toast } from "sonner";

export const billingKeys = {
  all: ["billing"] as const,
  cycles: (p: GetBillingCyclesParams) => ["billing", "cycles", p] as const,
  cycleItems: (id: string) => ["billing", "cycle-items", id] as const,
  items: (p: GetBillingItemsParams) => ["billing", "items", p] as const,
  logs: (p: GetBillingLogsParams) => ["billing", "logs", p] as const,
};

export function useBillingCycles(params: GetBillingCyclesParams = {}) {
  return useQuery({
    queryKey: billingKeys.cycles(params),
    queryFn: () => billingApi.getCycles(params),
  });
}

export function useCycleItems(cycleId: string) {
  return useQuery({
    queryKey: billingKeys.cycleItems(cycleId),
    queryFn: () => billingApi.getCycleItems(cycleId),
    enabled: !!cycleId,
  });
}

export function useBillingItems(params: GetBillingItemsParams = {}) {
  return useQuery({
    queryKey: billingKeys.items(params),
    queryFn: () => billingApi.getItems(params),
  });
}

export function useBillingLogs(params: GetBillingLogsParams = {}) {
  return useQuery({
    queryKey: billingKeys.logs(params),
    queryFn: () => billingApi.getLogs(params),
  });
}

export function useGenerateBilling() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: GenerateBillingRequest) => billingApi.generate(data),
    onSuccess: (result) => {
      qc.invalidateQueries({ queryKey: ["billing"] });
      if (result.success) {
        toast.success(`Facturación generada: ${result.itemsGenerated} cargos creados.`);
      } else {
        toast.warning(`Generación con errores: ${result.errors.join(", ")}`);
      }
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useGenerateForSubscription() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: GenerateForSubscriptionRequest) =>
      billingApi.generateForSubscription(data),
    onSuccess: (result) => {
      qc.invalidateQueries({ queryKey: ["billing"] });
      toast.success(`${result.itemsGenerated} cargo${result.itemsGenerated !== 1 ? "s" : ""} generado${result.itemsGenerated !== 1 ? "s" : ""}.`);
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useCloseBillingCycle() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (cycleId: string) => billingApi.closeCycle(cycleId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["billing"] });
      toast.success("Ciclo cerrado correctamente.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useReprocessBillingCycle() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ cycleId, onlyPending }: { cycleId: string; onlyPending?: boolean }) =>
      billingApi.reprocessCycle(cycleId, onlyPending),
    onSuccess: (result) => {
      qc.invalidateQueries({ queryKey: ["billing"] });
      toast.success(`Reprocesado: ${result.itemsGenerated} cargos actualizados.`);
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useCancelBillingItem() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ itemId, data }: { itemId: string; data: CancelBillingItemRequest }) =>
      billingApi.cancelItem(itemId, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["billing"] });
      toast.success("Cargo cancelado.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}
