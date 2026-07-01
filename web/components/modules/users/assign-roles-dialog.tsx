"use client";

import { useState, useEffect } from "react";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { useRoles } from "@/hooks/use-roles";
import { useAssignRoles } from "@/hooks/use-users";
import type { UserDto } from "@/types/api";
import { Check } from "lucide-react";
import { cn } from "@/lib/utils/cn";

interface AssignRolesDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: UserDto | null;
}

export function AssignRolesDialog({ open, onOpenChange, user }: AssignRolesDialogProps) {
  const { data: allRoles = [] } = useRoles();
  const assignRoles = useAssignRoles();
  const [selected, setSelected] = useState<Set<string>>(new Set());

  useEffect(() => {
    if (open && user) {
      const roleCodes = new Set<string>();
      allRoles.forEach((r) => {
        if (user.roles.includes(r.name)) roleCodes.add(r.id);
      });
      setSelected(roleCodes);
    }
  }, [open, user, allRoles]);

  const toggle = (id: string) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleSave = async () => {
    if (!user) return;
    await assignRoles.mutateAsync({
      id: user.id,
      data: { roleIds: Array.from(selected) },
    });
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        title="Asignar roles"
        description={user ? `Usuario: ${user.userName}` : undefined}
        size="sm"
      >
        <div className="space-y-1 max-h-60 overflow-y-auto">
          {allRoles.map((role) => (
            <button
              key={role.id}
              type="button"
              onClick={() => toggle(role.id)}
              className={cn(
                "flex w-full items-center justify-between rounded-input px-3 py-2 text-[13px]",
                "transition-colors duration-100 text-left",
                selected.has(role.id)
                  ? "bg-[#C9E8ED] text-[#0F5C6B]"
                  : "hover:bg-[#F7F8FA] text-[#3A3F4B]"
              )}
            >
              <div>
                <p className="font-600">{role.name}</p>
                {role.description && (
                  <p className="text-[11px] text-[#5B6472]">{role.description}</p>
                )}
              </div>
              {selected.has(role.id) && (
                <Check className="h-4 w-4 text-[#0F5C6B] flex-shrink-0" />
              )}
            </button>
          ))}
        </div>

        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="secondary">Cancelar</Button>
          </DialogClose>
          <Button onClick={handleSave} loading={assignRoles.isPending}>
            Guardar
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
