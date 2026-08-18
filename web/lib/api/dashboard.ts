import { api } from "./client";
import type { BankImportAttemptDto, DashboardActivityDto, DashboardAttentionDto, DashboardFilters, DashboardSummaryDto, UnreconciledBankLineDto } from "@/types/api";

const params = (filters: DashboardFilters) => filters as Record<string, string | number | boolean | undefined>;

export const dashboardApi = {
  summary: (filters: DashboardFilters) => api.get<DashboardSummaryDto>("/dashboard/summary", params(filters)),
  attention: (filters: DashboardFilters) => api.get<DashboardAttentionDto>("/dashboard/attention", params(filters)),
  activity: (filters: DashboardFilters) => api.get<DashboardActivityDto>("/dashboard/activity", params(filters)),
  importAttempts: (filters: DashboardFilters & { status?: string }) =>
    api.get<BankImportAttemptDto[]>("/dashboard/import-attempts", params(filters)),
  unreconciledLines: (filters: DashboardFilters) =>
    api.get<UnreconciledBankLineDto[]>("/dashboard/unreconciled-lines", params(filters)),
};
