import { api } from "./client";
import type {
  AllocationResult,
  ManualAllocateRequest,
  SubscriptionAllocationDto,
} from "@/types/api";

export const allocationsApi = {
  getByClient: (clientId: string) =>
    api.get<SubscriptionAllocationDto[]>(`/api/allocations/client/${clientId}`),

  getByBillingItem: (billingItemId: string) =>
    api.get<SubscriptionAllocationDto[]>(`/api/allocations/billing-item/${billingItemId}`),

  autoAllocate: (ledgerEntryId: string) =>
    api.post<AllocationResult>(`/api/allocations/auto-allocate/${ledgerEntryId}`, {}),

  manualAllocate: (data: ManualAllocateRequest) =>
    api.post<AllocationResult>("/api/allocations/manual", data),

  unallocate: (allocationId: string) =>
    api.post<void>(`/api/allocations/${allocationId}/unallocate`, {}),
};
