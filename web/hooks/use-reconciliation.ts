"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { reconciliationApi } from "@/lib/api/reconciliation";
import type { ReconciliationStatus } from "@/types/api";

const KEY = ["reconciliation"];
export function useReconciliationCases(bankAccountId?: string, status?: ReconciliationStatus) { return useQuery({ queryKey: [...KEY, "cases", bankAccountId, status], queryFn: () => reconciliationApi.cases({ bankAccountId, status }) }); }
export function useReconciliationSettings() { return useQuery({ queryKey: [...KEY, "settings"], queryFn: reconciliationApi.settings }); }
export function useReconciliationPeriods(bankAccountId?: string) { return useQuery({ queryKey: [...KEY, "periods", bankAccountId], queryFn: () => reconciliationApi.periods(bankAccountId) }); }
function invalidate(client: ReturnType<typeof useQueryClient>) { client.invalidateQueries({ queryKey: KEY }); client.invalidateQueries({ queryKey: ["ledger"] }); client.invalidateQueries({ queryKey: ["dashboard"] }); }
export function useRunReconciliation() { const client = useQueryClient(); return useMutation({ mutationFn: reconciliationApi.run, onSuccess: (result) => { invalidate(client); toast.success(`${result.suggestionsCreated} coincidencias preparadas`); }, onError: (error: Error) => toast.error(error.message) }); }
export function useConfirmReconciliation() { const client = useQueryClient(); return useMutation({ mutationFn: ({ id, ...data }: Parameters<typeof reconciliationApi.confirm>[1] & { id: string }) => reconciliationApi.confirm(id, data), onSuccess: () => { invalidate(client); toast.success("Conciliación confirmada"); }, onError: (error: Error) => toast.error(error.message) }); }
export function useRejectReconciliation() { const client = useQueryClient(); return useMutation({ mutationFn: ({ id, reason }: { id: string; reason?: string }) => reconciliationApi.reject(id, reason), onSuccess: () => { invalidate(client); toast.success("Sugerencia descartada"); }, onError: (error: Error) => toast.error(error.message) }); }
export function useReverseReconciliation() { const client = useQueryClient(); return useMutation({ mutationFn: ({ id, reason }: { id: string; reason: string }) => reconciliationApi.reverse(id, reason), onSuccess: () => { invalidate(client); toast.success("Conciliación revertida"); }, onError: (error: Error) => toast.error(error.message) }); }
export function useUpdateReconciliationSettings() { const client = useQueryClient(); return useMutation({ mutationFn: reconciliationApi.updateSettings, onSuccess: () => { client.invalidateQueries({ queryKey: [...KEY, "settings"] }); toast.success("Reglas de conciliación actualizadas"); }, onError: (error: Error) => toast.error(error.message) }); }
export function useCloseReconciliationPeriod() { const client = useQueryClient(); return useMutation({ mutationFn: reconciliationApi.closePeriod, onSuccess: () => { invalidate(client); toast.success("Periodo de conciliación cerrado"); }, onError: (error: Error) => toast.error(error.message) }); }
