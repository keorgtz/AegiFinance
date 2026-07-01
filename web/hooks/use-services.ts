"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { servicesApi } from "@/lib/api/services";
import type {
  AddPriceHistoryRequest,
  CreateServiceRequest,
  GetServicesParams,
  UpdateServiceRequest,
} from "@/types/api";
import { toast } from "sonner";

export const serviceKeys = {
  all: ["services"] as const,
  lists: () => [...serviceKeys.all, "list"] as const,
  list: (p: GetServicesParams) => [...serviceKeys.lists(), p] as const,
  detail: (id: string) => [...serviceKeys.all, "detail", id] as const,
  priceHistory: (id: string) => [...serviceKeys.all, "price-history", id] as const,
};

export function useServices(params: GetServicesParams = {}) {
  return useQuery({
    queryKey: serviceKeys.list(params),
    queryFn: () => servicesApi.list(params),
  });
}

export function useService(id: string) {
  return useQuery({
    queryKey: serviceKeys.detail(id),
    queryFn: () => servicesApi.getById(id),
    enabled: !!id,
  });
}

export function useServicePriceHistory(id: string) {
  return useQuery({
    queryKey: serviceKeys.priceHistory(id),
    queryFn: () => servicesApi.getPriceHistory(id),
    enabled: !!id,
  });
}

export function useCreateService() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateServiceRequest) => servicesApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: serviceKeys.lists() });
      toast.success("Servicio creado correctamente.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useUpdateService() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateServiceRequest }) =>
      servicesApi.update(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: serviceKeys.lists() });
      qc.invalidateQueries({ queryKey: serviceKeys.detail(id) });
      toast.success("Servicio actualizado.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useDeleteService() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => servicesApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: serviceKeys.lists() });
      toast.success("Servicio eliminado.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useToggleServiceStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, active }: { id: string; active: boolean }) =>
      active ? servicesApi.activate(id) : servicesApi.deactivate(id),
    onSuccess: (_, { id, active }) => {
      qc.invalidateQueries({ queryKey: serviceKeys.lists() });
      qc.invalidateQueries({ queryKey: serviceKeys.detail(id) });
      toast.success(active ? "Servicio activado." : "Servicio desactivado.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useAddPriceHistory() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AddPriceHistoryRequest }) =>
      servicesApi.addPriceHistory(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: serviceKeys.priceHistory(id) });
      qc.invalidateQueries({ queryKey: serviceKeys.detail(id) });
      toast.success("Precio registrado en el historial.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}
