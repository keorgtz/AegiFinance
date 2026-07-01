"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { clientCategoriesApi } from "@/lib/api/client-categories";
import type { CreateCategoryRequest, UpdateCategoryRequest } from "@/types/api";
import { toast } from "sonner";

export const categoryKeys = {
  all: ["client-categories"] as const,
  lists: () => [...categoryKeys.all, "list"] as const,
};

export function useClientCategories() {
  return useQuery({ queryKey: categoryKeys.lists(), queryFn: clientCategoriesApi.list });
}

export function useCreateCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCategoryRequest) => clientCategoriesApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: categoryKeys.lists() });
      toast.success("Categoría creada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useUpdateCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCategoryRequest }) =>
      clientCategoriesApi.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: categoryKeys.lists() });
      toast.success("Categoría actualizada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useDeleteCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => clientCategoriesApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: categoryKeys.lists() });
      toast.success("Categoría eliminada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}
