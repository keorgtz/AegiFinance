"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { clientTagsApi } from "@/lib/api/client-tags";
import type { CreateTagRequest, UpdateTagRequest } from "@/types/api";
import { toast } from "sonner";

export const tagKeys = {
  all: ["client-tags"] as const,
  lists: () => [...tagKeys.all, "list"] as const,
};

export function useClientTags() {
  return useQuery({ queryKey: tagKeys.lists(), queryFn: clientTagsApi.list });
}

export function useCreateTag() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateTagRequest) => clientTagsApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: tagKeys.lists() });
      toast.success("Etiqueta creada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useUpdateTag() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTagRequest }) =>
      clientTagsApi.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: tagKeys.lists() });
      toast.success("Etiqueta actualizada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useDeleteTag() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => clientTagsApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: tagKeys.lists() });
      toast.success("Etiqueta eliminada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}
