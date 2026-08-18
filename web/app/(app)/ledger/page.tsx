"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { type ColumnDef } from "@tanstack/react-table";
import { useQuery } from "@tanstack/react-query";
import {
  useLedgerEntries,
  useReconcileLedgerEntry,
  useUnreconcileLedgerEntry,
} from "@/hooks/use-ledger";
import {
  useBankAccounts,
  useDeleteBankAccount,
} from "@/hooks/use-bank-accounts";
import { Tabs, TabList, Tab, TabPanel } from "@/components/ui/tabs";
import { DataTable } from "@/components/ui/data-table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Pagination } from "@/components/ui/pagination";
import { Select, SelectItem } from "@/components/ui/select";
import { EntryTypeBadge } from "@/components/modules/ledger/entry-type-badge";
import { BankAccountForm } from "@/components/modules/ledger/bank-account-form";
import { IncomeForm } from "@/components/modules/ledger/income-form";
import { ExpenseForm } from "@/components/modules/ledger/expense-form";
import { TransferForm } from "@/components/modules/ledger/transfer-form";
import { AdjustmentForm } from "@/components/modules/ledger/adjustment-form";
import { Can } from "@/lib/auth/can";
import { dashboardApi } from "@/lib/api/dashboard";
import { formatAmount, formatDate, formatDateTime } from "@/lib/utils/format";
import type {
  BankAccountDto,
  BankAccountListDto,
  LedgerEntryListDto,
  LedgerEntryType,
} from "@/types/api";
import {
  ArrowDownLeft,
  ArrowUpRight,
  CheckCircle2,
  Circle,
  Edit2,
  MoreHorizontal,
  Plus,
  RefreshCw,
  Trash2,
  TrendingDown,
  TrendingUp,
  Wallet,
  FileWarning,
} from "lucide-react";
import * as DropdownMenu from "@radix-ui/react-dropdown-menu";
import { cn } from "@/lib/utils/cn";

const PAGE_SIZE = 20;

const ENTRY_TYPES: { value: LedgerEntryType; label: string }[] = [
  { value: "Income", label: "Ingreso" },
  { value: "Expense", label: "Egreso" },
  { value: "TransferIn", label: "Transferencia entrada" },
  { value: "TransferOut", label: "Transferencia salida" },
  { value: "Adjustment", label: "Ajuste" },
];

type EntryAction = "income" | "expense" | "transfer" | "adjustment";

export default function LedgerPage() {
  // ── Ledger (movimientos) state ─────────────────────────────────────────────
  const [entriesPage, setEntriesPage] = useState(1);
  const [entryTypeFilter, setEntryTypeFilter] = useState<LedgerEntryType | "">("");
  const [accountFilter, setAccountFilter] = useState("");
  const [dateFromFilter, setDateFromFilter] = useState("");
  const [dateToFilter, setDateToFilter] = useState("");
  const [clientIdFilter, setClientIdFilter] = useState("");
  const [currencyFilter, setCurrencyFilter] = useState("");
  const [reconciledFilter, setReconciledFilter] = useState<"" | "true" | "false">("");
  const [hasUnappliedBalance, setHasUnappliedBalance] = useState(false);
  const [showImportAttempts, setShowImportAttempts] = useState(false);
  const [showReconciliationLines, setShowReconciliationLines] = useState(false);

  // ── Bank accounts state ────────────────────────────────────────────────────
  const [accountsPage, setAccountsPage] = useState(1);
  const [accountStatusFilter, setAccountStatusFilter] = useState<"" | "true" | "false">("");
  const [editingAccount, setEditingAccount] = useState<BankAccountDto | null>(null);
  const [accountFormOpen, setAccountFormOpen] = useState(false);

  // ── Register dialogs ───────────────────────────────────────────────────────
  const [openAction, setOpenAction] = useState<EntryAction | null>(null);
  const [defaultAccountId, setDefaultAccountId] = useState("");

  // ── Keyboard shortcuts ─────────────────────────────────────────────────────
  const searchRef = useRef<HTMLInputElement>(null);
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === "/" && !(e.target instanceof HTMLInputElement) && !(e.target instanceof HTMLTextAreaElement)) {
        e.preventDefault();
        searchRef.current?.focus();
      }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, []);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    setAccountFilter(params.get("bankAccountId") ?? "");
    setDateFromFilter(params.get("dateFrom") ?? "");
    setDateToFilter(params.get("dateTo") ?? "");
    setClientIdFilter(params.get("clientId") ?? "");
    setCurrencyFilter(params.get("currency") ?? "");
    const reconciled = params.get("isReconciled") ?? params.get("reconciled") ?? "";
    setReconciledFilter(reconciled === "true" || reconciled === "false" ? reconciled : "");
    setEntryTypeFilter((params.get("entryType") as LedgerEntryType | null) ?? "");
    setHasUnappliedBalance(params.get("hasUnappliedBalance") === "true");
    setShowImportAttempts(params.get("view") === "imports");
    setShowReconciliationLines(params.get("view") === "reconciliation");
  }, []);

  // ── Data ───────────────────────────────────────────────────────────────────
  const { data: entriesData, isLoading: entriesLoading } = useLedgerEntries({
    entryType: entryTypeFilter || undefined,
    bankAccountId: accountFilter || undefined,
    dateFrom: dateFromFilter || undefined,
    dateTo: dateToFilter || undefined,
    clientId: clientIdFilter || undefined,
    currency: currencyFilter || undefined,
    isReconciled: reconciledFilter === "" ? undefined : reconciledFilter === "true",
    hasUnappliedBalance: hasUnappliedBalance || undefined,
    pageNumber: entriesPage,
    pageSize: PAGE_SIZE,
  });

  const { data: accountsPage_data, isLoading: accountsLoading } = useBankAccounts({
    isActive: accountStatusFilter === "" ? undefined : accountStatusFilter === "true",
    pageNumber: accountsPage,
    pageSize: 15,
  });

  const importAttempts = useQuery({
    queryKey: ["dashboard", "import-attempts", dateFromFilter, dateToFilter, accountFilter],
    queryFn: () => dashboardApi.importAttempts({
      from: dateFromFilter || undefined,
      to: dateToFilter || undefined,
      bankAccountId: accountFilter || undefined,
      currency: currencyFilter || "MXN",
      status: "Failed",
    }),
    enabled: showImportAttempts,
  });

  const reconciliationLines = useQuery({
    queryKey: ["dashboard", "unreconciled-lines", dateFromFilter, dateToFilter, accountFilter],
    queryFn: () => dashboardApi.unreconciledLines({
      from: dateFromFilter || undefined,
      to: dateToFilter || undefined,
      bankAccountId: accountFilter || undefined,
      currency: currencyFilter || "MXN",
    }),
    enabled: showReconciliationLines,
  });

  // Also load all active accounts for filter dropdowns + form defaults
  const { data: allAccountsPage } = useBankAccounts({ isActive: true, pageSize: 100 });
  const allAccounts: BankAccountListDto[] = allAccountsPage?.items ?? [];

  const reconcile = useReconcileLedgerEntry();
  const unreconcile = useUnreconcileLedgerEntry();
  const deleteAccount = useDeleteBankAccount();

  const hasEntryFilters = !!entryTypeFilter || !!accountFilter || !!dateFromFilter || !!dateToFilter || !!clientIdFilter || !!currencyFilter || !!reconciledFilter || hasUnappliedBalance;

  const openActionDialog = useCallback((action: EntryAction, accountId = "") => {
    setDefaultAccountId(accountId);
    setOpenAction(action);
  }, []);

  /* ── Columns: ledger entries ─────────────────────────────────────────── */
  const entryColumns: ColumnDef<LedgerEntryListDto, unknown>[] = [
    {
      accessorKey: "date",
      header: "Fecha",
      size: 105,
      cell: ({ getValue }) => (
        <span className="text-[12px] text-muted">{formatDate(getValue() as string)}</span>
      ),
    },
    {
      id: "type",
      header: "Tipo",
      size: 160,
      cell: ({ row }) => <EntryTypeBadge type={row.original.entryType} />,
    },
    {
      id: "description",
      header: "Descripción",
      cell: ({ row }) => {
        const e = row.original;
        return (
          <div className="min-w-0">
            <p className="truncate font-medium text-foreground">{e.description}</p>
            <p className="text-[11px] text-muted">
              {e.bankAccountName}
              {e.clientName ? ` · ${e.clientName}` : ""}
              {e.reference ? ` · Ref: ${e.reference}` : ""}
            </p>
          </div>
        );
      },
    },
    {
      accessorKey: "amount",
      header: "Importe",
      size: 130,
      cell: ({ row }) => {
        const e = row.original;
        const isOut = e.entryType === "Expense" || e.entryType === "TransferOut";
        const isIn = e.entryType === "Income" || e.entryType === "TransferIn";
        return (
          <span className={cn("flex items-center gap-1 font-semibold text-[13px]",
            isIn ? "text-success" : isOut ? "text-danger" : "text-foreground"
          )}>
            {isIn && <TrendingUp className="h-3.5 w-3.5" />}
            {isOut && <TrendingDown className="h-3.5 w-3.5" />}
            {formatAmount(e.amount, e.currency)}
          </span>
        );
      },
    },
    {
      accessorKey: "isReconciled",
      header: "Conciliado",
      size: 110,
      cell: ({ getValue, row }) => {
        const reconciled = getValue() as boolean;
        return reconciled ? (
          <span className="flex items-center gap-1 text-[12px] font-semibold text-success">
            <CheckCircle2 className="h-3.5 w-3.5" />
            Sí
          </span>
        ) : (
          <span className="flex items-center gap-1 text-[12px] text-muted">
            <Circle className="h-3.5 w-3.5" />
            No
          </span>
        );
      },
    },
    {
      id: "actions",
      size: 48,
      cell: ({ row }) => {
        const e = row.original;
        return (
          <Can permission="ManageReconciliation">
            <DropdownMenu.Root>
              <DropdownMenu.Trigger asChild>
                <Button controlKey="ui.app.app.ledger.page.button.1" variant="ghost" size="icon" aria-label="Opciones">
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenu.Trigger>
              <DropdownMenu.Portal>
                <DropdownMenu.Content
                  className="z-50 min-w-[160px] overflow-hidden rounded-table border border-border bg-surface p-1 shadow-dp2"
                  sideOffset={4}
                  align="end"
                >
                  {e.isReconciled ? (
                    <DropdownMenu.Item
                      className="flex cursor-pointer items-center gap-2 rounded-[6px] px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:outline-none"
                      onSelect={() => unreconcile.mutate(e.id)}
                    >
                      <RefreshCw className="h-3.5 w-3.5" />
                      Deshacer conciliación
                    </DropdownMenu.Item>
                  ) : (
                    <DropdownMenu.Item
                      className="flex cursor-pointer items-center gap-2 rounded-[6px] px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:outline-none"
                      onSelect={() => reconcile.mutate(e.id)}
                    >
                      <CheckCircle2 className="h-3.5 w-3.5" />
                      Marcar conciliado
                    </DropdownMenu.Item>
                  )}
                </DropdownMenu.Content>
              </DropdownMenu.Portal>
            </DropdownMenu.Root>
          </Can>
        );
      },
    },
  ];

  /* ── Columns: bank accounts ──────────────────────────────────────────── */
  const accountColumns: ColumnDef<BankAccountListDto, unknown>[] = [
    {
      id: "account",
      header: "Cuenta",
      cell: ({ row }) => {
        const a = row.original;
        return (
          <div className="min-w-0">
            <p className="truncate font-medium text-foreground">{a.name}</p>
            <p className="text-[11px] text-muted">
              {a.bankName ?? "—"}
              {a.maskedAccountNumber ? ` · ${a.maskedAccountNumber}` : ""}
            </p>
          </div>
        );
      },
    },
    {
      accessorKey: "currency",
      header: "Moneda",
      size: 80,
      cell: ({ getValue }) => (
        <Badge variant="muted">{getValue() as string}</Badge>
      ),
    },
    {
      accessorKey: "openingBalance",
      header: "Saldo inicial",
      size: 130,
      cell: ({ row }) => (
        <span className="font-semibold text-foreground">
          {formatAmount(row.original.openingBalance, row.original.currency)}
        </span>
      ),
    },
    {
      accessorKey: "isActive",
      header: "Estado",
      size: 90,
      cell: ({ getValue }) => (
        <Badge variant={(getValue() as boolean) ? "jade" : "muted"}>
          {(getValue() as boolean) ? "Activa" : "Inactiva"}
        </Badge>
      ),
    },
    {
      id: "actions",
      size: 48,
      cell: ({ row }) => {
        const a = row.original;
        return (
          <Can permission="ManageBilling">
            <DropdownMenu.Root>
              <DropdownMenu.Trigger asChild>
                <Button controlKey="ui.app.app.ledger.page.button.2" variant="ghost" size="icon" aria-label="Opciones">
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenu.Trigger>
              <DropdownMenu.Portal>
                <DropdownMenu.Content
                  className="z-50 min-w-[170px] overflow-hidden rounded-table border border-border bg-surface p-1 shadow-dp2"
                  sideOffset={4}
                  align="end"
                >
                  <DropdownMenu.Item
                    className="flex cursor-pointer items-center gap-2 rounded-[6px] px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:outline-none"
                    onSelect={() => openActionDialog("income", a.id)}
                  >
                    <ArrowDownLeft className="h-3.5 w-3.5 text-success" />
                    Registrar ingreso
                  </DropdownMenu.Item>
                  <DropdownMenu.Item
                    className="flex cursor-pointer items-center gap-2 rounded-[6px] px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:outline-none"
                    onSelect={() => openActionDialog("expense", a.id)}
                  >
                    <ArrowUpRight className="h-3.5 w-3.5 text-danger" />
                    Registrar egreso
                  </DropdownMenu.Item>
                  <DropdownMenu.Item
                    className="flex cursor-pointer items-center gap-2 rounded-[6px] px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:outline-none"
                    onSelect={() => openActionDialog("transfer", a.id)}
                  >
                    <RefreshCw className="h-3.5 w-3.5" />
                    Transferencia
                  </DropdownMenu.Item>
                  <DropdownMenu.Separator className="my-1 h-px bg-border" />
                  <DropdownMenu.Item
                    className="flex cursor-pointer items-center gap-2 rounded-[6px] px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:outline-none"
                    onSelect={() => {
                      setEditingAccount(a as BankAccountDto);
                      setAccountFormOpen(true);
                    }}
                  >
                    <Edit2 className="h-3.5 w-3.5" />
                    Editar
                  </DropdownMenu.Item>
                  <DropdownMenu.Item
                    className="flex cursor-pointer items-center gap-2 rounded-[6px] px-3 py-1.5 text-[13px] text-danger hover:bg-danger-soft focus:outline-none"
                    onSelect={() => {
                      if (confirm(`¿Eliminar la cuenta "${a.name}"?`)) {
                        deleteAccount.mutate(a.id);
                      }
                    }}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                    Eliminar
                  </DropdownMenu.Item>
                </DropdownMenu.Content>
              </DropdownMenu.Portal>
            </DropdownMenu.Root>
          </Can>
        );
      },
    },
  ];

  return (
    <div>
      {/* Header */}
      <div className="mb-5 flex items-center justify-between gap-4">
        <div>
          <h1 className="font-display text-[22px] font-bold text-foreground">Ledger</h1>
          <p className="mt-0.5 text-[13px] text-muted">
            Movimientos y cuentas bancarias
          </p>
        </div>
        <Can permission="ManageBilling">
          <div className="flex items-center gap-2">
            <DropdownMenu.Root>
              <DropdownMenu.Trigger asChild>
                <Button controlKey="ui.app.app.ledger.page.button.3" size="md">
                  <Plus className="h-4 w-4" />
                  Registrar movimiento
                </Button>
              </DropdownMenu.Trigger>
              <DropdownMenu.Portal>
                <DropdownMenu.Content
                  className="z-50 min-w-[200px] overflow-hidden rounded-table border border-border bg-surface p-1 shadow-dp2"
                  sideOffset={6}
                  align="end"
                >
                  <DropdownMenu.Item
                    className="flex cursor-pointer items-center gap-2 rounded-[6px] px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:outline-none"
                    onSelect={() => openActionDialog("income")}
                  >
                    <ArrowDownLeft className="h-3.5 w-3.5 text-success" />
                    Ingreso
                  </DropdownMenu.Item>
                  <DropdownMenu.Item
                    className="flex cursor-pointer items-center gap-2 rounded-[6px] px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:outline-none"
                    onSelect={() => openActionDialog("expense")}
                  >
                    <ArrowUpRight className="h-3.5 w-3.5 text-danger" />
                    Egreso
                  </DropdownMenu.Item>
                  <DropdownMenu.Item
                    className="flex cursor-pointer items-center gap-2 rounded-[6px] px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:outline-none"
                    onSelect={() => openActionDialog("transfer")}
                  >
                    <RefreshCw className="h-3.5 w-3.5" />
                    Transferencia
                  </DropdownMenu.Item>
                  <DropdownMenu.Separator className="my-1 h-px bg-border" />
                  <DropdownMenu.Item
                    className="flex cursor-pointer items-center gap-2 rounded-[6px] px-3 py-1.5 text-[13px] text-foreground-secondary hover:bg-surface-subtle focus:outline-none"
                    onSelect={() => openActionDialog("adjustment")}
                  >
                    <Wallet className="h-3.5 w-3.5" />
                    Ajuste
                  </DropdownMenu.Item>
                </DropdownMenu.Content>
              </DropdownMenu.Portal>
            </DropdownMenu.Root>
          </div>
        </Can>
      </div>

      {showImportAttempts && (
        <Can permission="ManageReconciliation">
          <section className="mb-5 rounded-card border border-danger/30 bg-danger-soft p-4" aria-labelledby="failed-imports-title">
            <div className="flex items-start justify-between gap-3">
              <div className="flex min-w-0 gap-3">
                <FileWarning className="mt-0.5 h-5 w-5 shrink-0 text-danger" aria-hidden="true" />
                <div>
                  <h2 id="failed-imports-title" className="font-display text-base font-bold text-foreground">Importaciones bancarias fallidas</h2>
                  <p className="mt-1 text-[13px] text-foreground-secondary">Evidencia conservada para corregir el archivo y volver a importarlo.</p>
                </div>
              </div>
              <Button controlKey="dashboard.importFailures.close" variant="ghost" size="sm" onClick={() => setShowImportAttempts(false)}>
                Cerrar
              </Button>
            </div>
            {importAttempts.isLoading ? (
              <p className="mt-4 text-[13px] text-muted">Cargando intentos…</p>
            ) : importAttempts.isError ? (
              <div className="mt-4 flex items-center justify-between gap-3 rounded-input bg-surface p-3">
                <p className="text-[13px] text-danger">No se pudieron cargar los intentos.</p>
                <Button controlKey="dashboard.importFailures.retry" variant="outline" size="sm" onClick={() => importAttempts.refetch()}>Reintentar</Button>
              </div>
            ) : importAttempts.data?.length ? (
              <div className="mt-4 grid gap-2">
                {importAttempts.data.map((attempt) => (
                  <article key={attempt.id} className="rounded-input border border-border bg-surface p-3">
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <p className="font-semibold text-foreground">{attempt.fileName}</p>
                      <time className="text-[11px] text-muted">{formatDateTime(attempt.attemptedAt)}</time>
                    </div>
                    <p className="mt-1 text-[12px] text-muted">{attempt.bankAccountName}</p>
                    <p className="mt-2 text-[13px] text-danger">{attempt.error ?? "El archivo fue rechazado sin detalle adicional."}</p>
                  </article>
                ))}
              </div>
            ) : (
              <p className="mt-4 text-[13px] font-semibold text-success">No hay importaciones fallidas en este periodo.</p>
            )}
          </section>
        </Can>
      )}

      {showReconciliationLines && (
        <Can permission="ManageReconciliation">
          <section className="mb-5 rounded-card border border-warning/30 bg-warning-soft p-4" aria-labelledby="reconciliation-lines-title">
            <div className="flex items-start justify-between gap-3">
              <div className="flex min-w-0 gap-3">
                <RefreshCw className="mt-0.5 h-5 w-5 shrink-0 text-warning" aria-hidden="true" />
                <div>
                  <h2 id="reconciliation-lines-title" className="font-display text-base font-bold text-foreground">Líneas bancarias por conciliar</h2>
                  <p className="mt-1 text-[13px] text-foreground-secondary">Movimientos del estado bancario sin conciliación confirmada.</p>
                </div>
              </div>
              <Button controlKey="dashboard.reconciliationLines.close" variant="ghost" size="sm" onClick={() => setShowReconciliationLines(false)}>Cerrar</Button>
            </div>
            {reconciliationLines.isLoading ? (
              <p className="mt-4 text-[13px] text-muted">Cargando diferencias…</p>
            ) : reconciliationLines.isError ? (
              <div className="mt-4 flex items-center justify-between gap-3 rounded-input bg-surface p-3"><p className="text-[13px] text-danger">No se pudieron cargar las líneas.</p><Button controlKey="dashboard.reconciliationLines.retry" variant="outline" size="sm" onClick={() => reconciliationLines.refetch()}>Reintentar</Button></div>
            ) : reconciliationLines.data?.length ? (
              <div className="mt-4 grid gap-2">
                {reconciliationLines.data.map((line) => (
                  <article key={line.id} className="grid gap-2 rounded-input border border-border bg-surface p-3 sm:grid-cols-[1fr_auto] sm:items-center">
                    <div className="min-w-0"><p className="truncate font-semibold text-foreground">{line.description}</p><p className="mt-1 text-[12px] text-muted">{line.bankAccountName}{line.reference ? ` · Ref: ${line.reference}` : ""} · {formatDate(line.transactionDate)}</p></div>
                    <p className={cn("font-bold", line.amount < 0 ? "text-danger" : "text-success")}>{formatAmount(Math.abs(line.amount), line.currency)}</p>
                  </article>
                ))}
              </div>
            ) : <p className="mt-4 text-[13px] font-semibold text-success">No hay diferencias pendientes en este periodo.</p>}
          </section>
        </Can>
      )}

      <Tabs defaultTab="entries">
        <TabList>
          <Tab controlKey="ui.app.app.ledger.page.tab.1" id="entries">
            Movimientos
            {entriesData && entriesData.totalCount > 0 && (
              <span className="ml-1.5 rounded-full bg-border px-1.5 py-0.5 text-[10px] font-bold text-muted">
                {entriesData.totalCount}
              </span>
            )}
          </Tab>
          <Tab controlKey="ui.app.app.ledger.page.tab.2" id="accounts">
            Cuentas bancarias
            {accountsPage_data && accountsPage_data.totalCount > 0 && (
              <span className="ml-1.5 rounded-full bg-border px-1.5 py-0.5 text-[10px] font-bold text-muted">
                {accountsPage_data.totalCount}
              </span>
            )}
          </Tab>
        </TabList>

        {/* ── MOVIMIENTOS ── */}
        <TabPanel id="entries">
          <div className="mb-4 flex flex-wrap items-center gap-3">
            <Select controlKey="ui.app.app.ledger.page.select.1"
              label=""
              value={entryTypeFilter}
              onValueChange={(v) => {
                setEntryTypeFilter(v as LedgerEntryType | "");
                setEntriesPage(1);
              }}
            >
              <SelectItem value="">Todos los tipos</SelectItem>
              {ENTRY_TYPES.map((t) => (
                <SelectItem key={t.value} value={t.value}>{t.label}</SelectItem>
              ))}
            </Select>

            {allAccounts.length > 0 && (
              <Select controlKey="ui.app.app.ledger.page.select.2"
                label=""
                value={accountFilter}
                onValueChange={(v) => { setAccountFilter(v); setEntriesPage(1); }}
              >
                <SelectItem value="">Todas las cuentas</SelectItem>
                {allAccounts.map((a) => (
                  <SelectItem key={a.id} value={a.id}>{a.name}</SelectItem>
                ))}
              </Select>
            )}

            <div className="flex items-center gap-1.5">
              <input data-ui-control="ui.app.app.ledger.page.input.1"
                type="date"
                value={dateFromFilter}
                onChange={(e) => { setDateFromFilter(e.target.value); setEntriesPage(1); }}
                className="h-8 rounded-input border border-border bg-surface px-2 text-[12px] text-foreground focus:border-action focus:outline-none focus:ring-1 focus:ring-action"
                placeholder="Desde"
              />
              <span className="text-[12px] text-muted">—</span>
              <input data-ui-control="ui.app.app.ledger.page.input.2"
                type="date"
                value={dateToFilter}
                onChange={(e) => { setDateToFilter(e.target.value); setEntriesPage(1); }}
                className="h-8 rounded-input border border-border bg-surface px-2 text-[12px] text-foreground focus:border-action focus:outline-none focus:ring-1 focus:ring-action"
                placeholder="Hasta"
              />
            </div>

            <Select controlKey="ledger.filters.reconciled" permission="ViewPayments" label="" value={reconciledFilter || "all"} onValueChange={(value) => { setReconciledFilter(value === "all" ? "" : value as "true" | "false"); setEntriesPage(1); }}>
              <SelectItem value="all">Toda conciliación</SelectItem><SelectItem value="false">Pendientes</SelectItem><SelectItem value="true">Conciliados</SelectItem>
            </Select>
            {currencyFilter && <Badge variant="muted">Moneda: {currencyFilter}</Badge>}
            {hasUnappliedBalance && <Badge variant="saffron">Saldo sin aplicar</Badge>}

            {hasEntryFilters && (
              <button data-ui-control="ui.app.app.ledger.page.button.8"
                onClick={() => {
                  setEntryTypeFilter("");
                  setAccountFilter("");
                  setDateFromFilter("");
                  setDateToFilter("");
                  setClientIdFilter("");
                  setCurrencyFilter("");
                  setReconciledFilter("");
                  setHasUnappliedBalance(false);
                  setEntriesPage(1);
                }}
                className="text-[12px] text-muted hover:text-action underline"
              >
                Limpiar
              </button>
            )}
          </div>

          <DataTable
            columns={entryColumns}
            data={entriesData?.items ?? []}
            isLoading={entriesLoading}
            emptyMessage="Sin movimientos para los filtros actuales."
            getRowId={(r) => r.id}
          />

          {entriesData && entriesData.totalCount > PAGE_SIZE && (
            <Pagination
              page={entriesPage}
              totalPages={entriesData.totalPages}
              totalCount={entriesData.totalCount}
              pageSize={PAGE_SIZE}
              onPageChange={setEntriesPage}
            />
          )}
        </TabPanel>

        {/* ── CUENTAS BANCARIAS ── */}
        <TabPanel id="accounts">
          <div className="mb-4 flex items-center justify-between gap-3">
            <Select controlKey="ui.app.app.ledger.page.select.4"
              label=""
              value={accountStatusFilter}
              onValueChange={(v) => {
                setAccountStatusFilter(v as "" | "true" | "false");
                setAccountsPage(1);
              }}
            >
              <SelectItem value="">Todas</SelectItem>
              <SelectItem value="true">Activas</SelectItem>
              <SelectItem value="false">Inactivas</SelectItem>
            </Select>

            <Can permission="ManageBilling">
              <Button controlKey="ui.app.app.ledger.page.button.9"
                size="sm"
                onClick={() => {
                  setEditingAccount(null);
                  setAccountFormOpen(true);
                }}
              >
                <Plus className="h-3.5 w-3.5" />
                Nueva cuenta
              </Button>
            </Can>
          </div>

          <DataTable
            columns={accountColumns}
            data={accountsPage_data?.items ?? []}
            isLoading={accountsLoading}
            emptyMessage="Sin cuentas bancarias registradas."
            getRowId={(r) => r.id}
          />

          {accountsPage_data && accountsPage_data.totalCount > 15 && (
            <Pagination
              page={accountsPage}
              totalPages={accountsPage_data.totalPages}
              totalCount={accountsPage_data.totalCount}
              pageSize={15}
              onPageChange={setAccountsPage}
            />
          )}
        </TabPanel>
      </Tabs>

      {/* Bank Account Form */}
      <BankAccountForm
        open={accountFormOpen}
        onOpenChange={(v) => {
          setAccountFormOpen(v);
          if (!v) setEditingAccount(null);
        }}
        account={editingAccount}
      />

      {/* Entry registration dialogs */}
      <IncomeForm
        open={openAction === "income"}
        onOpenChange={(v) => { if (!v) setOpenAction(null); }}
        accounts={allAccounts}
        defaultAccountId={defaultAccountId}
      />
      <ExpenseForm
        open={openAction === "expense"}
        onOpenChange={(v) => { if (!v) setOpenAction(null); }}
        accounts={allAccounts}
        defaultAccountId={defaultAccountId}
      />
      <TransferForm
        open={openAction === "transfer"}
        onOpenChange={(v) => { if (!v) setOpenAction(null); }}
        accounts={allAccounts}
        defaultFromAccountId={defaultAccountId}
      />
      <AdjustmentForm
        open={openAction === "adjustment"}
        onOpenChange={(v) => { if (!v) setOpenAction(null); }}
        accounts={allAccounts}
        defaultAccountId={defaultAccountId}
      />
    </div>
  );
}
