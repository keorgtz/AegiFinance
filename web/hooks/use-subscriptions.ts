"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { subscriptionsApi } from "@/lib/api/subscriptions";
import type {
  AddSubscriptionPermissionRequest,
  ChangePriceRequest,
  CreateSubscriptionRequest,
  GetSubscriptionsParams,
  SubscriptionActionRequest,
  UpdateSubscriptionRequest,
} from "@/types/api";
import { toast } from "sonner";

export const subscriptionKeys = {
  all: ["subscriptions"] as const,
  lists: () => [...subscriptionKeys.all, "list"] as const,
  list: (p: GetSubscriptionsParams) => [...subscriptionKeys.lists(), p] as const,
  detail: (id: string) => [...subscriptionKeys.all, "detail", id] as const,
  priceHistory: (id: string) => [...subscriptionKeys.all, "price-history", id] as const,
  history: (id: string) => [...subscriptionKeys.all, "history", id] as const,
  byClient: (clientId: string) => [...subscriptionKeys.all, "by-client", clientId] as const,
  permissions: (id: string) => [...subscriptionKeys.all, "permissions", id] as const,
};

export function useSubscriptions(params: GetSubscriptionsParams = {}) {
  return useQuery({
    queryKey: subscriptionKeys.list(params),
    queryFn: () => subscriptionsApi.list(params),
  });
}

export function useSubscription(id: string) {
  return useQuery({
    queryKey: subscriptionKeys.detail(id),
    queryFn: () => subscriptionsApi.getById(id),
    enabled: !!id,
  });
}

export function useSubscriptionPriceHistory(id: string) {
  return useQuery({
    queryKey: subscriptionKeys.priceHistory(id),
    queryFn: () => subscriptionsApi.getPriceHistory(id),
    enabled: !!id,
  });
}

export function useSubscriptionHistory(id: string) {
  return useQuery({
    queryKey: subscriptionKeys.history(id),
    queryFn: () => subscriptionsApi.getHistory(id),
    enabled: !!id,
  });
}

export function useClientSubscriptions(clientId: string) {
  return useQuery({
    queryKey: subscriptionKeys.byClient(clientId),
    queryFn: () => subscriptionsApi.getByClient(clientId),
    enabled: !!clientId,
  });
}

export function useSubscriptionPermissions(id: string) {
  return useQuery({
    queryKey: subscriptionKeys.permissions(id),
    queryFn: () => subscriptionsApi.getPermissions(id),
    enabled: !!id,
  });
}

export function useCreateSubscription() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateSubscriptionRequest) => subscriptionsApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: subscriptionKeys.lists() });
      toast.success("Suscripción creada correctamente.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useUpdateSubscription() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSubscriptionRequest }) =>
      subscriptionsApi.update(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: subscriptionKeys.lists() });
      qc.invalidateQueries({ queryKey: subscriptionKeys.detail(id) });
      toast.success("Suscripción actualizada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useDeleteSubscription() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => subscriptionsApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: subscriptionKeys.lists() });
      toast.success("Suscripción eliminada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useSuspendSubscription() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SubscriptionActionRequest }) =>
      subscriptionsApi.suspend(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: subscriptionKeys.lists() });
      qc.invalidateQueries({ queryKey: subscriptionKeys.detail(id) });
      qc.invalidateQueries({ queryKey: subscriptionKeys.history(id) });
      toast.success("Suscripción suspendida.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useReactivateSubscription() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SubscriptionActionRequest }) =>
      subscriptionsApi.reactivate(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: subscriptionKeys.lists() });
      qc.invalidateQueries({ queryKey: subscriptionKeys.detail(id) });
      qc.invalidateQueries({ queryKey: subscriptionKeys.history(id) });
      toast.success("Suscripción reactivada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useCancelSubscription() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SubscriptionActionRequest }) =>
      subscriptionsApi.cancel(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: subscriptionKeys.lists() });
      qc.invalidateQueries({ queryKey: subscriptionKeys.detail(id) });
      qc.invalidateQueries({ queryKey: subscriptionKeys.history(id) });
      toast.success("Suscripción cancelada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useRenewSubscription() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SubscriptionActionRequest }) =>
      subscriptionsApi.renew(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: subscriptionKeys.lists() });
      qc.invalidateQueries({ queryKey: subscriptionKeys.detail(id) });
      qc.invalidateQueries({ queryKey: subscriptionKeys.history(id) });
      toast.success("Suscripción renovada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useChangeSubscriptionPrice() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ChangePriceRequest }) =>
      subscriptionsApi.changePrice(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: subscriptionKeys.detail(id) });
      qc.invalidateQueries({ queryKey: subscriptionKeys.priceHistory(id) });
      toast.success("Precio actualizado.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useAddSubscriptionPermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AddSubscriptionPermissionRequest }) =>
      subscriptionsApi.addPermission(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: subscriptionKeys.permissions(id) });
      toast.success("Visibilidad asignada al usuario.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useDeleteSubscriptionPermission() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ subscriptionId, permissionId }: { subscriptionId: string; permissionId: string }) =>
      subscriptionsApi.deletePermission(subscriptionId, permissionId),
    onSuccess: (_, { subscriptionId }) => {
      qc.invalidateQueries({ queryKey: subscriptionKeys.permissions(subscriptionId) });
      toast.success("Visibilidad removida.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}
