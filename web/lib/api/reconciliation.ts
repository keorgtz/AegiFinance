import { api } from "./client";
import type { ReconciliationCaseDto, ReconciliationDifferenceType, ReconciliationPeriodDto, ReconciliationRunResultDto, ReconciliationSettingsDto, ReconciliationStatus } from "@/types/api";

export const reconciliationApi = {
  cases: (params: { bankAccountId?: string; status?: ReconciliationStatus; limit?: number } = {}) => api.get<ReconciliationCaseDto[]>("/reconciliation/cases", params),
  run: (data: { bankAccountId: string; from: string; to: string }) => api.post<ReconciliationRunResultDto>("/reconciliation/run", data),
  confirm: (id: string, data: { differenceType: ReconciliationDifferenceType; differenceReason?: string | null }) => api.post<ReconciliationCaseDto>(`/reconciliation/cases/${id}/confirm`, data),
  reject: (id: string, reason?: string) => api.post<void>(`/reconciliation/cases/${id}/reject`, { reason }),
  reverse: (id: string, reason: string) => api.post<void>(`/reconciliation/cases/${id}/reverse`, { reason }),
  settings: () => api.get<ReconciliationSettingsDto>("/reconciliation/settings"),
  updateSettings: (data: ReconciliationSettingsDto) => api.put<ReconciliationSettingsDto>("/reconciliation/settings", data),
  periods: (bankAccountId?: string) => api.get<ReconciliationPeriodDto[]>("/reconciliation/periods", { bankAccountId }),
  closePeriod: (data: { bankAccountId: string; startDate: string; endDate: string; differenceType: ReconciliationDifferenceType; justification?: string | null }) => api.post<ReconciliationPeriodDto>("/reconciliation/periods/close", data),
};
