"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { currenciesApi } from "@/lib/api/currencies";
import type { CreateCurrencyConfigRequest, UpdateCurrencyConfigRequest } from "@/types/api";
import { toast } from "sonner";

export function useCurrencies() {
  return useQuery({
    queryKey: ["currencies"],
    queryFn: currenciesApi.list,
    staleTime: 5 * 60 * 1000,
  });
}

export function useCreateCurrency() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: (data: CreateCurrencyConfigRequest) => currenciesApi.create(data), onSuccess: () => { qc.invalidateQueries({ queryKey: ["currencies"] }); toast.success("Moneda creada."); }, onError: (error: Error) => toast.error(error.message) });
}

export function useUpdateCurrency() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: ({ id, data }: { id: string; data: UpdateCurrencyConfigRequest }) => currenciesApi.update(id, data), onSuccess: () => { qc.invalidateQueries({ queryKey: ["currencies"] }); toast.success("Moneda actualizada."); }, onError: (error: Error) => toast.error(error.message) });
}

export function useDeleteCurrency() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: currenciesApi.delete, onSuccess: () => { qc.invalidateQueries({ queryKey: ["currencies"] }); toast.success("Moneda eliminada."); }, onError: (error: Error) => toast.error(error.message) });
}
