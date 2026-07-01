"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { serviceCategoriesApi } from "@/lib/api/service-categories";
import type { CreateServiceCategoryRequest, UpdateServiceCategoryRequest } from "@/types/api";
import { toast } from "sonner";

export const serviceCategoryKeys = {
  all: ["service-categories"] as const,
  list: () => [...serviceCategoryKeys.all, "list"] as const,
};

export function useServiceCategories() {
  return useQuery({
    queryKey: serviceCategoryKeys.list(),
    queryFn: () => serviceCategoriesApi.list(),
  });
}

export function useCreateServiceCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateServiceCategoryRequest) => serviceCategoriesApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: serviceCategoryKeys.all });
      toast.success("Categoría creada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useUpdateServiceCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateServiceCategoryRequest }) =>
      serviceCategoriesApi.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: serviceCategoryKeys.all });
      toast.success("Categoría actualizada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useDeleteServiceCategory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => serviceCategoriesApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: serviceCategoryKeys.all });
      toast.success("Categoría eliminada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}
