import { api } from "./client";
import type {
  ServiceCategoryDto,
  CreateServiceCategoryRequest,
  UpdateServiceCategoryRequest,
} from "@/types/api";

export const serviceCategoriesApi = {
  list: () =>
    api.get<ServiceCategoryDto[]>("/service-categories"),

  create: (data: CreateServiceCategoryRequest) =>
    api.post<ServiceCategoryDto>("/service-categories", data),

  update: (id: string, data: UpdateServiceCategoryRequest) =>
    api.put<ServiceCategoryDto>(`/service-categories/${id}`, data),

  delete: (id: string) =>
    api.delete<void>(`/service-categories/${id}`),
};
