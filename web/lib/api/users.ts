import { api } from "./client";
import type {
  AssignRolesRequest,
  CreateUserRequest,
  GetUsersParams,
  PaginatedList,
  UpdateUserRequest,
  UserDto,
  UserSessionDto,
  UserPermissionOverrideDto,
  SetUserPermissionRequest,
} from "@/types/api";

export const usersApi = {
  list(params?: GetUsersParams) {
    return api.get<PaginatedList<UserDto>>("/users", params as Record<string, string | number | boolean | undefined>);
  },

  getById(id: string) {
    return api.get<UserDto>(`/users/${id}`);
  },

  create(data: CreateUserRequest) {
    return api.post<UserDto>("/users", data);
  },

  update(id: string, data: UpdateUserRequest) {
    return api.put<UserDto>(`/users/${id}`, data);
  },

  delete(id: string) {
    return api.delete(`/users/${id}`);
  },

  activate(id: string) {
    return api.post<void>(`/users/${id}/activate`);
  },

  deactivate(id: string) {
    return api.post<void>(`/users/${id}/deactivate`);
  },

  assignRoles(id: string, data: AssignRolesRequest) {
    return api.post<void>(`/users/${id}/roles`, data);
  },

  permissionOverrides(id: string) {
    return api.get<UserPermissionOverrideDto[]>(`/users/${id}/permissions`);
  },

  setPermission(id: string, data: SetUserPermissionRequest) {
    return api.post<void>(`/users/${id}/permissions`, data);
  },

  removePermission(id: string, permissionId: string, clientId?: string | null, subscriptionId?: string | null) {
    const query = new URLSearchParams();
    if (clientId) query.set("clientId", clientId);
    if (subscriptionId) query.set("subscriptionId", subscriptionId);
    const suffix = query.size ? `?${query.toString()}` : "";
    return api.delete<void>(`/users/${id}/permissions/${permissionId}${suffix}`);
  },

  sessions(id: string) {
    return api.get<UserSessionDto[]>(`/users/${id}/sessions`);
  },

  revokeSession(id: string, sessionId: string) {
    return api.delete<void>(`/users/${id}/sessions/${sessionId}`);
  },

  revokeAllSessions(id: string) {
    return api.delete<void>(`/users/${id}/sessions`);
  },

  unlock(id: string) { return api.post<void>(`/users/${id}/unlock`); },
  resetPassword(id: string, temporaryPassword: string) { return api.post<void>(`/users/${id}/reset-password`, { temporaryPassword }); },
};
