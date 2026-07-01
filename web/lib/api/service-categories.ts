import { api } from "./client";
import type {
  ServiceCategoryDto,
  CreateServiceCategoryRequest,
  UpdateServiceCategoryRequest,
} from "@/types/api";

export const serviceCategoriesApi = {
  list: () =>
    api.get<ServiceCategoryDto[]>("/api/service-categories"),

  create: (data: CreateServiceCategoryRequest) =>
    api.post<ServiceCategoryDto>("/api/service-categories", data),

  update: (id: string, data: UpdateServiceCategoryRequest) =>
    api.put<ServiceCategoryDto>(`/api/service-categories/${id}`, data),

  delete: (id: string) =>
    api.delete<void>(`/api/service-categories/${id}`),
};
