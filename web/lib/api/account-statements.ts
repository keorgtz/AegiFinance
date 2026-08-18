import { api } from "./client";
import type {
  AccountStatementDto,
  AccountStatementItemDto,
  FinancialSummaryDto,
  GetClientStatementParams,
} from "@/types/api";

function qs(params: object): string {
  const q = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
  }
  const s = q.toString();
  return s ? `?${s}` : "";
}

export const accountStatementsApi = {
  getStatement: (clientId: string, params: GetClientStatementParams = {}) =>
    api.get<AccountStatementDto>(`/accountstatements/client/${clientId}${qs(params)}`),

  getSummary: (clientId: string, params: GetClientStatementParams = {}) =>
    api.get<FinancialSummaryDto>(`/accountstatements/client/${clientId}/summary${qs(params)}`),

  getMovements: (clientId: string, params: GetClientStatementParams = {}) =>
    api.get<AccountStatementItemDto[]>(`/accountstatements/client/${clientId}/movements${qs(params)}`),
};
