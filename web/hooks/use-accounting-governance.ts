"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { accountingGovernanceApi } from "@/lib/api/accounting-governance";

const KEY = ["accounting-governance"] as const;
const refresh = (client: ReturnType<typeof useQueryClient>) => client.invalidateQueries({ queryKey: KEY });

export const useGovernancePeriods = (enabled = true) => useQuery({ queryKey: [...KEY, "periods"], queryFn: accountingGovernanceApi.periods, enabled });
export const usePeriodChecklist = (periodId: string) => useQuery({ queryKey: [...KEY, "checklist", periodId], queryFn: () => accountingGovernanceApi.checklist(periodId), enabled: !!periodId });
export const useIntegrityAlerts = (enabled = true) => useQuery({ queryKey: [...KEY, "alerts"], queryFn: accountingGovernanceApi.integrityAlerts, enabled });
export const useAccountingEvidence = (params: { from?: string; to?: string }, enabled = true) => useQuery({ queryKey: [...KEY, "evidence", params], queryFn: () => accountingGovernanceApi.evidence(params), enabled });
export const useReopenRequests = (enabled = true) => useQuery({ queryKey: [...KEY, "reopen-requests"], queryFn: accountingGovernanceApi.reopenRequests, enabled });

export function useCloseGovernancePeriod() { const client = useQueryClient(); return useMutation({ mutationFn: ({ id, verificationCode }: { id: string; verificationCode: string }) => accountingGovernanceApi.close(id, verificationCode), onSuccess: () => { refresh(client); toast.success("Periodo cerrado con evidencia verificable."); }, onError: (error: Error) => toast.error(error.message) }); }
export function useRequestPeriodReopen() { const client = useQueryClient(); return useMutation({ mutationFn: ({ id, reason }: { id: string; reason: string }) => accountingGovernanceApi.requestReopen(id, reason), onSuccess: () => { refresh(client); toast.success("Solicitud enviada para aprobación independiente."); }, onError: (error: Error) => toast.error(error.message) }); }
export function useReviewPeriodReopen() { const client = useQueryClient(); return useMutation({ mutationFn: ({ id, approve, comment }: { id: string; approve: boolean; comment: string }) => accountingGovernanceApi.reviewReopen(id, approve, comment), onSuccess: (_, variables) => { refresh(client); toast.success(variables.approve ? "Reapertura aprobada y periodo abierto." : "Solicitud rechazada con evidencia."); }, onError: (error: Error) => toast.error(error.message) }); }
