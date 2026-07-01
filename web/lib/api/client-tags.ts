import { api } from "./client";
import type { ClientTagDto, CreateTagRequest, UpdateTagRequest } from "@/types/api";

export const clientTagsApi = {
  list() {
    return api.get<ClientTagDto[]>("/client-tags");
  },

  create(data: CreateTagRequest) {
    return api.post<ClientTagDto>("/client-tags", data);
  },

  update(id: string, data: UpdateTagRequest) {
    return api.put<ClientTagDto>(`/client-tags/${id}`, data);
  },

  delete(id: string) {
    return api.delete(`/client-tags/${id}`);
  },
};
