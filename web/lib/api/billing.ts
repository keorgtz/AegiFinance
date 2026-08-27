import { api } from "./client";
import type {
  BillingCycleDto,
  BillingGenerationLogDto,
  BillingGenerationResult,
  BillingItemListDto,
  CancelBillingItemRequest,
  CreateManualChargeRequest,
  AddBillingAdjustmentRequest,
  CreatePaymentPromiseRequest,
  BillingAdjustmentDto,
  PaymentPromiseDto,
  ReceivablesAgingDto,
  GenerateBillingRequest,
  GenerateForSubscriptionRequest,
  GetBillingCyclesParams,
  GetBillingItemsParams,
  GetBillingLogsParams,
  PaginatedList,
} from "@/types/api";

function qs(params: object): string {
  const q = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
  }
  const s = q.toString();
  return s ? `?${s}` : "";
}

export const billingApi = {
  // Cycles
  getCycles: (params: GetBillingCyclesParams = {}) =>
    api.get<BillingCycleDto[]>(`/billing/cycles${qs(params)}`),

  getCycleItems: (cycleId: string) =>
    api.get<PaginatedList<BillingItemListDto>>(`/billing/cycles/${cycleId}/items`),

  closeCycle: (cycleId: string) =>
    api.post<void>(`/billing/close-cycle/${cycleId}`, {}),

  reprocessCycle: (cycleId: string, onlyPending = true) =>
    api.post<BillingGenerationResult>(`/billing/reprocess/${cycleId}?onlyPending=${onlyPending}`, {}),

  // Items
  getItems: (params: GetBillingItemsParams = {}) =>
    api.get<PaginatedList<BillingItemListDto>>(`/billing/items${qs(params)}`),

  cancelItem: (itemId: string, data: CancelBillingItemRequest) =>
    api.post<void>(`/billing/cancel-item/${itemId}`, data),
  createManualCharge: (data: CreateManualChargeRequest) => api.post<BillingItemListDto>("/billing/items/manual", data),
  addAdjustment: (itemId: string, data: AddBillingAdjustmentRequest) => api.post<BillingAdjustmentDto>(`/billing/items/${itemId}/adjustments`, data),
  createPromise: (itemId: string, data: CreatePaymentPromiseRequest) => api.post<PaymentPromiseDto>(`/billing/items/${itemId}/payment-promises`, data),
  updatePromiseStatus: (promiseId: string, status: "Fulfilled" | "Broken" | "Cancelled") => api.post<void>(`/billing/payment-promises/${promiseId}/status`, { status }),
  getAging: (params: { asOfDate?: string; clientId?: string; currency?: string } = {}) => api.get<ReceivablesAgingDto>(`/billing/receivables/aging${qs(params)}`),

  // Generation
  generate: (data: GenerateBillingRequest) =>
    api.post<BillingGenerationResult>("/billing/generate", data),

  generateForSubscription: (data: GenerateForSubscriptionRequest) =>
    api.post<BillingGenerationResult>("/billing/generate-for-subscription", data),

  // Logs
  getLogs: (params: GetBillingLogsParams = {}) =>
    api.get<BillingGenerationLogDto[]>(`/billing/generation-logs${qs(params)}`),
};
