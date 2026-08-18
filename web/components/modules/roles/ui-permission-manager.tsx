"use client";

import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useRoles } from "@/hooks/use-roles";
import { useUsers } from "@/hooks/use-users";
import { uiPermissionsApi } from "@/lib/api/ui-permissions";
import { Button } from "@/components/ui/button";
import { Select, SelectItem } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import type { UiAccessMode } from "@/types/api";

const modes: UiAccessMode[] = ["Enabled", "ReadOnly", "Disabled", "Hidden"];

export function UiPermissionManager() {
  const queryClient = useQueryClient();
  const { data: roles = [] } = useRoles();
  const { data: users } = useUsers({ pageSize: 100 });
  const { data: catalog = [], isLoading } = useQuery({ queryKey: ["ui-permissions", "catalog"], queryFn: uiPermissionsApi.catalog });
  const [roleId, setRoleId] = useState("");
  const [userId, setUserId] = useState("none");
  const [controlId, setControlId] = useState("");
  const [mode, setMode] = useState<UiAccessMode>("Enabled");
  const [simulation, setSimulation] = useState<Record<string, UiAccessMode> | null>(null);

  const save = useMutation({
    mutationFn: () => uiPermissionsApi.setPolicy({
      uiControlDefinitionId: controlId,
      roleId: userId === "none" ? roleId : null,
      userId: userId === "none" ? null : userId,
      accessMode: mode,
    }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ["ui-permissions"] });
      toast.success("Política de interfaz guardada.");
    },
    onError: (error: Error) => toast.error(error.message),
  });
  const simulate = useMutation({
    mutationFn: () => uiPermissionsApi.simulate({ roleId, userId: userId === "none" ? null : userId }),
    onSuccess: setSimulation,
    onError: (error: Error) => toast.error(error.message),
  });
  const counts = useMemo(() => modes.map((item) => [item, Object.values(simulation ?? {}).filter((value) => value === item).length] as const), [simulation]);

  return (
    <section className="rounded-panel border border-border bg-surface p-5 shadow-dp1">
      <div className="mb-5">
        <h2 className="font-display text-[16px] font-bold text-foreground">Permisos dinámicos de interfaz</h2>
        <p className="mt-1 text-xs text-muted">Personalizá un control por rol o usuario y verificá el resultado antes de aplicarlo.</p>
      </div>
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <Select controlKey="roles.ui-policy.role" permission="ManageRoles" label="Rol base" value={roleId} onValueChange={setRoleId} placeholder="Seleccioná un rol">
          {roles.map((role) => <SelectItem key={role.id} value={role.id}>{role.name}</SelectItem>)}
        </Select>
        <Select controlKey="roles.ui-policy.user" permission="ManageRoles" label="Override de usuario" value={userId} onValueChange={setUserId}>
          <SelectItem value="none">Sin override</SelectItem>
          {users?.items.map((user) => <SelectItem key={user.id} value={user.id}>{user.name}</SelectItem>)}
        </Select>
        <Select controlKey="roles.ui-policy.control" permission="ManageRoles" label="Control" value={controlId} onValueChange={setControlId} placeholder={isLoading ? "Cargando…" : "Seleccioná un control"}>
          {catalog.map((control) => <SelectItem key={control.id} value={control.id}>{control.module} · {control.label}</SelectItem>)}
        </Select>
        <Select controlKey="roles.ui-policy.mode" permission="ManageRoles" label="Comportamiento" value={mode} onValueChange={(value) => setMode(value as UiAccessMode)}>
          {modes.map((item) => <SelectItem key={item} value={item}>{item}</SelectItem>)}
        </Select>
      </div>
      <div className="mt-5 flex flex-wrap gap-3">
        <Button controlKey="roles.ui-policy.save" permission="ManageRoles" disabled={!controlId || (!roleId && userId === "none")} loading={save.isPending} onClick={() => save.mutate()}>
          Guardar política
        </Button>
        <Button controlKey="roles.ui-policy.simulate" permission="ManageRoles" variant="outline" disabled={!roleId} loading={simulate.isPending} onClick={() => simulate.mutate()}>
          Ver como rol o usuario
        </Button>
      </div>
      {simulation && (
        <div className="mt-5 border-t border-border pt-4">
          <div className="flex flex-wrap gap-2">
            {counts.map(([item, count]) => <Badge key={item} variant="muted">{item}: {count}</Badge>)}
          </div>
          <div className="mt-3 max-h-56 overflow-auto rounded-table bg-surface-subtle p-3 text-xs">
            {Object.entries(simulation).map(([key, value]) => <div key={key} className="flex justify-between gap-4 border-b border-border py-1.5 last:border-0"><code>{key}</code><span>{value}</span></div>)}
          </div>
        </div>
      )}
    </section>
  );
}
