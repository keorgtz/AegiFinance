import { api } from "./client";
import type { CurrencyConfigDto } from "@/types/api";

export const currenciesApi = {
  list: () => api.get<CurrencyConfigDto[]>("/currencies"),
};
