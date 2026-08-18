import { api } from "./client";
import type {
  AddContactRequest,
  AddNoteRequest,
  ClientContactDto,
  ClientDetailDto,
  ClientDto,
  ClientListDto,
  ClientNoteDto,
  CreateClientRequest,
  GetClientsParams,
  PaginatedList,
  UpdateClientRequest,
  ClientDocumentDto,
  ClientTimelineItemDto,
  ClientDuplicateRuleDto,
  ClientDuplicateMatchDto,
} from "@/types/api";

export const clientsApi = {
  list(params?: GetClientsParams) {
    return api.get<PaginatedList<ClientListDto>>(
      "/clients",
      params as Record<string, string | number | boolean | undefined>
    );
  },

  getById(id: string) {
    return api.get<ClientDetailDto>(`/clients/${id}`);
  },

  create(data: CreateClientRequest) {
    return api.post<ClientDto>("/clients", data);
  },

  update(id: string, data: UpdateClientRequest) {
    return api.put<ClientDto>(`/clients/${id}`, data);
  },

  delete(id: string) {
    return api.delete(`/clients/${id}`);
  },

  activate(id: string) {
    return api.post<void>(`/clients/${id}/activate`);
  },

  deactivate(id: string) {
    return api.post<void>(`/clients/${id}/deactivate`);
  },

  assignTags(id: string, tagIds: string[]) {
    return api.post<void>(`/clients/${id}/tags`, { tagIds });
  },

  getContacts(id: string) {
    return api.get<ClientContactDto[]>(`/clients/${id}/contacts`);
  },

  addContact(id: string, data: AddContactRequest) {
    return api.post<ClientContactDto>(`/clients/${id}/contacts`, data);
  },

  updateContact(id: string, contactId: string, data: AddContactRequest) {
    return api.put<ClientContactDto>(`/clients/${id}/contacts/${contactId}`, data);
  },

  deleteContact(id: string, contactId: string) {
    return api.delete(`/clients/${id}/contacts/${contactId}`);
  },

  setPrimaryContact(id: string, contactId: string) {
    return api.post<void>(`/clients/${id}/contacts/${contactId}/primary`);
  },

  getNotes(id: string) {
    return api.get<ClientNoteDto[]>(`/clients/${id}/notes`);
  },

  addNote(id: string, data: AddNoteRequest) {
    return api.post<ClientNoteDto>(`/clients/${id}/notes`, data);
  },

  timeline(id: string) { return api.get<ClientTimelineItemDto[]>(`/client-governance/clients/${id}/timeline`); },
  duplicateRule() { return api.get<ClientDuplicateRuleDto>("/client-governance/duplicate-rule"); },
  updateDuplicateRule(data: ClientDuplicateRuleDto) { return api.put<ClientDuplicateRuleDto>("/client-governance/duplicate-rule", data); },
  duplicates(params: { name: string; taxId?: string; billingEmail?: string; excludeClientId?: string }) { return api.get<ClientDuplicateMatchDto[]>("/client-governance/duplicates", params); },
  uploadDocument(id: string, file: File, description?: string) { const data = new FormData(); data.append("file", file); if (description) data.append("description", description); return api.postForm<ClientDocumentDto>(`/clients/${id}/documents`, data); },
  deleteDocument(id: string, documentId: string) { return api.delete(`/clients/${id}/documents/${documentId}`); },
  downloadDocument(id: string, documentId: string) { return api.blob(`/clients/${id}/documents/${documentId}`); },
};
