import { api } from "./client";
import type {
  AssignPermissionsRequest,
  CreateRoleRequest,
  RoleDetailDto,
  RoleDto,
  UpdateRoleRequest,
} from "@/types/api";

export const rolesApi = {
  list() {
    return api.get<RoleDto[]>("/roles");
  },

  getById(id: string) {
    return api.get<RoleDetailDto>(`/roles/${id}`);
  },

  create(data: CreateRoleRequest) {
    return api.post<RoleDto>("/roles", data);
  },

  update(id: string, data: UpdateRoleRequest) {
    return api.put<RoleDto>(`/roles/${id}`, data);
  },

  delete(id: string) {
    return api.delete(`/roles/${id}`);
  },

  assignPermissions(id: string, data: AssignPermissionsRequest) {
    return api.post<void>(`/roles/${id}/permissions`, data);
  },
};
