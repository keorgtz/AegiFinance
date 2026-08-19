import { api } from "./client";
import type { CreateCurrencyConfigRequest, CurrencyConfigDto, UpdateCurrencyConfigRequest } from "@/types/api";

export const currenciesApi = {
  list: () => api.get<CurrencyConfigDto[]>("/currencies"),
  create: (data: CreateCurrencyConfigRequest) => api.post<CurrencyConfigDto>("/currencies", data),
  update: (id: string, data: UpdateCurrencyConfigRequest) => api.put<CurrencyConfigDto>(`/currencies/${id}`, data),
  delete: (id: string) => api.delete<void>(`/currencies/${id}`),
};
