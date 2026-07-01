"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ledgerApi } from "@/lib/api/ledger";
import { toast } from "sonner";
import type {
  GetLedgerEntriesParams,
  RegisterAdjustmentRequest,
  RegisterExpenseRequest,
  RegisterIncomeRequest,
  RegisterTransferRequest,
} from "@/types/api";

const LEDGER_KEY = ["ledger"];

export function useLedgerEntries(params: GetLedgerEntriesParams = {}) {
  return useQuery({
    queryKey: [...LEDGER_KEY, "entries", params],
    queryFn: () => ledgerApi.getEntries(params),
  });
}

export function useRegisterIncome() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: RegisterIncomeRequest) => ledgerApi.registerIncome(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: LEDGER_KEY });
      toast.success("Ingreso registrado");
    },
    onError: () => toast.error("Error al registrar ingreso"),
  });
}

export function useRegisterExpense() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: RegisterExpenseRequest) => ledgerApi.registerExpense(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: LEDGER_KEY });
      toast.success("Egreso registrado");
    },
    onError: () => toast.error("Error al registrar egreso"),
  });
}

export function useRegisterTransfer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: RegisterTransferRequest) => ledgerApi.registerTransfer(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: LEDGER_KEY });
      toast.success("Transferencia registrada");
    },
    onError: () => toast.error("Error al registrar transferencia"),
  });
}

export function useRegisterAdjustment() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: RegisterAdjustmentRequest) => ledgerApi.registerAdjustment(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: LEDGER_KEY });
      toast.success("Ajuste registrado");
    },
    onError: () => toast.error("Error al registrar ajuste"),
  });
}

export function useReconcileLedgerEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => ledgerApi.reconcile(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: LEDGER_KEY });
      toast.success("Entrada conciliada");
    },
    onError: () => toast.error("Error al conciliar"),
  });
}

export function useUnreconcileLedgerEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => ledgerApi.unreconcile(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: LEDGER_KEY });
      toast.success("Conciliación revertida");
    },
    onError: () => toast.error("Error al desconciliar"),
  });
}
