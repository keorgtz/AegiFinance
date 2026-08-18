import { api } from "./client";
import type {
  AllocationResult,
  ManualAllocateRequest,
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
};
