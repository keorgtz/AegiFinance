import { api } from "./client";
import type { FinancialReportDto, FinancialReportQuery, ReportAccountDto, ReportRunDto, ReportScheduleDto, SaveReportScheduleRequest } from "@/types/api";

const params = (query: FinancialReportQuery) => query as unknown as Record<string, string | number | boolean | undefined>;
export const reportsApi = {
  get: (query: FinancialReportQuery) => api.get<FinancialReportDto>("/reports", params(query)),
  accounts: () => api.get<ReportAccountDto[]>("/reports/accounts"),
  export: (query: FinancialReportQuery) => api.blob(`/reports/export?${new URLSearchParams(Object.entries(query).filter(([, value]) => value).map(([key, value]) => [key, String(value)])).toString()}`),
  schedules: () => api.get<ReportScheduleDto[]>("/reports/schedules"),
  createSchedule: (request: SaveReportScheduleRequest) => api.post<ReportScheduleDto>("/reports/schedules", request),
  updateSchedule: (id: string, request: SaveReportScheduleRequest) => api.put<ReportScheduleDto>(`/reports/schedules/${id}`, request),
  deleteSchedule: (id: string) => api.delete(`/reports/schedules/${id}`),
  runs: () => api.get<ReportRunDto[]>("/reports/runs"),
  runFile: (id: string) => api.blob(`/reports/runs/${id}/file`),
};
