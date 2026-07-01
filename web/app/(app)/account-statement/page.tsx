"use client";

import { useState, useMemo } from "react";
import { type ColumnDef } from "@tanstack/react-table";
import { useClientStatement } from "@/hooks/use-account-statements";
import { useClients } from "@/hooks/use-clients";
import { DataTable } from "@/components/ui/data-table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import { Select, SelectItem } from "@/components/ui/select";
import { Spinner } from "@/components/ui/spinner";
import { formatAmount, formatDate, formatDateTime } from "@/lib/utils/format";
import type { AccountStatementItemDto } from "@/types/api";
import {
  ArrowDownLeft,
  ArrowUpRight,
  FileText,
  MinusCircle,
  Printer,
  SlidersHorizontal,
} from "lucide-react";
import { cn } from "@/lib/utils/cn";

const ITEM_TYPE_CONFIG: Record<string, { label: string; variant: "jade" | "terracotta" | "periwinkle" | "saffron" | "muted" }> = {
  Charge:     { label: "Cargo",   variant: "terracotta" },
  Payment:    { label: "Pago",    variant: "jade" },
  Adjustment: { label: "Ajuste",  variant: "periwinkle" },
};

function ItemTypeBadge({ type }: { type: string }) {
  const cfg = ITEM_TYPE_CONFIG[type] ?? { label: type, variant: "muted" as const };
  return <Badge variant={cfg.variant}>{cfg.label}</Badge>;
}

interface KpiCardProps {
  label: string;
  amount: number;
  currency: string;
  color?: "default" | "green" | "red" | "blue";
}

function KpiCard({ label, amount, currency, color = "default" }: KpiCardProps) {
  return (
    <div className="flex flex-col gap-1 rounded-card border border-[#E3E6EC] bg-white p-4">
      <span className="text-[11px] font-600 uppercase tracking-wider text-[#5B6472]">{label}</span>
      <span
        className={cn(
          "text-[20px] font-700 leading-tight",
          color === "green" && "text-[#0E9F6E]",
          color === "red" && "text-[#B6452C]",
          color === "blue" && "text-[#1B4298]",
          color === "default" && "text-[#16181D]"
        )}
      >
        {formatAmount(amount, currency)}
      </span>
    </div>
  );
}

const CURRENCIES = ["MXN", "USD", "EUR"];

export default function AccountStatementPage() {
  const [selectedClientId, setSelectedClientId] = useState("");
  const [dateFrom, setDateFrom] = useState("");
  const [dateTo, setDateTo] = useState("");
  const [currency, setCurrency] = useState("MXN");

  const { data: clientsPage } = useClients({ pageSize: 200 });
  const clientOptions =
    clientsPage?.items.map((c) => ({
      value: c.id,
      label: c.name,
      description: c.code,
    })) ?? [];

  const params = useMemo(
    () => ({
      from: dateFrom || undefined,
      to: dateTo || undefined,
      currency,
    }),
    [dateFrom, dateTo, currency]
  );

  const { data: statement, isLoading, isFetching } = useClientStatement(selectedClientId, params);

  const hasClient = !!selectedClientId;
  const hasData = !!statement;

  /* ── Table columns ── */
  const columns: ColumnDef<AccountStatementItemDto, unknown>[] = [
    {
      accessorKey: "date",
      header: "Fecha",
      size: 105,
      cell: ({ getValue }) => (
        <span className="text-[12px] text-[#5B6472]">{formatDate(getValue() as string)}</span>
      ),
    },
    {
      accessorKey: "type",
      header: "Tipo",
      size: 110,
      cell: ({ getValue }) => <ItemTypeBadge type={getValue() as string} />,
    },
    {
      accessorKey: "description",
      header: "Descripción",
      cell: ({ getValue }) => (
        <span className="text-[13px] text-[#16181D]">{getValue() as string}</span>
      ),
    },
    {
      accessorKey: "debit",
      header: "Cargo",
      size: 130,
      cell: ({ getValue, row }) => {
        const v = getValue() as number;
        if (v === 0) return <span className="text-[#C9D0DE]">—</span>;
        return (
          <span className="flex items-center gap-1 font-600 text-[13px] text-[#B6452C]">
            <ArrowUpRight className="h-3.5 w-3.5" />
            {formatAmount(v, row.original.exchangeRateUsed ? currency : "MXN")}
          </span>
        );
      },
    },
    {
      accessorKey: "credit",
      header: "Abono",
      size: 130,
      cell: ({ getValue, row }) => {
        const v = getValue() as number;
        if (v === 0) return <span className="text-[#C9D0DE]">—</span>;
        return (
          <span className="flex items-center gap-1 font-600 text-[13px] text-[#0E9F6E]">
            <ArrowDownLeft className="h-3.5 w-3.5" />
            {formatAmount(v, row.original.exchangeRateUsed ? currency : "MXN")}
          </span>
        );
      },
    },
    {
      accessorKey: "balance",
      header: "Saldo",
      size: 130,
      cell: ({ getValue }) => {
        const v = getValue() as number;
        return (
          <span
            className={cn(
              "font-700 text-[13px]",
              v > 0 ? "text-[#B6452C]" : v < 0 ? "text-[#0E9F6E]" : "text-[#5B6472]"
            )}
          >
            {formatAmount(v, currency)}
          </span>
        );
      },
    },
  ];

  return (
    <div>
      {/* Header */}
      <div className="mb-5 flex items-center justify-between gap-4">
        <div>
          <h1 className="font-display text-[22px] font-700 text-[#16181D]">Estado de Cuenta</h1>
          <p className="mt-0.5 text-[13px] text-[#5B6472]">
            Historial de cargos y pagos por cliente
          </p>
        </div>
        {hasData && (
          <Button variant="secondary" size="md" onClick={() => window.print()}>
            <Printer className="h-4 w-4" />
            Imprimir
          </Button>
        )}
      </div>

      {/* Filters */}
      <div className="mb-5 flex flex-wrap items-end gap-3">
        <div className="w-72">
          <Combobox
            label="Cliente"
            value={selectedClientId}
            onValueChange={setSelectedClientId}
            options={clientOptions}
            placeholder="Seleccionar cliente…"
            searchPlaceholder="Buscar cliente…"
          />
        </div>

        <div className="flex flex-col gap-1">
          <span className="text-[11px] font-600 uppercase tracking-wider text-[#5B6472]">
            Desde
          </span>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            className="h-8 rounded-input border border-[#E3E6EC] bg-white px-2 text-[12px] text-[#16181D] focus:border-[#5BAEBC] focus:outline-none focus:ring-1 focus:ring-[#5BAEBC]"
          />
        </div>

        <div className="flex flex-col gap-1">
          <span className="text-[11px] font-600 uppercase tracking-wider text-[#5B6472]">
            Hasta
          </span>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            className="h-8 rounded-input border border-[#E3E6EC] bg-white px-2 text-[12px] text-[#16181D] focus:border-[#5BAEBC] focus:outline-none focus:ring-1 focus:ring-[#5BAEBC]"
          />
        </div>

        <div className="w-28">
          <Select
            label="Moneda"
            value={currency}
            onValueChange={setCurrency}
          >
            {CURRENCIES.map((c) => (
              <SelectItem key={c} value={c}>{c}</SelectItem>
            ))}
          </Select>
        </div>

        {(dateFrom || dateTo || currency !== "MXN") && (
          <button
            onClick={() => { setDateFrom(""); setDateTo(""); setCurrency("MXN"); }}
            className="self-end pb-1 text-[12px] text-[#5B6472] underline hover:text-[#0F5C6B]"
          >
            Limpiar
          </button>
        )}
      </div>

      {/* Empty state */}
      {!hasClient && (
        <div className="flex flex-col items-center justify-center rounded-card border border-dashed border-[#E3E6EC] py-20 text-center">
          <FileText className="mb-3 h-10 w-10 text-[#C9D0DE]" />
          <p className="text-[14px] font-500 text-[#5B6472]">Selecciona un cliente</p>
          <p className="mt-1 text-[13px] text-[#8A93A2]">
            El estado de cuenta muestra todos los cargos y abonos en el período indicado.
          </p>
        </div>
      )}

      {/* Loading */}
      {hasClient && isLoading && (
        <div className="flex justify-center py-16">
          <Spinner size="lg" />
        </div>
      )}

      {/* Content */}
      {hasData && !isLoading && (
        <div className={cn("space-y-5", isFetching && "opacity-60 pointer-events-none")}>
          {/* Client + period header */}
          <div className="flex items-start justify-between gap-4 rounded-card border border-[#E3E6EC] bg-[#F7F8FA] p-4">
            <div>
              <p className="text-[15px] font-700 text-[#16181D]">{statement.clientName}</p>
              <p className="mt-0.5 text-[12px] text-[#5B6472]">
                {statement.startDate
                  ? `${formatDate(statement.startDate)} — ${formatDate(statement.endDate ?? new Date().toISOString())}`
                  : "Todo el historial"}
              </p>
              {statement.exchangeRateUsed && (
                <p className="mt-0.5 text-[11px] text-[#8A93A2]">
                  Tipo de cambio aplicado: {statement.exchangeRateUsed.toFixed(4)} MXN/{statement.displayCurrency}
                </p>
              )}
            </div>
            <p className="text-[11px] text-[#8A93A2]">
              Generado: {formatDateTime(statement.statementDate)}
            </p>
          </div>

          {/* KPI cards */}
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
            <KpiCard
              label="Saldo inicial"
              amount={statement.initialBalance}
              currency={statement.displayCurrency}
              color={statement.initialBalance > 0 ? "red" : "green"}
            />
            <KpiCard
              label="Total cargos"
              amount={statement.totalCharges}
              currency={statement.displayCurrency}
              color="red"
            />
            <KpiCard
              label="Total pagos"
              amount={statement.totalPayments}
              currency={statement.displayCurrency}
              color="green"
            />
            <KpiCard
              label="Saldo final"
              amount={statement.finalBalance}
              currency={statement.displayCurrency}
              color={statement.finalBalance > 0 ? "red" : statement.finalBalance < 0 ? "green" : "default"}
            />
          </div>

          {/* Adjustments note */}
          {statement.totalAdjustments !== 0 && (
            <div className="flex items-center gap-2 rounded-input border border-[#D1D5DB] bg-[#F7F8FA] px-3 py-2">
              <MinusCircle className="h-3.5 w-3.5 text-[#5B6472]" />
              <span className="text-[12px] text-[#5B6472]">
                Incluye ajustes por{" "}
                <span className="font-600">{formatAmount(statement.totalAdjustments, statement.displayCurrency)}</span>
              </span>
            </div>
          )}

          {/* Movements table */}
          <DataTable
            columns={columns}
            data={statement.items}
            isLoading={false}
            emptyMessage="Sin movimientos en el período seleccionado."
            getRowId={(r) => `${r.referenceId}-${r.type}`}
          />

          {/* Footer summary */}
          {statement.items.length > 0 && (
            <div className="flex justify-end">
              <div className="rounded-input border border-[#E3E6EC] bg-white px-5 py-3 text-right">
                <p className="text-[11px] font-600 uppercase tracking-wider text-[#5B6472]">
                  Saldo al corte
                </p>
                <p
                  className={cn(
                    "mt-0.5 text-[22px] font-700",
                    statement.finalBalance > 0
                      ? "text-[#B6452C]"
                      : statement.finalBalance < 0
                      ? "text-[#0E9F6E]"
                      : "text-[#16181D]"
                  )}
                >
                  {formatAmount(statement.finalBalance, statement.displayCurrency)}
                </p>
                {statement.finalBalance > 0 && (
                  <p className="text-[11px] text-[#B6452C]">Saldo a favor de la empresa</p>
                )}
                {statement.finalBalance < 0 && (
                  <p className="text-[11px] text-[#0E9F6E]">Saldo a favor del cliente</p>
                )}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
