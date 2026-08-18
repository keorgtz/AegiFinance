"use client";

import { useEffect, useMemo, useState } from "react";
import { Dialog, DialogContent } from "@/components/ui/dialog";
import { Tabs, TabList, Tab, TabPanel } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectItem } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { useRoles } from "@/hooks/use-roles";
import { usePermissionsData } from "@/hooks/use-permissions-data";
import { useClients } from "@/hooks/use-clients";
import { useSubscriptions } from "@/hooks/use-subscriptions";
import {
  useAssignRoles, useRemoveUserPermission, useRevokeAllUserSessions, useRevokeUserSession,
  useSetUserPermission, useUserPermissionOverrides, useUserSessions,
} from "@/hooks/use-users";
import { cn } from "@/lib/utils/cn";
import { formatDateTime } from "@/lib/utils/format";
import type { UserDto } from "@/types/api";
import { Check, Laptop, ShieldAlert, Trash2 } from "lucide-react";

export function UserAccessDialog({ open, onOpenChange, user }: { open: boolean; onOpenChange: (open: boolean) => void; user: UserDto | null }) {
  const userId = user?.id ?? "";
  const { data: roles = [] } = useRoles();
  const { data: permissions = [] } = usePermissionsData();
  const { data: clients } = useClients({ pageSize: 200 });
  const [selectedRoles, setSelectedRoles] = useState<Set<string>>(new Set());
  const [permissionId, setPermissionId] = useState("");
  const [effect, setEffect] = useState("grant");
  const [clientId, setClientId] = useState("all");
  const [subscriptionId, setSubscriptionId] = useState("all");
  const [expiresAt, setExpiresAt] = useState("");
  const { data: subscriptions } = useSubscriptions({ clientId: clientId === "all" ? undefined : clientId, pageSize: 200 });
  const overrides = useUserPermissionOverrides(userId);
  const sessions = useUserSessions(userId);
  const assignRoles = useAssignRoles();
  const setPermission = useSetUserPermission();
  const removePermission = useRemoveUserPermission();
  const revokeSession = useRevokeUserSession();
  const revokeAll = useRevokeAllUserSessions();

  const compatibleRoles = useMemo(() => roles.filter((role) => !role.userType || role.userType === user?.userType), [roles, user?.userType]);
  useEffect(() => {
    if (!open || !user) return;
    setSelectedRoles(new Set(compatibleRoles.filter((role) => user.roles.includes(role.name)).map((role) => role.id)));
    setPermissionId(""); setEffect("grant"); setClientId("all"); setSubscriptionId("all"); setExpiresAt("");
  }, [open, user, compatibleRoles]);

  const toggleRole = (id: string) => setSelectedRoles((current) => {
    const next = new Set(current); next.has(id) ? next.delete(id) : next.add(id); return next;
  });
  const saveRoles = async () => {
    if (!user) return;
    await assignRoles.mutateAsync({ id: user.id, data: { roleIds: [...selectedRoles] } });
  };
  const saveOverride = async () => {
    if (!user || !permissionId) return;
    await setPermission.mutateAsync({ id: user.id, data: {
      permissionId, isGranted: effect === "grant", clientId: clientId === "all" ? null : clientId,
      subscriptionId: subscriptionId === "all" ? null : subscriptionId,
      expiresAt: expiresAt ? new Date(expiresAt).toISOString() : null,
    }});
    setPermissionId(""); setExpiresAt("");
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Administrar acceso" description={user ? `${user.name} · @${user.userName}` : undefined} size="lg">
        <Tabs defaultTab="roles">
          <TabList>
            <Tab id="roles" controlKey="users.access.tabs.roles" permission="ManageUsers">Roles</Tab>
            <Tab id="exceptions" controlKey="users.access.tabs.exceptions" permission="ManageUsers">Excepciones</Tab>
            <Tab id="sessions" controlKey="users.access.tabs.sessions" permission="ManageUsers">Sesiones</Tab>
          </TabList>

          <TabPanel id="roles">
            <p className="mb-3 text-sm text-muted">Asignación masiva compatible con el tipo de usuario.</p>
            <div className="grid gap-2 sm:grid-cols-2">
              {compatibleRoles.map((role) => (
                <button data-ui-control="users.access.roles.toggle" data-ui-permission="ManageUsers" key={role.id} type="button" onClick={() => toggleRole(role.id)} className={cn("flex min-h-14 items-center justify-between rounded-input border p-3 text-left transition-ui", selectedRoles.has(role.id) ? "border-action bg-action-soft text-action" : "border-border bg-surface hover:border-border-strong")}>
                  <span><span className="block text-sm font-bold">{role.name}</span><span className="mt-0.5 block text-xs text-muted">{role.description || "Sin descripción"}</span></span>
                  {selectedRoles.has(role.id) && <Check className="h-4 w-4" aria-label="Seleccionado" />}
                </button>
              ))}
            </div>
            <div className="mt-4 flex justify-end"><Button controlKey="users.access.roles.save" permission="ManageUsers" loading={assignRoles.isPending} onClick={saveRoles}>Guardar roles</Button></div>
          </TabPanel>

          <TabPanel id="exceptions">
            <div className="rounded-card border border-border bg-surface-subtle p-4">
              <div className="grid gap-4 sm:grid-cols-2">
                <Select controlKey="users.access.exceptions.permission" permission="ManageUsers" label="Permiso" value={permissionId} onValueChange={setPermissionId} placeholder="Seleccioná un permiso">{permissions.map((item) => <SelectItem key={item.id} value={item.id}>{item.module} · {item.name}</SelectItem>)}</Select>
                <Select controlKey="users.access.exceptions.effect" permission="ManageUsers" label="Efecto" value={effect} onValueChange={setEffect}><SelectItem value="grant">Conceder</SelectItem><SelectItem value="deny">Denegar</SelectItem></Select>
                <Select controlKey="users.access.exceptions.client" permission="ManageUsers" label="Alcance de cliente" value={clientId} onValueChange={(value) => { setClientId(value); setSubscriptionId("all"); }}><SelectItem value="all">Global</SelectItem>{clients?.items.map((item) => <SelectItem key={item.id} value={item.id}>{item.name}</SelectItem>)}</Select>
                <Select controlKey="users.access.exceptions.subscription" permission="ManageUsers" label="Alcance de suscripción" value={subscriptionId} onValueChange={setSubscriptionId} disabled={clientId === "all"}><SelectItem value="all">Todas</SelectItem>{subscriptions?.items.map((item) => <SelectItem key={item.id} value={item.id}>{item.code}</SelectItem>)}</Select>
                <Input controlKey="users.access.exceptions.expires" permission="ManageUsers" type="datetime-local" label="Expira (opcional)" value={expiresAt} min={new Date().toISOString().slice(0, 16)} onChange={(event) => setExpiresAt(event.target.value)} />
              </div>
              <div className="mt-4 flex justify-end"><Button controlKey="users.access.exceptions.save" permission="ManageUsers" disabled={!permissionId} loading={setPermission.isPending} onClick={saveOverride}>Guardar excepción</Button></div>
            </div>
            <div className="mt-4 space-y-2">
              {overrides.isLoading ? <p className="text-sm text-muted">Cargando excepciones…</p> : overrides.isError ? <p className="text-sm text-danger">No se pudieron cargar las excepciones.</p> : overrides.data?.length ? overrides.data.map((item) => (
                <div key={item.id} className="flex min-h-14 items-center gap-3 rounded-input border border-border p-3">
                  <ShieldAlert className={cn("h-4 w-4 shrink-0", item.isGranted ? "text-success" : "text-danger")} />
                  <div className="min-w-0 flex-1"><p className="truncate text-sm font-bold text-foreground">{item.permissionName}</p><p className="text-xs text-muted">{item.permissionCode}{item.expiresAt ? ` · vence ${formatDateTime(item.expiresAt)}` : " · sin vencimiento"}</p></div>
                  <Badge variant={item.isGranted ? "jade" : "terracotta"}>{item.isGranted ? "Concede" : "Deniega"}</Badge>
                  <Button controlKey="users.access.exceptions.remove" permission="ManageUsers" variant="ghost" size="icon" aria-label="Eliminar excepción" onClick={() => user && removePermission.mutate({ id: user.id, permissionId: item.permissionId, clientId: item.clientId, subscriptionId: item.subscriptionId })}><Trash2 className="h-4 w-4 text-danger" /></Button>
                </div>
              )) : <p className="rounded-input bg-surface-subtle p-4 text-sm text-muted">Este usuario no tiene excepciones.</p>}
            </div>
          </TabPanel>

          <TabPanel id="sessions">
            <div className="mb-4 flex items-start justify-between gap-3"><div><p className="text-sm font-bold text-foreground">Sesiones activas</p><p className="mt-1 text-xs text-muted">Cerrar una sesión invalida inmediatamente su token de acceso.</p></div><Button controlKey="users.access.sessions.revoke-all" permission="ManageUsers" variant="danger" size="sm" disabled={!sessions.data?.some((item) => item.isActive)} loading={revokeAll.isPending} onClick={() => user && revokeAll.mutate(user.id)}>Cerrar todas</Button></div>
            <div className="space-y-2">{sessions.isLoading ? <p className="text-sm text-muted">Cargando sesiones…</p> : sessions.isError ? <p className="text-sm text-danger">No se pudieron cargar las sesiones.</p> : sessions.data?.length ? sessions.data.map((session) => (
              <div key={session.id} className="flex min-h-16 items-center gap-3 rounded-input border border-border p-3"><span className="grid h-10 w-10 shrink-0 place-items-center rounded-input bg-action-soft text-action"><Laptop className="h-4 w-4" /></span><div className="min-w-0 flex-1"><p className="truncate text-sm font-bold text-foreground">{session.deviceName}</p><p className="text-xs text-muted">{session.ipAddress || "IP no disponible"} · {formatDateTime(session.lastSeenAt)}</p></div><Badge variant={session.isActive ? "jade" : "muted"}>{session.isActive ? "Activa" : "Cerrada"}</Badge>{session.isActive && <Button controlKey="users.access.sessions.revoke" permission="ManageUsers" variant="outline" size="sm" loading={revokeSession.isPending} onClick={() => user && revokeSession.mutate({ id: user.id, sessionId: session.id })}>Cerrar</Button>}</div>
            )) : <p className="rounded-input bg-surface-subtle p-4 text-sm text-muted">No hay sesiones registradas.</p>}</div>
          </TabPanel>
        </Tabs>
      </DialogContent>
    </Dialog>
  );
}
