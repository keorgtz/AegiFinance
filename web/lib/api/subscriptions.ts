import { api } from "./client";
import type {
  PaginatedList,
  SubscriptionListDto,
  SubscriptionDetailDto,
  SubscriptionDto,
  SubscriptionPriceHistoryDto,
  SubscriptionChangeLogDto,
  SubscriptionPermissionDto,
  GetSubscriptionsParams,
  CreateSubscriptionRequest,
  UpdateSubscriptionRequest,
  ChangePriceRequest,
  ChangeSubscriptionPlanRequest,
  SubscriptionActionRequest,
  AddSubscriptionPermissionRequest,
} from "@/types/api";

function toQuery(p: GetSubscriptionsParams): string {
  const q = new URLSearchParams();
  if (p.clientId) q.set("clientId", p.clientId);
  if (p.serviceId) q.set("serviceId", p.serviceId);
  if (p.status) q.set("status", p.status);
  if (p.startDateFrom) q.set("startDateFrom", p.startDateFrom);
  if (p.startDateTo) q.set("startDateTo", p.startDateTo);
  if (p.pageNumber) q.set("pageNumber", String(p.pageNumber));
  if (p.pageSize) q.set("pageSize", String(p.pageSize));
  const s = q.toString();
  return s ? `?${s}` : "";
}

export const subscriptionsApi = {
  list: (params: GetSubscriptionsParams = {}) =>
    api.get<PaginatedList<SubscriptionListDto>>(`/subscriptions${toQuery(params)}`),

  getById: (id: string) =>
    api.get<SubscriptionDetailDto>(`/subscriptions/${id}`),

  create: (data: CreateSubscriptionRequest) =>
    api.post<SubscriptionDto>("/subscriptions", data),

  update: (id: string, data: UpdateSubscriptionRequest) =>
    api.put<SubscriptionDto>(`/subscriptions/${id}`, data),

  delete: (id: string) =>
    api.delete<void>(`/subscriptions/${id}`),

  suspend: (id: string, data: SubscriptionActionRequest) =>
    api.post<void>(`/subscriptions/${id}/suspend`, data),

  reactivate: (id: string, data: SubscriptionActionRequest) =>
    api.post<void>(`/subscriptions/${id}/reactivate`, data),

  cancel: (id: string, data: SubscriptionActionRequest) =>
    api.post<void>(`/subscriptions/${id}/cancel`, data),

  renew: (id: string, data: SubscriptionActionRequest) =>
    api.post<void>(`/subscriptions/${id}/renew`, data),

  changePrice: (id: string, data: ChangePriceRequest) =>
    api.post<SubscriptionDto>(`/subscriptions/${id}/change-price`, data),

  changePlan: (id: string, data: ChangeSubscriptionPlanRequest) =>
    api.post<void>(`/subscriptions/${id}/change-plan`, data),

  getPriceHistory: (id: string) =>
    api.get<SubscriptionPriceHistoryDto[]>(`/subscriptions/${id}/price-history`),

  getHistory: (id: string) =>
    api.get<SubscriptionChangeLogDto[]>(`/subscriptions/${id}/history`),

  getByClient: (clientId: string) =>
    api.get<SubscriptionListDto[]>(`/subscriptions/by-client/${clientId}`),

  getPermissions: (id: string) =>
    api.get<SubscriptionPermissionDto[]>(`/subscriptions/${id}/permissions`),

  addPermission: (id: string, data: AddSubscriptionPermissionRequest) =>
    api.post<SubscriptionPermissionDto>(`/subscriptions/${id}/permissions`, data),

  deletePermission: (id: string, permissionId: string) =>
    api.delete<void>(`/subscriptions/${id}/permissions/${permissionId}`),
};
