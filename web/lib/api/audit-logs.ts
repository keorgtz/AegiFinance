import { api } from "./client";
import type { AuditLogDto, PaginatedList } from "@/types/api";

export const auditLogsApi = {
  list: (params: { entityType?: string; entityId?: string; action?: string; fromDate?: string; toDate?: string; search?: string; pageNumber?: number; pageSize?: number }) =>
    api.get<PaginatedList<AuditLogDto>>("/audit-logs", params as Record<string, string | number | boolean | undefined>),
};
