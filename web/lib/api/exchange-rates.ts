import { api } from "./client";
import type { ExchangeRateDto, ExchangeRateRequest } from "@/types/api";

export const exchangeRatesApi = {
  list: () => api.get<ExchangeRateDto[]>("/exchange-rates"),
  create: (data: ExchangeRateRequest) => api.post<ExchangeRateDto>("/exchange-rates", data),
  update: (id: string, data: Omit<ExchangeRateRequest, "currencyCode">) => api.put<ExchangeRateDto>(`/exchange-rates/${id}`, data),
  delete: (id: string) => api.delete<void>(`/exchange-rates/${id}`),
  sync: (currencyCode: string) => api.post<ExchangeRateDto>("/exchange-rates/sync", { currencyCode }),
};
