"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { clientsApi } from "@/lib/api/clients";
import type {
  AddContactRequest,
  AddNoteRequest,
  CreateClientRequest,
  GetClientsParams,
  UpdateClientRequest,
  ClientDuplicateRuleDto,
} from "@/types/api";
import { toast } from "sonner";

export const clientKeys = {
  all: ["clients"] as const,
  lists: () => [...clientKeys.all, "list"] as const,
  list: (p: GetClientsParams) => [...clientKeys.lists(), p] as const,
  detail: (id: string) => [...clientKeys.all, "detail", id] as const,
  contacts: (id: string) => [...clientKeys.all, "contacts", id] as const,
  notes: (id: string) => [...clientKeys.all, "notes", id] as const,
  timeline: (id: string) => [...clientKeys.all, "timeline", id] as const,
};

export function useClients(params: GetClientsParams = {}) {
  return useQuery({
    queryKey: clientKeys.list(params),
    queryFn: () => clientsApi.list(params),
  });
}

export function useClientTimeline(clientId: string) { return useQuery({ queryKey: clientKeys.timeline(clientId), queryFn: () => clientsApi.timeline(clientId), enabled: !!clientId }); }
export function useDuplicateRule() { return useQuery({ queryKey: [...clientKeys.all, "duplicate-rule"], queryFn: clientsApi.duplicateRule }); }
export function useUpdateDuplicateRule() { const qc = useQueryClient(); return useMutation({ mutationFn: (data: ClientDuplicateRuleDto) => clientsApi.updateDuplicateRule(data), onSuccess: () => { qc.invalidateQueries({ queryKey: [...clientKeys.all, "duplicate-rule"] }); toast.success("Reglas de duplicados actualizadas."); }, onError: (error: Error) => toast.error(error.message) }); }
export function useClientDuplicates(name: string, taxId?: string, billingEmail?: string, excludeClientId?: string) { return useQuery({ queryKey: [...clientKeys.all, "duplicates", name, taxId, billingEmail, excludeClientId], queryFn: () => clientsApi.duplicates({ name, taxId, billingEmail, excludeClientId }), enabled: name.trim().length >= 3 }); }
export function useUploadClientDocument() { const qc = useQueryClient(); return useMutation({ mutationFn: ({ clientId, file, description }: { clientId: string; file: File; description?: string }) => clientsApi.uploadDocument(clientId, file, description), onSuccess: (_, { clientId }) => { qc.invalidateQueries({ queryKey: clientKeys.detail(clientId) }); qc.invalidateQueries({ queryKey: clientKeys.timeline(clientId) }); toast.success("Documento cargado."); }, onError: (error: Error) => toast.error(error.message) }); }
export function useDeleteClientDocument() { const qc = useQueryClient(); return useMutation({ mutationFn: ({ clientId, documentId }: { clientId: string; documentId: string }) => clientsApi.deleteDocument(clientId, documentId), onSuccess: (_, { clientId }) => { qc.invalidateQueries({ queryKey: clientKeys.detail(clientId) }); qc.invalidateQueries({ queryKey: clientKeys.timeline(clientId) }); toast.success("Documento retirado."); }, onError: (error: Error) => toast.error(error.message) }); }

export function useClient(id: string) {
  return useQuery({
    queryKey: clientKeys.detail(id),
    queryFn: () => clientsApi.getById(id),
    enabled: !!id,
  });
}

export function useClientContacts(clientId: string) {
  return useQuery({
    queryKey: clientKeys.contacts(clientId),
    queryFn: () => clientsApi.getContacts(clientId),
    enabled: !!clientId,
  });
}

export function useClientNotes(clientId: string) {
  return useQuery({
    queryKey: clientKeys.notes(clientId),
    queryFn: () => clientsApi.getNotes(clientId),
    enabled: !!clientId,
  });
}

export function useCreateClient() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateClientRequest) => clientsApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: clientKeys.lists() });
      toast.success("Cliente creado correctamente.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useUpdateClient() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateClientRequest }) =>
      clientsApi.update(id, data),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: clientKeys.lists() });
      qc.invalidateQueries({ queryKey: clientKeys.detail(id) });
      toast.success("Cliente actualizado.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useDeleteClient() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => clientsApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: clientKeys.lists() });
      toast.success("Cliente eliminado.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useToggleClientStatus() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, active }: { id: string; active: boolean }) =>
      active ? clientsApi.activate(id) : clientsApi.deactivate(id),
    onSuccess: (_, { id, active }) => {
      qc.invalidateQueries({ queryKey: clientKeys.lists() });
      qc.invalidateQueries({ queryKey: clientKeys.detail(id) });
      toast.success(active ? "Cliente activado." : "Cliente desactivado.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useAssignClientTags() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, tagIds }: { id: string; tagIds: string[] }) =>
      clientsApi.assignTags(id, tagIds),
    onSuccess: (_, { id }) => {
      qc.invalidateQueries({ queryKey: clientKeys.detail(id) });
      qc.invalidateQueries({ queryKey: clientKeys.lists() });
      toast.success("Etiquetas actualizadas.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useAddContact() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ clientId, data }: { clientId: string; data: AddContactRequest }) =>
      clientsApi.addContact(clientId, data),
    onSuccess: (_, { clientId }) => {
      qc.invalidateQueries({ queryKey: clientKeys.contacts(clientId) });
      qc.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
      toast.success("Contacto agregado.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useUpdateContact() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({
      clientId,
      contactId,
      data,
    }: {
      clientId: string;
      contactId: string;
      data: AddContactRequest;
    }) => clientsApi.updateContact(clientId, contactId, data),
    onSuccess: (_, { clientId }) => {
      qc.invalidateQueries({ queryKey: clientKeys.contacts(clientId) });
      qc.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
      toast.success("Contacto actualizado.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useDeleteContact() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ clientId, contactId }: { clientId: string; contactId: string }) =>
      clientsApi.deleteContact(clientId, contactId),
    onSuccess: (_, { clientId }) => {
      qc.invalidateQueries({ queryKey: clientKeys.contacts(clientId) });
      qc.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
      toast.success("Contacto eliminado.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useSetPrimaryContact() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ clientId, contactId }: { clientId: string; contactId: string }) =>
      clientsApi.setPrimaryContact(clientId, contactId),
    onSuccess: (_, { clientId }) => {
      qc.invalidateQueries({ queryKey: clientKeys.contacts(clientId) });
      qc.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
    },
    onError: (err: Error) => toast.error(err.message),
  });
}

export function useAddNote() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ clientId, data }: { clientId: string; data: AddNoteRequest }) =>
      clientsApi.addNote(clientId, data),
    onSuccess: (_, { clientId }) => {
      qc.invalidateQueries({ queryKey: clientKeys.notes(clientId) });
      qc.invalidateQueries({ queryKey: clientKeys.detail(clientId) });
      toast.success("Nota agregada.");
    },
    onError: (err: Error) => toast.error(err.message),
  });
}
