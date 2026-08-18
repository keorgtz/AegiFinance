"use client";

import { useQuery } from "@tanstack/react-query";
import { currenciesApi } from "@/lib/api/currencies";

export function useCurrencies() {
  return useQuery({
    queryKey: ["currencies"],
    queryFn: currenciesApi.list,
    staleTime: 5 * 60 * 1000,
  });
}
