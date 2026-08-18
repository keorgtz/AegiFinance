"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { usersApi } from "@/lib/api/users";
import type {
  AssignRolesRequest,
  CreateUserRequest,
  GetUsersParams,
  UpdateUserRequest,
  SetUserPermissionRequest,
} from "@/types/api";
import { toast } from "sonner";

export const userKeys = {
  all: ["users"] as const,
  lists: () => [...userKeys.all, "list"] as const,
  list: (params: GetUsersParams) => [...userKeys.lists(), params] as const,
  detail: (id: string) => [...userKeys.all, "detail", id] as const,
};

export function useUsers(params: GetUsersParams = {}) {
  return useQuery({
    queryKey: userKeys.list(params),
    queryFn: () => usersApi.list(params),
  });
}

export function useUser(id: string) {
  return useQuery({
    queryKey: userKeys.detail(id),
    queryFn: () => usersApi.getById(id),
    enabled: !!id,
  });
}

export function useCreateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateUserRequest) => usersApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: userKeys.lists() });
      toast.success("Usuario creado correctamente.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}

export function useUpdateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateUserRequest }) =>
      usersApi.update(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: userKeys.lists() });
      qc.invalidateQueries({ queryKey: userKeys.detail(id) });
      toast.success("Usuario actualizado correctamente.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}

export function useDeleteUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: userKeys.lists() });
      toast.success("Usuario eliminado.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}

export function useToggleUserActive() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, active }: { id: string; active: boolean }) =>
      active ? usersApi.activate(id) : usersApi.deactivate(id),
    onSuccess: (_, { active }) => {
      qc.invalidateQueries({ queryKey: userKeys.lists() });
      toast.success(active ? "Usuario activado." : "Usuario desactivado.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}

export function useAssignRoles() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AssignRolesRequest }) =>
      usersApi.assignRoles(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: userKeys.detail(id) });
      qc.invalidateQueries({ queryKey: userKeys.lists() });
      toast.success("Roles asignados correctamente.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}

export function useUserPermissionOverrides(userId: string) {
  return useQuery({
    queryKey: [...userKeys.detail(userId), "permission-overrides"],
    queryFn: () => usersApi.permissionOverrides(userId),
    enabled: !!userId,
  });
}

export function useSetUserPermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SetUserPermissionRequest }) => usersApi.setPermission(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: userKeys.detail(id) });
      toast.success("Excepción de permiso guardada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useRemoveUserPermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, permissionId, clientId, subscriptionId }: { id: string; permissionId: string; clientId?: string | null; subscriptionId?: string | null }) =>
      usersApi.removePermission(id, permissionId, clientId, subscriptionId),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: userKeys.detail(id) });
      toast.success("Excepción eliminada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useUserSessions(userId: string) {
  return useQuery({ queryKey: [...userKeys.detail(userId), "sessions"], queryFn: () => usersApi.sessions(userId), enabled: !!userId });
}

export function useRevokeUserSession() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, sessionId }: { id: string; sessionId: string }) => usersApi.revokeSession(id, sessionId),
    onSuccess: (_, { id }) => { qc.invalidateQueries({ queryKey: userKeys.detail(id) }); toast.success("Sesión cerrada."); },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useRevokeAllUserSessions() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersApi.revokeAllSessions(id),
    onSuccess: (_, id) => { qc.invalidateQueries({ queryKey: userKeys.detail(id) }); toast.success("Todas las sesiones fueron cerradas."); },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useUnlockUser() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: (id: string) => usersApi.unlock(id), onSuccess: () => { qc.invalidateQueries({ queryKey: userKeys.lists() }); toast.success("Usuario desbloqueado."); }, onError: (err: Error) => toast.error(err.message) });
}

export function useResetUserPassword() {
  const qc = useQueryClient();
  return useMutation({ mutationFn: ({ id, temporaryPassword }: { id: string; temporaryPassword: string }) => usersApi.resetPassword(id, temporaryPassword), onSuccess: (_, { id }) => { qc.invalidateQueries({ queryKey: userKeys.detail(id) }); qc.invalidateQueries({ queryKey: userKeys.lists() }); toast.success("Contraseña temporal guardada y sesiones cerradas."); }, onError: (err: Error) => toast.error(err.message) });
}
