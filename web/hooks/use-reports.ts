"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { reportsApi } from "@/lib/api/reports";
import type { FinancialReportQuery, SaveReportScheduleRequest } from "@/types/api";

const KEY = ["reports"] as const;
export const useFinancialReport = (query: FinancialReportQuery, enabled = true) => useQuery({ queryKey: [...KEY, "report", query], queryFn: () => reportsApi.get(query), enabled });
export const useReportAccounts = (enabled = true) => useQuery({ queryKey: [...KEY, "accounts"], queryFn: reportsApi.accounts, enabled });
export const useReportSchedules = (enabled = true) => useQuery({ queryKey: [...KEY, "schedules"], queryFn: reportsApi.schedules, enabled });
export const useReportRuns = (enabled = true) => useQuery({ queryKey: [...KEY, "runs"], queryFn: reportsApi.runs, enabled, refetchInterval: 60_000 });
export function useSaveReportSchedule() { const client = useQueryClient(); return useMutation({ mutationFn: ({ id, request }: { id?: string; request: SaveReportScheduleRequest }) => id ? reportsApi.updateSchedule(id, request) : reportsApi.createSchedule(request), onSuccess: () => { client.invalidateQueries({ queryKey: KEY }); toast.success("Reporte programado correctamente."); }, onError: (error: Error) => toast.error(error.message) }); }
export function useDeleteReportSchedule() { const client = useQueryClient(); return useMutation({ mutationFn: async (id: string) => { if (!window.confirm("¿Eliminar esta programación? Los archivos generados conservarán su evidencia.")) return false; await reportsApi.deleteSchedule(id); return true; }, onSuccess: (deleted) => { if (!deleted) return; client.invalidateQueries({ queryKey: KEY }); toast.success("Programación eliminada."); }, onError: (error: Error) => toast.error(error.message) }); }
