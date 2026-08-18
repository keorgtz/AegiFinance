import { api } from "./client";
import type {
  GetLedgerEntriesParams,
  LedgerEntryDto,
  LedgerEntryListDto,
  PaginatedList,
  RegisterAdjustmentRequest,
  RegisterExpenseRequest,
  RegisterIncomeRequest,
  RegisterTransferRequest,
  TransferGroupDto,
} from "@/types/api";

function qs(params: object): string {
  const q = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v !== undefined && v !== null && v !== "") q.set(k, String(v));
  }
  const s = q.toString();
  return s ? `?${s}` : "";
}

export const ledgerApi = {
  getEntries: (params: GetLedgerEntriesParams = {}) =>
    api.get<PaginatedList<LedgerEntryListDto>>(`/ledger${qs(params)}`),

  registerIncome: (data: RegisterIncomeRequest) =>
    api.post<LedgerEntryDto>("/ledger/income", data),

  registerExpense: (data: RegisterExpenseRequest) =>
    api.post<LedgerEntryDto>("/ledger/expense", data),

  registerTransfer: (data: RegisterTransferRequest) =>
    api.post<TransferGroupDto>("/ledger/transfer", data),

  registerAdjustment: (data: RegisterAdjustmentRequest) =>
    api.post<LedgerEntryDto>("/ledger/adjustment", data),

  reconcile: (id: string) =>
    api.post<void>(`/ledger/${id}/reconcile`, {}),

  unreconcile: (id: string) =>
    api.post<void>(`/ledger/${id}/unreconcile`, {}),
};
