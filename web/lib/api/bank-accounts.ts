import { api } from "./client";
import type {
  BankAccountDto,
  BankAccountBalancesDto,
  BankAccountListDto,
  CreateBankAccountRequest,
  GetBankAccountsParams,
  PaginatedList,
  UpdateBankAccountRequest,
  RecordBankBalanceRequest,
} from "@/types/api";

function qs(params: object): string {
  const q = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
  }
  const s = q.toString();
  return s ? `?${s}` : "";
}

export const bankAccountsApi = {
  getAll: (params: GetBankAccountsParams = {}) =>
    api.get<PaginatedList<BankAccountListDto>>(`/bankaccounts${qs(params)}`),

  getById: (id: string) =>
    api.get<BankAccountDto>(`/bankaccounts/${id}`),

  getBalance: (id: string, asOfDate?: string) =>
    api.get<number>(`/bankaccounts/${id}/balance${asOfDate ? `?asOfDate=${asOfDate}` : ""}`),

  getBalances: (id: string, asOfDate?: string) =>
    api.get<BankAccountBalancesDto>(`/bankaccounts/${id}/balances${asOfDate ? `?asOfDate=${asOfDate}` : ""}`),

  create: (data: CreateBankAccountRequest) =>
    api.post<BankAccountDto>("/bankaccounts", data),

  update: (id: string, data: UpdateBankAccountRequest) =>
    api.put<BankAccountDto>(`/bankaccounts/${id}`, data),

  recordBankBalance: (id: string, data: RecordBankBalanceRequest) =>
    api.post<BankAccountBalancesDto>(`/bankaccounts/${id}/bank-balance`, data),

  delete: (id: string) =>
    api.delete<void>(`/bankaccounts/${id}`),
};
