import { api } from "./client";
import type {
  AccountingPeriodDto,
  GeneralLedgerAccountDto,
  JournalEntryDto,
  LegacyMigrationResultDto,
  TrialBalanceDto,
} from "@/types/api";

export const majorLedgerApi = {
  accounts: (asOfDate?: string) => api.get<GeneralLedgerAccountDto[]>("/major-ledger/accounts", { asOfDate }),
  periods: () => api.get<AccountingPeriodDto[]>("/major-ledger/periods"),
  entries: (params: { from?: string; to?: string; status?: string } = {}) =>
    api.get<JournalEntryDto[]>("/major-ledger/journal-entries", params),
  trialBalance: (asOfDate: string, currency = "MXN") =>
    api.get<TrialBalanceDto>("/major-ledger/trial-balance", { asOfDate, currency }),
  post: (id: string) => api.post<JournalEntryDto>(`/major-ledger/journal-entries/${id}/post`),
  reverse: (id: string, body: { date: string; reason: string }) =>
    api.post<JournalEntryDto>(`/major-ledger/journal-entries/${id}/reverse`, body),
  migrateLegacy: () => api.post<LegacyMigrationResultDto>("/major-ledger/legacy-migration"),
  closePeriod: (id: string) => api.post<AccountingPeriodDto>(`/major-ledger/periods/${id}/close`),
};
