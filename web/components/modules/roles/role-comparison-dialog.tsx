"use client";

import { useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Select, SelectItem } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { rolesApi } from "@/lib/api/roles";
import type { RoleDto } from "@/types/api";

export function RoleComparisonDialog({ open, onOpenChange, roles }: { open: boolean; onOpenChange: (open: boolean) => void; roles: RoleDto[] }) {
  const [leftId, rightId] = useComparisonSelection(roles);
  const left = useQuery({ queryKey: ["roles", "compare", leftId.value], queryFn: () => rolesApi.getById(leftId.value), enabled: open && !!leftId.value });
  const right = useQuery({ queryKey: ["roles", "compare", rightId.value], queryFn: () => rolesApi.getById(rightId.value), enabled: open && !!rightId.value });
  const leftCodes = new Set(left.data?.permissions.map((item) => item.code) ?? []);
  const rightCodes = new Set(right.data?.permissions.map((item) => item.code) ?? []);
  const allCodes = Array.from(new Set([...leftCodes, ...rightCodes])).sort();

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Comparar roles" description="Diferencias explicables antes de cambiar accesos." size="lg">
        <div className="grid gap-4 sm:grid-cols-2">
          <Select controlKey="roles.compare.left" permission="ManageRoles" label="Rol A" value={leftId.value} onValueChange={leftId.set} placeholder="Seleccioná un rol">{roles.map((role) => <SelectItem key={role.id} value={role.id}>{role.name}</SelectItem>)}</Select>
          <Select controlKey="roles.compare.right" permission="ManageRoles" label="Rol B" value={rightId.value} onValueChange={rightId.set} placeholder="Seleccioná un rol">{roles.map((role) => <SelectItem key={role.id} value={role.id}>{role.name}</SelectItem>)}</Select>
        </div>
        {!leftId.value || !rightId.value ? <p className="mt-5 rounded-input bg-surface-subtle p-4 text-sm text-muted">Seleccioná dos roles para comparar sus permisos.</p> : left.isLoading || right.isLoading ? <p className="mt-5 text-sm text-muted">Comparando permisos…</p> : left.isError || right.isError ? <p className="mt-5 text-sm text-danger">No se pudo completar la comparación.</p> : (
          <div className="mt-5 max-h-[52vh] overflow-auto rounded-table border border-border">
            <div className="grid grid-cols-[1fr_auto_auto] gap-3 border-b border-border bg-surface-subtle px-4 py-3 text-xs font-bold text-muted"><span>Permiso</span><span>{left.data?.name}</span><span>{right.data?.name}</span></div>
            {allCodes.map((code) => <div key={code} className="grid min-h-12 grid-cols-[1fr_auto_auto] items-center gap-3 border-b border-border px-4 py-2 last:border-0"><code className="truncate text-xs text-foreground">{code}</code><Badge variant={leftCodes.has(code) ? "jade" : "muted"}>{leftCodes.has(code) ? "Sí" : "No"}</Badge><Badge variant={rightCodes.has(code) ? "jade" : "muted"}>{rightCodes.has(code) ? "Sí" : "No"}</Badge></div>)}
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}

function useComparisonSelection(roles: RoleDto[]) {
  const [left, setLeft] = useState("");
  const [right, setRight] = useState("");
  useEffect(() => { if (!left && roles[0]) setLeft(roles[0].id); if (!right && roles[1]) setRight(roles[1].id); }, [roles, left, right]);
  return [{ value: left, set: setLeft }, { value: right, set: setRight }] as const;
}
