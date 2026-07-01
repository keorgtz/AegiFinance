"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Select, SelectItem } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { useGenerateBilling } from "@/hooks/use-billing";

const MONTHS = [
  { value: "1", label: "Enero" }, { value: "2", label: "Febrero" },
  { value: "3", label: "Marzo" }, { value: "4", label: "Abril" },
  { value: "5", label: "Mayo" }, { value: "6", label: "Junio" },
  { value: "7", label: "Julio" }, { value: "8", label: "Agosto" },
  { value: "9", label: "Septiembre" }, { value: "10", label: "Octubre" },
  { value: "11", label: "Noviembre" }, { value: "12", label: "Diciembre" },
];

const schema = z.object({
  year: z.coerce.number().int().min(2020).max(2099),
  month: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

interface GenerateBillingDialogProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
}

export function GenerateBillingDialog({ open, onOpenChange }: GenerateBillingDialogProps) {
  const generate = useGenerateBilling();
  const now = new Date();

  const { register, handleSubmit, reset, setValue, watch, formState: { errors, isSubmitting } } =
    useForm<FormValues>({
      resolver: zodResolver(schema),
      defaultValues: { year: now.getFullYear(), month: String(now.getMonth() + 1) },
    });

  const month = watch("month");

  useEffect(() => {
    if (open) reset({ year: now.getFullYear(), month: String(now.getMonth() + 1) });
  }, [open]); // eslint-disable-line react-hooks/exhaustive-deps

  const onSubmit = async (values: FormValues) => {
    await generate.mutateAsync({
      year: values.year,
      month: values.month ? Number(values.month) : null,
    });
    onOpenChange(false);
  };

  const currentYear = now.getFullYear();
  const years = Array.from({ length: 6 }, (_, i) => currentYear - 2 + i);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Generar facturación" size="sm">
        <p className="mb-4 text-[13px] text-[#5B6472]">
          Genera los cargos de facturación para el período indicado. Los cargos ya existentes no se duplican.
        </p>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-3">
            <Select
              label="Año *"
              value={String(watch("year"))}
              onValueChange={(v) => setValue("year", Number(v))}
            >
              {years.map((y) => (
                <SelectItem key={y} value={String(y)}>{y}</SelectItem>
              ))}
            </Select>
            <Select
              label="Mes (vacío = anual)"
              value={month ?? ""}
              onValueChange={(v) => setValue("month", v)}
            >
              <SelectItem value="">Sin mes (anual)</SelectItem>
              {MONTHS.map((m) => (
                <SelectItem key={m.value} value={m.value}>{m.label}</SelectItem>
              ))}
            </Select>
          </div>

          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button type="submit" loading={isSubmitting}>
              Generar cargos
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
