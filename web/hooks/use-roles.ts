"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { rolesApi } from "@/lib/api/roles";
import type {
  AssignPermissionsRequest,
  CreateRoleRequest,
  UpdateRoleRequest,
} from "@/types/api";
import { toast } from "sonner";

export const roleKeys = {
  all: ["roles"] as const,
  lists: () => [...roleKeys.all, "list"] as const,
  detail: (id: string) => [...roleKeys.all, "detail", id] as const,
};

export function useRoles() {
  return useQuery({
    queryKey: roleKeys.lists(),
    queryFn: () => rolesApi.list(),
  });
}

export function useRole(id: string) {
  return useQuery({
    queryKey: roleKeys.detail(id),
    queryFn: () => rolesApi.getById(id),
    enabled: !!id,
  });
}

export function useCreateRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateRoleRequest) => rolesApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: roleKeys.lists() });
      toast.success("Rol creado correctamente.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}

export function useUpdateRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateRoleRequest }) =>
      rolesApi.update(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: roleKeys.lists() });
      qc.invalidateQueries({ queryKey: roleKeys.detail(id) });
      toast.success("Rol actualizado correctamente.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}

export function useDeleteRole() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => rolesApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: roleKeys.lists() });
      toast.success("Rol eliminado.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}

export function useAssignPermissions() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AssignPermissionsRequest }) =>
      rolesApi.assignPermissions(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: roleKeys.detail(id) });
      toast.success("Permisos asignados correctamente.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}
