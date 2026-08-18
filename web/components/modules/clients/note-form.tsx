"use client";

import { useEffect, useRef } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Dialog, DialogContent, DialogFooter, DialogClose } from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import * as RadixSwitch from "@radix-ui/react-switch";
import { useAddNote } from "@/hooks/use-clients";

const schema = z.object({
  content: z.string().min(1, "La nota no puede estar vacía"),
  isPinned: z.boolean().optional(),
});

type FormValues = z.infer<typeof schema>;

interface NoteFormProps {
  open: boolean;
  onOpenChange: (v: boolean) => void;
  clientId: string;
}

export function NoteForm({ open, onOpenChange, clientId }: NoteFormProps) {
  const addNote = useAddNote();
  const textareaRef = useRef<HTMLTextAreaElement | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { content: "", isPinned: false },
  });

  const isPinned = watch("isPinned");

  useEffect(() => {
    if (open) {
      reset({ content: "", isPinned: false });
      setTimeout(() => textareaRef.current?.focus(), 80);
    }
  }, [open, reset]);

  const { ref: registerRef, ...restRegister } = register("content");

  const onSubmit = async (values: FormValues) => {
    await addNote.mutateAsync({
      clientId,
      data: { content: values.content, isPinned: values.isPinned ?? false },
    });
    onOpenChange(false);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if ((e.ctrlKey || e.metaKey) && e.key === "Enter") {
      handleSubmit(onSubmit)();
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent title="Nueva nota" size="sm">
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Textarea controlKey="ui.components.modules.clients.note.form.textarea.1"
            {...restRegister}
            ref={(el) => {
              registerRef(el);
              textareaRef.current = el;
            }}
            label="Contenido *"
            placeholder="Escribe la nota aquí… (Ctrl+Enter para guardar)"
            rows={5}
            error={errors.content?.message}
            onKeyDown={handleKeyDown}
          />

          <div className="flex items-center justify-between rounded-input border border-border px-3 py-2">
            <label htmlFor="is-pinned" className="text-[13px] font-medium text-foreground-secondary">
              Fijar nota
            </label>
            <RadixSwitch.Root
              id="is-pinned"
              checked={isPinned ?? false}
              onCheckedChange={(v) => setValue("isPinned", v)}
              className="relative inline-flex h-5 w-9 items-center rounded-full transition-colors data-[state=checked]:bg-action data-[state=unchecked]:bg-border"
            >
              <RadixSwitch.Thumb className="block h-4 w-4 rounded-full bg-surface shadow-dp1 transition-transform data-[state=checked]:translate-x-4 data-[state=unchecked]:translate-x-0.5" />
            </RadixSwitch.Root>
          </div>

          <p className="text-[11px] text-muted">Tip: Ctrl+Enter para guardar rápido</p>

          <DialogFooter>
            <DialogClose asChild>
              <Button controlKey="ui.components.modules.clients.note.form.button.1" type="button" variant="secondary">Cancelar</Button>
            </DialogClose>
            <Button controlKey="ui.components.modules.clients.note.form.button.2" type="submit" loading={isSubmitting}>Guardar nota</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
