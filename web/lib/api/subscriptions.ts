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
    api.get<PaginatedList<SubscriptionListDto>>(`/api/subscriptions${toQuery(params)}`),

  getById: (id: string) =>
    api.get<SubscriptionDetailDto>(`/api/subscriptions/${id}`),

  create: (data: CreateSubscriptionRequest) =>
    api.post<SubscriptionDto>("/api/subscriptions", data),

  update: (id: string, data: UpdateSubscriptionRequest) =>
    api.put<SubscriptionDto>(`/api/subscriptions/${id}`, data),

  delete: (id: string) =>
    api.delete<void>(`/api/subscriptions/${id}`),

  suspend: (id: string, data: SubscriptionActionRequest) =>
    api.post<void>(`/api/subscriptions/${id}/suspend`, data),

  reactivate: (id: string, data: SubscriptionActionRequest) =>
    api.post<void>(`/api/subscriptions/${id}/reactivate`, data),

  cancel: (id: string, data: SubscriptionActionRequest) =>
    api.post<void>(`/api/subscriptions/${id}/cancel`, data),

  renew: (id: string, data: SubscriptionActionRequest) =>
    api.post<void>(`/api/subscriptions/${id}/renew`, data),

  changePrice: (id: string, data: ChangePriceRequest) =>
    api.post<SubscriptionDto>(`/api/subscriptions/${id}/change-price`, data),

  getPriceHistory: (id: string) =>
    api.get<SubscriptionPriceHistoryDto[]>(`/api/subscriptions/${id}/price-history`),

  getHistory: (id: string) =>
    api.get<SubscriptionChangeLogDto[]>(`/api/subscriptions/${id}/history`),

  getByClient: (clientId: string) =>
    api.get<SubscriptionListDto[]>(`/api/subscriptions/by-client/${clientId}`),

  getPermissions: (id: string) =>
    api.get<SubscriptionPermissionDto[]>(`/api/subscriptions/${id}/permissions`),

  addPermission: (id: string, data: AddSubscriptionPermissionRequest) =>
    api.post<SubscriptionPermissionDto>(`/api/subscriptions/${id}/permissions`, data),

  deletePermission: (id: string, permissionId: string) =>
    api.delete<void>(`/api/subscriptions/${id}/permissions/${permissionId}`),
};
