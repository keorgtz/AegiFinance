"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { accountStatementsApi } from "@/lib/api/account-statements";
import type { CreateAccountStatementInquiryRequest, GetClientStatementParams } from "@/types/api";
import { toast } from "sonner";

const statementKeys = { all: ["account-statements"] as const, statement: (clientId: string, params: GetClientStatementParams) => ["account-statements", "statement", clientId, params] as const, inquiries: (clientId: string) => ["account-statements", "inquiries", clientId] as const };

export function useAccountStatementClients(enabled = true) {
  return useQuery({ queryKey: [...statementKeys.all, "clients"], queryFn: accountStatementsApi.clients, enabled });
}

export function useAccountStatementSubscriptions(clientId: string) {
  return useQuery({ queryKey: [...statementKeys.all, "subscriptions", clientId], queryFn: () => accountStatementsApi.subscriptions(clientId), enabled: !!clientId });
}

export function useClientStatement(clientId: string, params: GetClientStatementParams = {}) {
  return useQuery({
    queryKey: statementKeys.statement(clientId, params),
    queryFn: () => accountStatementsApi.getStatement(clientId, params),
    enabled: !!clientId,
  });
}

export function useAccountStatementInquiries(clientId: string, enabled = true) {
  return useQuery({ queryKey: statementKeys.inquiries(clientId), queryFn: () => accountStatementsApi.inquiries(clientId), enabled: !!clientId && enabled });
}

export function useCreateAccountStatementInquiry(clientId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateAccountStatementInquiryRequest) => accountStatementsApi.createInquiry(clientId, data),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: statementKeys.inquiries(clientId) }); toast.success("Aclaración enviada. Podés seguir su estado desde esta pantalla."); },
    onError: (error: Error) => toast.error(error.message),
  });
}

export function useResolveAccountStatementInquiry(clientId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, resolution }: { id: string; resolution: string }) => accountStatementsApi.resolveInquiry(id, resolution),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: statementKeys.inquiries(clientId) }); toast.success("Aclaración resuelta y respuesta publicada."); },
    onError: (error: Error) => toast.error(error.message),
  });
}

export function useClientFinancialSummary(clientId: string, params: GetClientStatementParams = {}) {
  return useQuery({
    queryKey: ["account-statements", "summary", clientId, params],
    queryFn: () => accountStatementsApi.getSummary(clientId, params),
    enabled: !!clientId,
  });
}

export function useClientMovements(clientId: string, params: GetClientStatementParams = {}) {
  return useQuery({
    queryKey: ["account-statements", "movements", clientId, params],
    queryFn: () => accountStatementsApi.getMovements(clientId, params),
    enabled: !!clientId,
  });
}
