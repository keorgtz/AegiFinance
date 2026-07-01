import { api } from "./client";
import type {
  AssignRolesRequest,
  CreateUserRequest,
  GetUsersParams,
  PaginatedList,
  UpdateUserRequest,
  UserDto,
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
};
