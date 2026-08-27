import { api } from "./client";
import type { BankImportAdapterDto, BankImportBatchDto, BankImportProfileDto } from "@/types/api";

export const bankImportsApi = {
  list: (bankAccountId?: string) => api.get<BankImportBatchDto[]>("/bank-imports", { bankAccountId }),
  get: (id: string) => api.get<BankImportBatchDto>(`/bank-imports/${id}`),
  adapters: () => api.get<BankImportAdapterDto[]>("/bank-imports/adapters"),
  profiles: () => api.get<BankImportProfileDto[]>("/bank-imports/profiles"),
  preview: (data: { bankAccountId: string; file: File; adapterCode: string; profileId?: string; columns?: Record<string, string> }) => {
    const form = new FormData();
    form.set("bankAccountId", data.bankAccountId);
    form.set("file", data.file);
    form.set("adapterCode", data.adapterCode);
    if (data.profileId) form.set("profileId", data.profileId);
    if (data.columns && Object.values(data.columns).some(Boolean)) form.set("columnsJson", JSON.stringify(data.columns));
    return api.postForm<BankImportBatchDto>("/bank-imports/preview", form);
  },
  createProfile: (data: Omit<BankImportProfileDto, "id">) => api.post<BankImportProfileDto>("/bank-imports/profiles", data),
  confirm: (id: string) => api.post<BankImportBatchDto>(`/bank-imports/${id}/confirm`, {}),
  rollback: (id: string) => api.post<void>(`/bank-imports/${id}/rollback`, {}),
};
