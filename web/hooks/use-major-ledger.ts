"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { majorLedgerApi } from "@/lib/api/major-ledger";

const KEY = ["major-ledger"];

export const useGeneralLedgerAccounts = (asOfDate?: string) => useQuery({
  queryKey: [...KEY, "accounts", asOfDate],
  queryFn: () => majorLedgerApi.accounts(asOfDate),
});

export const useJournalEntries = (params: { from?: string; to?: string; status?: string } = {}) => useQuery({
  queryKey: [...KEY, "entries", params],
  queryFn: () => majorLedgerApi.entries(params),
});

export const useTrialBalance = (asOfDate: string, currency = "MXN") => useQuery({
  queryKey: [...KEY, "trial-balance", asOfDate, currency],
  queryFn: () => majorLedgerApi.trialBalance(asOfDate, currency),
});

export const useAccountingPeriods = () => useQuery({
  queryKey: [...KEY, "periods"],
  queryFn: majorLedgerApi.periods,
});

export function useCloseAccountingPeriod() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: majorLedgerApi.closePeriod,
    onSuccess: () => { client.invalidateQueries({ queryKey: KEY }); toast.success("Periodo contable cerrado"); },
    onError: () => toast.error("No se pudo cerrar el periodo; revisá que no tenga borradores"),
  });
}

export function useReverseJournalEntry() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: ({ id, date, reason }: { id: string; date: string; reason: string }) => majorLedgerApi.reverse(id, { date, reason }),
    onSuccess: () => { client.invalidateQueries({ queryKey: KEY }); toast.success("Asiento revertido con trazabilidad completa"); },
    onError: () => toast.error("No se pudo revertir el asiento"),
  });
}

export function useMigrateLegacyLedger() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: majorLedgerApi.migrateLegacy,
    onSuccess: (result) => {
      client.invalidateQueries({ queryKey: KEY });
      toast[result.isReconciled ? "success" : "error"](result.isReconciled ? "Migración conciliada" : "La migración conserva una diferencia");
    },
    onError: () => toast.error("No se pudo migrar el historial"),
  });
}
