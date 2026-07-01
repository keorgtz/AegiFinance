import { api } from "./client";
import type {
  PaginatedList,
  ServiceListDto,
  ServiceDetailDto,
  ServiceDto,
  ServicePriceHistoryDto,
  GetServicesParams,
  CreateServiceRequest,
  UpdateServiceRequest,
  AddPriceHistoryRequest,
} from "@/types/api";

function toQuery(params: GetServicesParams): string {
  const q = new URLSearchParams();
  if (params.searchTerm) q.set("searchTerm", params.searchTerm);
  if (params.categoryId) q.set("categoryId", params.categoryId);
  if (params.billingType) q.set("billingType", params.billingType);
  if (params.isActive !== undefined) q.set("isActive", String(params.isActive));
  if (params.pageNumber) q.set("pageNumber", String(params.pageNumber));
  if (params.pageSize) q.set("pageSize", String(params.pageSize));
  const s = q.toString();
  return s ? `?${s}` : "";
}

export const servicesApi = {
  list: (params: GetServicesParams = {}) =>
    api.get<PaginatedList<ServiceListDto>>(`/api/services${toQuery(params)}`),

  getById: (id: string) =>
    api.get<ServiceDetailDto>(`/api/services/${id}`),

  create: (data: CreateServiceRequest) =>
    api.post<ServiceDto>("/api/services", data),

  update: (id: string, data: UpdateServiceRequest) =>
    api.put<ServiceDto>(`/api/services/${id}`, data),

  delete: (id: string) =>
    api.delete<void>(`/api/services/${id}`),

  activate: (id: string) =>
    api.post<void>(`/api/services/${id}/activate`, {}),

  deactivate: (id: string) =>
    api.post<void>(`/api/services/${id}/deactivate`, {}),

  getPriceHistory: (id: string) =>
    api.get<ServicePriceHistoryDto[]>(`/api/services/${id}/price-history`),

  addPriceHistory: (id: string, data: AddPriceHistoryRequest) =>
    api.post<ServicePriceHistoryDto>(`/api/services/${id}/price-history`, data),
};
