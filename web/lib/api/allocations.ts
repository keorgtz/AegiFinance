import { api } from "./client";
import type {
  AllocationResult,
  AutomaticPaymentApplicationRequest,
  ManualAllocateRequest,
  ManualPaymentApplicationRequest,
  PaymentApplicationDto,
  PaymentApplicationPriority,
  PaymentApplicationSettingsDto,
  PaymentApplicationStatus,
  SubscriptionAllocationDto,
} from "@/types/api";

export const allocationsApi = {
  getByClient: (clientId: string) =>
    api.get<SubscriptionAllocationDto[]>(`/allocations/client/${clientId}`),

  getByBillingItem: (billingItemId: string) =>
    api.get<SubscriptionAllocationDto[]>(`/allocations/billing-item/${billingItemId}`),

  autoAllocate: (ledgerEntryId: string) =>
    api.post<AllocationResult>(`/allocations/auto-allocate/${ledgerEntryId}`, {}),

  manualAllocate: (data: ManualAllocateRequest) =>
    api.post<AllocationResult>("/allocations/manual", data),

  unallocate: (allocationId: string) =>
    api.post<void>(`/allocations/${allocationId}/unallocate`, {}),
  applications: (params: { clientId?: string; status?: PaymentApplicationStatus } = {}) =>
    api.get<PaymentApplicationDto[]>("/allocations/applications", params),
  application: (id: string) => api.get<PaymentApplicationDto>(`/allocations/applications/${id}`),
  receipt: (id: string) => api.get<PaymentApplicationDto>(`/allocations/applications/${id}/receipt`),
  applyAutomatically: (data: AutomaticPaymentApplicationRequest) => api.post<AllocationResult>("/allocations/applications/auto", data),
  applyManually: (data: ManualPaymentApplicationRequest) => api.post<AllocationResult>("/allocations/applications/manual", data),
  reverseApplication: (id: string, reason: string) => api.post<void>(`/allocations/applications/${id}/reverse`, { reason }),
  reapply: (id: string, data: { priority?: PaymentApplicationPriority; preferredServiceId?: string | null; idempotencyKey: string }) => api.post<AllocationResult>(`/allocations/applications/${id}/reapply`, data),
  settings: () => api.get<PaymentApplicationSettingsDto>("/allocations/settings"),
  updateSettings: (data: PaymentApplicationSettingsDto) => api.put<PaymentApplicationSettingsDto>("/allocations/settings", data),
};
