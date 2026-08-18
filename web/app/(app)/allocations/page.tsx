"use client";

import { useState } from "react";
import { type ColumnDef } from "@tanstack/react-table";
import { useClientAllocations, useUnallocate } from "@/hooks/use-allocations";
import { useClients } from "@/hooks/use-clients";
import { DataTable } from "@/components/ui/data-table";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Combobox } from "@/components/ui/combobox";
import { AutoAllocateDialog } from "@/components/modules/allocations/auto-allocate-dialog";
import { ManualAllocateForm } from "@/components/modules/allocations/manual-allocate-form";
import { Can } from "@/lib/auth/can";
import { formatAmount, formatDateTime } from "@/lib/utils/format";
import type { SubscriptionAllocationDto } from "@/types/api";
import { Bot, Link2Off, SplitSquareVertical, Zap } from "lucide-react";
import { cn } from "@/lib/utils/cn";

export default function AllocationsPage() {
  const [selectedClientId, setSelectedClientId] = useState("");
  const [autoAllocateOpen, setAutoAllocateOpen] = useState(false);
  const [manualAllocateOpen, setManualAllocateOpen] = useState(false);

  const { data: clientsPage } = useClients({ pageSize: 200 });
  const clientOptions =
    clientsPage?.items.map((c) => ({
      value: c.id,
      label: c.name,
      description: c.code,
    })) ?? [];

  const { data: allocations = [], isLoading } = useClientAllocations(selectedClientId);
  const unallocate = useUnallocate();

  /* ── Columns ── */
  const columns: ColumnDef<SubscriptionAllocationDto, unknown>[] = [
    {
      id: "billingItem",
      header: "Cargo",
      cell: ({ row }) => {
        const a = row.original;
        return (
          <div className="min-w-0">
            <p className="truncate font-medium text-foreground">
              {a.billingItemDescription ?? "—"}
            </p>
          </div>
        );
      },
    },
    {
      id: "ledgerEntry",
      header: "Ingreso aplicado",
      cell: ({ row }) => {
        const a = row.original;
        return (
          <p className="truncate text-[13px] text-muted">
            {a.ledgerEntryDescription ?? "—"}
          </p>
        );
      },
    },
    {
      accessorKey: "amount",
      header: "Importe aplicado",
      size: 140,
      cell: ({ getValue }) => (
        <span className="font-semibold text-success">
          {formatAmount(getValue() as number)}
        </span>
      ),
    },
    {
      accessorKey: "isAutomatic",
      header: "Tipo",
      size: 100,
      cell: ({ getValue }) =>
        (getValue() as boolean) ? (
          <span className="flex items-center gap-1 text-[12px] font-semibold text-info">
            <Bot className="h-3.5 w-3.5" />
            Auto
          </span>
        ) : (
          <span className="flex items-center gap-1 text-[12px] font-semibold text-muted">
            <SplitSquareVertical className="h-3.5 w-3.5" />
            Manual
          </span>
        ),
    },
    {
      accessorKey: "allocatedAt",
      header: "Fecha",
      size: 140,
      cell: ({ getValue }) => (
        <span className="text-[12px] text-muted">
          {formatDateTime(getValue() as string)}
        </span>
      ),
    },
    {
      id: "actions",
      size: 48,
      cell: ({ row }) => (
        <Can permission="ManageBilling">
          <Button controlKey="ui.app.app.allocations.page.button.1"
            variant="ghost"
            size="icon"
            aria-label="Revertir asignación"
            onClick={(e) => {
              e.stopPropagation();
              if (confirm("¿Revertir esta asignación? El cargo quedará pendiente de nuevo.")) {
                unallocate.mutate(row.original.id);
              }
            }}
          >
            <Link2Off className="h-4 w-4 text-danger" />
          </Button>
        </Can>
      ),
    },
  ];

  const hasClient = !!selectedClientId;

  return (
    <div>
      {/* Header */}
      <div className="mb-5 flex items-center justify-between gap-4">
        <div>
          <h1 className="font-display text-[22px] font-bold text-foreground">Asignaciones</h1>
          <p className="mt-0.5 text-[13px] text-muted">
            Vincula ingresos con cargos de facturación
          </p>
        </div>
        <Can permission="ManageBilling">
          <div className={cn("flex items-center gap-2 transition-opacity", !hasClient && "pointer-events-none opacity-40")}>
            <Button controlKey="ui.app.app.allocations.page.button.2"
              variant="secondary"
              size="md"
              onClick={() => setAutoAllocateOpen(true)}
              disabled={!hasClient}
            >
              <Zap className="h-4 w-4" />
              Auto-asignar
            </Button>
            <Button controlKey="ui.app.app.allocations.page.button.3"
              size="md"
              onClick={() => setManualAllocateOpen(true)}
              disabled={!hasClient}
            >
              <SplitSquareVertical className="h-4 w-4" />
              Asignación manual
            </Button>
          </div>
        </Can>
      </div>

      {/* Client selector */}
      <div className="mb-5 max-w-sm">
        <Combobox
          label="Cliente"
          value={selectedClientId}
          onValueChange={setSelectedClientId}
          options={clientOptions}
          placeholder="Seleccionar cliente para ver asignaciones…"
          searchPlaceholder="Buscar cliente…"
        />
      </div>

      {!hasClient ? (
        <div className="flex flex-col items-center justify-center rounded-card border border-dashed border-border py-20 text-center">
          <SplitSquareVertical className="mb-3 h-10 w-10 text-border-strong" />
          <p className="text-[14px] font-medium text-muted">Selecciona un cliente</p>
          <p className="mt-1 text-[13px] text-muted">
            Las asignaciones vinculan ingresos del ledger con cargos de facturación pendientes.
          </p>
        </div>
      ) : (
        <>
          {allocations.length > 0 && (
            <div className="mb-3 flex items-center gap-2">
              <Badge variant="muted">{allocations.length} asignaciones</Badge>
              <span className="text-[12px] text-muted">
                Total aplicado:{" "}
                <span className="font-semibold text-success">
                  {formatAmount(allocations.reduce((s, a) => s + a.amount, 0))}
                </span>
              </span>
            </div>
          )}
          <DataTable
            columns={columns}
            data={allocations}
            isLoading={isLoading}
            emptyMessage="Sin asignaciones para este cliente. Usa Auto-asignar o Asignación manual."
            getRowId={(r) => r.id}
          />
        </>
      )}

      {hasClient && (
        <>
          <AutoAllocateDialog
            open={autoAllocateOpen}
            onOpenChange={setAutoAllocateOpen}
            clientId={selectedClientId}
          />
          <ManualAllocateForm
            open={manualAllocateOpen}
            onOpenChange={setManualAllocateOpen}
            clientId={selectedClientId}
          />
        </>
      )}
    </div>
  );
}
