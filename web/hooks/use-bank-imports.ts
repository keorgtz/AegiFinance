"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { bankImportsApi } from "@/lib/api/bank-imports";

const KEY = ["bank-imports"];

export function useBankImports(bankAccountId?: string) {
  return useQuery({ queryKey: [...KEY, bankAccountId], queryFn: () => bankImportsApi.list(bankAccountId) });
}
export function useBankImportMetadata(enabled = true) {
  const adapters = useQuery({ queryKey: [...KEY, "adapters"], queryFn: bankImportsApi.adapters, enabled });
  const profiles = useQuery({ queryKey: [...KEY, "profiles"], queryFn: bankImportsApi.profiles, enabled });
  return { adapters, profiles };
}
export function usePreviewBankImport() {
  return useMutation({ mutationFn: bankImportsApi.preview, onError: (error: Error) => toast.error(error.message || "No se pudo analizar el archivo") });
}
export function useCreateBankImportProfile() {
  const client = useQueryClient();
  return useMutation({ mutationFn: bankImportsApi.createProfile, onSuccess: () => { client.invalidateQueries({ queryKey: [...KEY, "profiles"] }); toast.success("Perfil de columnas guardado"); }, onError: (error: Error) => toast.error(error.message || "No se pudo guardar el perfil") });
}
export function useConfirmBankImport() {
  const client = useQueryClient();
  return useMutation({ mutationFn: bankImportsApi.confirm, onSuccess: () => { client.invalidateQueries({ queryKey: KEY }); client.invalidateQueries({ queryKey: ["bank-accounts"] }); toast.success("Estado bancario importado"); }, onError: (error: Error) => toast.error(error.message || "No se pudo confirmar el lote") });
}
export function useRollbackBankImport() {
  const client = useQueryClient();
  return useMutation({ mutationFn: bankImportsApi.rollback, onSuccess: () => { client.invalidateQueries({ queryKey: KEY }); client.invalidateQueries({ queryKey: ["bank-accounts"] }); toast.success("Importación revertida"); }, onError: (error: Error) => toast.error(error.message || "No se pudo revertir el lote") });
}
