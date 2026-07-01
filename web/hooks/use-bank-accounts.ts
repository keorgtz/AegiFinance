"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { bankAccountsApi } from "@/lib/api/bank-accounts";
import { toast } from "sonner";
import type {
  CreateBankAccountRequest,
  GetBankAccountsParams,
  UpdateBankAccountRequest,
} from "@/types/api";

const ACCOUNTS_KEY = ["bank-accounts"];

export function useBankAccounts(params: GetBankAccountsParams = {}) {
  return useQuery({
    queryKey: [...ACCOUNTS_KEY, params],
    queryFn: () => bankAccountsApi.getAll(params),
  });
}

export function useBankAccount(id: string) {
  return useQuery({
    queryKey: [...ACCOUNTS_KEY, id],
    queryFn: () => bankAccountsApi.getById(id),
    enabled: !!id,
  });
}

export function useBankAccountBalance(id: string, asOfDate?: string) {
  return useQuery({
    queryKey: [...ACCOUNTS_KEY, id, "balance", asOfDate],
    queryFn: () => bankAccountsApi.getBalance(id, asOfDate),
    enabled: !!id,
  });
}

export function useCreateBankAccount() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateBankAccountRequest) => bankAccountsApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ACCOUNTS_KEY });
      toast.success("Cuenta bancaria creada");
    },
    onError: () => toast.error("Error al crear cuenta bancaria"),
  });
}

export function useUpdateBankAccount() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateBankAccountRequest }) =>
      bankAccountsApi.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ACCOUNTS_KEY });
      toast.success("Cuenta bancaria actualizada");
    },
    onError: () => toast.error("Error al actualizar cuenta bancaria"),
  });
}

export function useDeleteBankAccount() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => bankAccountsApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ACCOUNTS_KEY });
      toast.success("Cuenta bancaria eliminada");
    },
    onError: () => toast.error("Error al eliminar cuenta bancaria"),
  });
}
