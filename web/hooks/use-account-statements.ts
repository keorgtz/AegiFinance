"use client";

import { useQuery } from "@tanstack/react-query";
import { accountStatementsApi } from "@/lib/api/account-statements";
import type { GetClientStatementParams } from "@/types/api";

export function useClientStatement(clientId: string, params: GetClientStatementParams = {}) {
  return useQuery({
    queryKey: ["account-statements", "statement", clientId, params],
    queryFn: () => accountStatementsApi.getStatement(clientId, params),
    enabled: !!clientId,
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
