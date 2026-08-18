"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { format } from "date-fns";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

export type ActionType = "suspend" | "reactivate" | "cancel" | "renew";

const ACTION_CONFIG: Record<
  ActionType,
  { title: string; description: string; confirmLabel: string; variant: "primary" | "danger" }
> = {
  suspend:    { title: "Suspender suscripción",  description: "La suscripción quedará en pausa. Puedes reactivarla después.",             confirmLabel: "Suspender",  variant: "danger"   },
  reactivate: { title: "Reactivar suscripción",  description: "La suscripción volverá al estado activo.",                               confirmLabel: "Reactivar", variant: "primary"  },
  cancel:     { title: "Cancelar suscripción",   description: "La suscripción será cancelada de forma permanente. No es reversible.",    confirmLabel: "Cancelar",  variant: "danger"   },
  renew:      { title: "Renovar suscripción",    description: "Se extenderá el período de la suscripción.",                              confirmLabel: "Renovar",   variant: "primary"  },
};

const schema = z.object({
  reason: z.string().optional(),
  effectiveDate: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

interface SubscriptionActionDialogProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  action: ActionType;
  onConfirm: (reason?: string | null, effectiveDate?: string | null) => Promise<void>;
}

export function SubscriptionActionDialog({
  open,
  onOpenChange,
  action,
  onConfirm,
}: SubscriptionActionDialogProps) {
  const cfg = ACTION_CONFIG[action];
  const showEffectiveDate = action === "cancel";

  const { register, handleSubmit, reset, formState: { isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { reason: "", effectiveDate: "" },
  });

  useEffect(() => {
    if (open) {
      reset({
        reason: "",
        effectiveDate: showEffectiveDate ? format(new Date(), "yyyy-MM-dd") : "",
      });
    }
  }, [open, showEffectiveDate, reset]);

  const onSubmit = async (values: FormValues) => {
    await onConfirm(values.reason || null, values.effectiveDate || null);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title={cfg.title} size="sm">
        <p className="mb-4 text-[13px] text-muted">{cfg.description}</p>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {showEffectiveDate && (
            <Input controlKey="ui.components.modules.subscriptions.subscription.action.dialog.input.1"
              {...register("effectiveDate")}
              label="Fecha efectiva"
              type="date"
            />
          )}
          <Textarea controlKey="ui.components.modules.subscriptions.subscription.action.dialog.textarea.1"
            {...register("reason")}
            label="Motivo (opcional)"
            placeholder="Ej. Solicitud del cliente"
            rows={2}
          />
          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.subscriptions.subscription.action.dialog.button.1" type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.subscriptions.subscription.action.dialog.button.2"
              type="submit"
              variant={cfg.variant}
              loading={isSubmitting}
            >
              {cfg.confirmLabel}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
