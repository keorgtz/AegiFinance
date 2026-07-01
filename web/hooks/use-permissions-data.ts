"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { permissionsApi } from "@/lib/api/permissions";
import type { CreatePermissionRequest, UpdatePermissionRequest } from "@/types/api";
import { toast } from "sonner";

export const permissionKeys = {
  all: ["permissions"] as const,
  lists: () => [...permissionKeys.all, "list"] as const,
};

export function usePermissionsData() {
  return useQuery({
    queryKey: permissionKeys.lists(),
    queryFn: () => permissionsApi.list(),
  });
}

export function useCreatePermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreatePermissionRequest) => permissionsApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: permissionKeys.lists() });
      toast.success("Permiso creado correctamente.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}

export function useUpdatePermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdatePermissionRequest }) =>
      permissionsApi.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: permissionKeys.lists() });
      toast.success("Permiso actualizado correctamente.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}

export function useDeletePermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => permissionsApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: permissionKeys.lists() });
      toast.success("Permiso eliminado.");
    },
    onError: (err: Error) => {
      toast.error(err.message);
    },
  });
}
