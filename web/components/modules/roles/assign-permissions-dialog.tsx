"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { usePermissionsData } from "@/hooks/use-permissions-data";
import { useAssignPermissions, useRole } from "@/hooks/use-roles";
import type { RoleDto } from "@/types/api";
import { Check, Search } from "lucide-react";
import { cn } from "@/lib/utils/cn";

interface AssignPermissionsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  role: RoleDto | null;
}

export function AssignPermissionsDialog({
  open,
  onOpenChange,
  role,
}: AssignPermissionsDialogProps) {
  const { data: allPermissions = [] } = usePermissionsData();
  const { data: roleDetail } = useRole(role?.id ?? "");
  const assignPermissions = useAssignPermissions();
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [filter, setFilter] = useState("");

  useEffect(() => {
    if (open && roleDetail) {
      setSelected(new Set(roleDetail.permissions.map((p) => p.id)));
      setFilter("");
    }
  }, [open, roleDetail]);

  const filtered = allPermissions.filter(
    (p) =>
      p.code.toLowerCase().includes(filter.toLowerCase()) ||
      p.name.toLowerCase().includes(filter.toLowerCase())
  );

  const toggle = (id: string) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleSave = async () => {
    if (!role) return;
    await assignPermissions.mutateAsync({
      id: role.id,
      data: { permissionIds: Array.from(selected) },
    });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        title="Permisos del rol"
        description={role ? `Rol: ${role.name}` : undefined}
        size="md"
      >
        <Input
          autoFocus
          placeholder="Filtrar permisos…"
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          leftIcon={<Search className="h-3.5 w-3.5" />}
        />

        <div className="mt-3 space-y-1 max-h-72 overflow-y-auto">
          {filtered.length === 0 && (
            <p className="py-4 text-center text-[13px] text-[#5B6472]">
              Sin resultados.
            </p>
          )}
          {filtered.map((perm) => (
            <button
              key={perm.id}
              type="button"
              onClick={() => toggle(perm.id)}
              className={cn(
                "flex w-full items-center justify-between rounded-input px-3 py-2 text-left",
                "text-[13px] transition-colors duration-100",
                selected.has(perm.id)
                  ? "bg-[#C9E8ED] text-[#0F5C6B]"
                  : "hover:bg-[#F7F8FA] text-[#3A3F4B]"
              )}
            >
              <div>
                <p className="font-600 font-mono text-[12px]">{perm.code}</p>
                <p className="text-[11px] text-[#5B6472]">{perm.name}</p>
              </div>
              {selected.has(perm.id) && (
                <Check className="h-4 w-4 text-[#0F5C6B] flex-shrink-0" />
              )}
            </button>
          ))}
        </div>

        <DialogFooter>
          <span className="mr-auto text-[11px] text-[#5B6472]">
            {selected.size} permiso{selected.size !== 1 ? "s" : ""} seleccionado{selected.size !== 1 ? "s" : ""}
          </span>
          <DialogClose asChild>
            <Button type="button" variant="secondary">Cancelar</Button>
          </DialogClose>
          <Button onClick={handleSave} loading={assignPermissions.isPending}>
            Guardar
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
