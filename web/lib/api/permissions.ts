import { api } from "./client";
import type {
  CreatePermissionRequest,
  PermissionDto,
  UpdatePermissionRequest,
} from "@/types/api";

export const permissionsApi = {
  list() {
    return api.get<PermissionDto[]>("/permissions");
  },

  create(data: CreatePermissionRequest) {
    return api.post<PermissionDto>("/permissions", data);
  },

  update(id: string, data: UpdatePermissionRequest) {
    return api.put<PermissionDto>(`/permissions/${id}`, data);
  },

  delete(id: string) {
    return api.delete(`/permissions/${id}`);
  },
};
