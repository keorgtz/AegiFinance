import { api } from "./client";
import type {
  ClientCategoryDto,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "@/types/api";

export const clientCategoriesApi = {
  list() {
    return api.get<ClientCategoryDto[]>("/client-categories");
  },

  create(data: CreateCategoryRequest) {
    return api.post<ClientCategoryDto>("/client-categories", data);
  },

  update(id: string, data: UpdateCategoryRequest) {
    return api.put<ClientCategoryDto>(`/client-categories/${id}`, data);
  },

  delete(id: string) {
    return api.delete(`/client-categories/${id}`);
  },
};
