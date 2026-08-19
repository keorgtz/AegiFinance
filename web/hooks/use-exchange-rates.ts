"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { exchangeRatesApi } from "@/lib/api/exchange-rates";
import type { ExchangeRateRequest } from "@/types/api";
import { toast } from "sonner";

const key = ["exchange-rates"] as const;
const refresh = (qc: ReturnType<typeof useQueryClient>) => qc.invalidateQueries({ queryKey: key });

export const useExchangeRates = () => useQuery({ queryKey: key, queryFn: exchangeRatesApi.list });
export function useCreateExchangeRate() { const qc = useQueryClient(); return useMutation({ mutationFn: exchangeRatesApi.create, onSuccess: () => { refresh(qc); toast.success("Tipo de cambio creado."); }, onError: (e: Error) => toast.error(e.message) }); }
export function useUpdateExchangeRate() { const qc = useQueryClient(); return useMutation({ mutationFn: ({ id, data }: { id: string; data: Omit<ExchangeRateRequest, "currencyCode"> }) => exchangeRatesApi.update(id, data), onSuccess: () => { refresh(qc); toast.success("Tipo de cambio actualizado."); }, onError: (e: Error) => toast.error(e.message) }); }
export function useDeleteExchangeRate() { const qc = useQueryClient(); return useMutation({ mutationFn: exchangeRatesApi.delete, onSuccess: () => { refresh(qc); toast.success("Tipo de cambio eliminado."); }, onError: (e: Error) => toast.error(e.message) }); }
export function useSyncExchangeRate() { const qc = useQueryClient(); return useMutation({ mutationFn: exchangeRatesApi.sync, onSuccess: () => { refresh(qc); toast.success("Tipo de cambio sincronizado."); }, onError: (e: Error) => toast.error(e.message) }); }
