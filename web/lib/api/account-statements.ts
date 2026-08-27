import { api } from "./client";
import type {
  AccountStatementDto,
  AccountStatementItemDto,
  FinancialSummaryDto,
  GetClientStatementParams,
  AccountStatementInquiryDto,
  CreateAccountStatementInquiryRequest,
  AccountStatementClientOptionDto,
  AccountStatementSubscriptionOptionDto,
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
  clients: () => api.get<AccountStatementClientOptionDto[]>("/accountstatements/clients"),

  subscriptions: (clientId: string) =>
    api.get<AccountStatementSubscriptionOptionDto[]>(`/accountstatements/client/${clientId}/subscriptions`),

  getStatement: (clientId: string, params: GetClientStatementParams = {}) =>
    api.get<AccountStatementDto>(`/accountstatements/client/${clientId}${qs(params)}`),

  getSummary: (clientId: string, params: GetClientStatementParams = {}) =>
    api.get<FinancialSummaryDto>(`/accountstatements/client/${clientId}/summary${qs(params)}`),

  getMovements: (clientId: string, params: GetClientStatementParams = {}) =>
    api.get<AccountStatementItemDto[]>(`/accountstatements/client/${clientId}/movements${qs(params)}`),

  exportPdf: (clientId: string, params: GetClientStatementParams = {}) =>
    api.blob(`/accountstatements/client/${clientId}/export.pdf${qs(params)}`),

  exportCsv: (clientId: string, params: GetClientStatementParams = {}) =>
    api.blob(`/accountstatements/client/${clientId}/export.csv${qs(params)}`),

  inquiries: (clientId: string) =>
    api.get<AccountStatementInquiryDto[]>(`/accountstatements/client/${clientId}/inquiries`),

  createInquiry: (clientId: string, data: CreateAccountStatementInquiryRequest) =>
    api.post<AccountStatementInquiryDto>(`/accountstatements/client/${clientId}/inquiries`, data),

  resolveInquiry: (id: string, resolution: string) =>
    api.put<AccountStatementInquiryDto>(`/accountstatements/inquiries/${id}/resolve`, { resolution }),
};
