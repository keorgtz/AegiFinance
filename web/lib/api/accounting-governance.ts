import { api } from "./client";
import type { AccountingEvidenceSummaryDto, AccountingIntegrityAlertDto, AccountingPeriodChecklistDto, AccountingPeriodDto, AccountingPeriodReopenRequestDto } from "@/types/api";

export const accountingGovernanceApi = {
  periods: () => api.get<AccountingPeriodDto[]>("/accounting-governance/periods"),
  checklist: (periodId: string) => api.get<AccountingPeriodChecklistDto>(`/accounting-governance/periods/${periodId}/checklist`),
  close: (periodId: string, verificationCode: string) => api.post<AccountingPeriodDto>(`/accounting-governance/periods/${periodId}/close`, { verificationCode }),
  requestReopen: (periodId: string, reason: string) => api.post<AccountingPeriodReopenRequestDto>(`/accounting-governance/periods/${periodId}/reopen-requests`, { reason }),
  reopenRequests: () => api.get<AccountingPeriodReopenRequestDto[]>("/accounting-governance/reopen-requests"),
  reviewReopen: (id: string, approve: boolean, comment: string) => api.post<AccountingPeriodReopenRequestDto>(`/accounting-governance/reopen-requests/${id}/review`, { approve, comment }),
  integrityAlerts: () => api.get<AccountingIntegrityAlertDto[]>("/accounting-governance/integrity-alerts"),
  evidence: (params: { from?: string; to?: string } = {}) => api.get<AccountingEvidenceSummaryDto>("/accounting-governance/evidence", params),
};
