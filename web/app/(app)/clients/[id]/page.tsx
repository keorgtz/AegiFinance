"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import {
  useClient,
  useClientContacts,
  useClientNotes,
  useDeleteContact,
  useSetPrimaryContact,
} from "@/hooks/use-clients";
import { Tabs, TabList, Tab, TabPanel } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Spinner } from "@/components/ui/spinner";
import { ClientStatusBadge } from "@/components/modules/clients/client-status-badge";
import { TagChip } from "@/components/modules/clients/tag-chip";
import { ClientForm } from "@/components/modules/clients/client-form";
import { ContactForm } from "@/components/modules/clients/contact-form";
import { NoteForm } from "@/components/modules/clients/note-form";
import { Can } from "@/lib/auth/can";
import { formatDate, formatDateTime } from "@/lib/utils/format";
import type { ClientContactDto } from "@/types/api";
import {
  ArrowLeft,
  Building2,
  Mail,
  MapPin,
  Pencil,
  Phone,
  Pin,
  Plus,
  Star,
  Trash2,
  User,
} from "lucide-react";
import { cn } from "@/lib/utils/cn";

export default function ClientDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const { data: client, isLoading } = useClient(params.id);
  const { data: contacts = [], isLoading: contactsLoading } = useClientContacts(params.id);
  const { data: notes = [], isLoading: notesLoading } = useClientNotes(params.id);

  const deleteContact = useDeleteContact();
  const setPrimary = useSetPrimaryContact();

  const [editOpen, setEditOpen] = useState(false);
  const [contactFormOpen, setContactFormOpen] = useState(false);
  const [editingContact, setEditingContact] = useState<ClientContactDto | null>(null);
  const [noteFormOpen, setNoteFormOpen] = useState(false);

  if (isLoading) {
    return (
      <div className="flex h-40 items-center justify-center">
        <Spinner className="text-[#0F5C6B]" />
      </div>
    );
  }

  if (!client) {
    return (
      <div className="flex h-40 flex-col items-center justify-center gap-3">
        <p className="text-[14px] text-[#5B6472]">Cliente no encontrado.</p>
        <Button variant="ghost" onClick={() => router.back()}>
          <ArrowLeft className="h-4 w-4" />
          Volver
        </Button>
      </div>
    );
  }

  const openEditContact = (contact: ClientContactDto) => {
    setEditingContact(contact);
    setContactFormOpen(true);
  };

  const pinnedNotes = notes.filter((n) => n.isPinned);
  const regularNotes = notes.filter((n) => !n.isPinned);

  return (
    <div>
      {/* Back + header */}
      <div className="mb-5">
        <button
          onClick={() => router.back()}
          className="mb-3 inline-flex items-center gap-1.5 text-[12px] text-[#5B6472] hover:text-[#0F5C6B] transition-colors"
        >
          <ArrowLeft className="h-3.5 w-3.5" />
          Clientes
        </button>

        <div className="flex items-start justify-between gap-4">
          <div className="flex items-center gap-3 min-w-0">
            <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-card bg-[#EFF9F9] text-[#0F5C6B]">
              <Building2 className="h-6 w-6" />
            </div>
            <div className="min-w-0">
              <div className="flex items-center gap-2 flex-wrap">
                <h1 className="font-display text-[22px] font-700 text-[#16181D] truncate">
                  {client.name}
                </h1>
                <ClientStatusBadge status={client.status} />
              </div>
              <div className="mt-0.5 flex items-center gap-2 flex-wrap">
                <span className="font-mono text-[12px] text-[#5B6472]">{client.code}</span>
                {client.tradeName && (
                  <span className="text-[12px] text-[#5B6472]">· {client.tradeName}</span>
                )}
                {client.categoryName && (
                  <span className="text-[12px] text-[#5B6472]">· {client.categoryName}</span>
                )}
              </div>
              {client.tags.length > 0 && (
                <div className="mt-1.5 flex flex-wrap gap-1">
                  {client.tags.map((t) => <TagChip key={t.id} tag={t} />)}
                </div>
              )}
            </div>
          </div>

          <Can permission="ManageClients">
            <Button variant="secondary" size="sm" onClick={() => setEditOpen(true)}>
              <Pencil className="h-3.5 w-3.5" />
              Editar
            </Button>
          </Can>
        </div>
      </div>

      {/* Tabs */}
      <Tabs defaultTab="info">
        <TabList>
          <Tab id="info">Información general</Tab>
          <Tab id="contacts">
            Contactos
            {contacts.length > 0 && (
              <span className="ml-1.5 rounded-full bg-[#E3E6EC] px-1.5 py-0.5 text-[10px] font-700 text-[#5B6472]">
                {contacts.length}
              </span>
            )}
          </Tab>
          <Tab id="notes">
            Notas
            {notes.length > 0 && (
              <span className="ml-1.5 rounded-full bg-[#E3E6EC] px-1.5 py-0.5 text-[10px] font-700 text-[#5B6472]">
                {notes.length}
              </span>
            )}
          </Tab>
          <Tab id="history">Historial</Tab>
        </TabList>

        {/* ── TAB: Información general ── */}
        <TabPanel id="info">
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <InfoCard title="Datos fiscales">
              <InfoRow icon={<Building2 className="h-4 w-4" />} label="Razón social" value={client.name} />
              {client.tradeName && (
                <InfoRow icon={<Building2 className="h-4 w-4" />} label="Nombre comercial" value={client.tradeName} />
              )}
              {client.taxId && (
                <InfoRow icon={<User className="h-4 w-4" />} label="RFC / ID Fiscal" value={client.taxId} mono />
              )}
            </InfoCard>

            <InfoCard title="Contacto">
              {client.billingEmail && (
                <InfoRow icon={<Mail className="h-4 w-4" />} label="Correo de facturación" value={client.billingEmail} />
              )}
              {client.phone && (
                <InfoRow icon={<Phone className="h-4 w-4" />} label="Teléfono" value={client.phone} />
              )}
              {client.billingAddress && (
                <InfoRow icon={<MapPin className="h-4 w-4" />} label="Dirección" value={client.billingAddress} />
              )}
              {!client.billingEmail && !client.phone && !client.billingAddress && (
                <p className="text-[13px] text-[#5B6472]">Sin datos de contacto registrados.</p>
              )}
            </InfoCard>

            {client.notes && (
              <div className="col-span-full">
                <InfoCard title="Notas internas">
                  <p className="whitespace-pre-wrap text-[13px] text-[#3A3F4B]">{client.notes}</p>
                </InfoCard>
              </div>
            )}
          </div>
        </TabPanel>

        {/* ── TAB: Contactos ── */}
        <TabPanel id="contacts">
          <div className="mb-4 flex items-center justify-between">
            <p className="text-[13px] text-[#5B6472]">
              {contacts.length === 0 ? "Sin contactos registrados." : `${contacts.length} contacto${contacts.length !== 1 ? "s" : ""}`}
            </p>
            <Can permission="ManageClients">
              <Button
                size="sm"
                onClick={() => { setEditingContact(null); setContactFormOpen(true); }}
              >
                <Plus className="h-3.5 w-3.5" />
                Agregar contacto
              </Button>
            </Can>
          </div>

          {contactsLoading ? (
            <Spinner className="text-[#0F5C6B]" />
          ) : (
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
              {contacts.map((contact) => (
                <div
                  key={contact.id}
                  className={cn(
                    "rounded-card border bg-white p-4 shadow-dp1",
                    contact.isPrimary ? "border-[#0F5C6B]/30" : "border-[#E3E6EC]"
                  )}
                >
                  <div className="mb-2 flex items-start justify-between gap-2">
                    <div className="flex items-center gap-1.5 min-w-0">
                      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-[#EFF9F9] text-[#0F5C6B] text-[12px] font-700">
                        {contact.name.charAt(0).toUpperCase()}
                      </div>
                      <div className="min-w-0">
                        <p className="truncate font-600 text-[#16181D]">{contact.name}</p>
                        {contact.position && (
                          <p className="truncate text-[11px] text-[#5B6472]">{contact.position}</p>
                        )}
                      </div>
                    </div>
                    {contact.isPrimary && (
                      <Star className="h-3.5 w-3.5 shrink-0 fill-[#B7791F] text-[#B7791F]" />
                    )}
                  </div>
                  <div className="space-y-1">
                    {contact.email && (
                      <a
                        href={`mailto:${contact.email}`}
                        className="flex items-center gap-1.5 text-[12px] text-[#0F5C6B] hover:underline"
                        onClick={(e) => e.stopPropagation()}
                      >
                        <Mail className="h-3 w-3 shrink-0" />
                        <span className="truncate">{contact.email}</span>
                      </a>
                    )}
                    {contact.phone && (
                      <a
                        href={`tel:${contact.phone}`}
                        className="flex items-center gap-1.5 text-[12px] text-[#3A3F4B] hover:underline"
                        onClick={(e) => e.stopPropagation()}
                      >
                        <Phone className="h-3 w-3 shrink-0" />
                        {contact.phone}
                      </a>
                    )}
                  </div>
                  <Can permission="ManageClients">
                    <div className="mt-3 flex items-center gap-1.5 border-t border-[#F7F8FA] pt-2">
                      {!contact.isPrimary && (
                        <button
                          onClick={() => setPrimary.mutate({ clientId: params.id, contactId: contact.id })}
                          className="text-[11px] text-[#5B6472] hover:text-[#0F5C6B] transition-colors"
                        >
                          Marcar principal
                        </button>
                      )}
                      <div className="ml-auto flex gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => openEditContact(contact)}
                          aria-label="Editar contacto"
                        >
                          <Pencil className="h-3.5 w-3.5" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => {
                            if (confirm(`¿Eliminar contacto "${contact.name}"?`)) {
                              deleteContact.mutate({ clientId: params.id, contactId: contact.id });
                            }
                          }}
                          aria-label="Eliminar contacto"
                        >
                          <Trash2 className="h-3.5 w-3.5 text-[#B6452C]" />
                        </Button>
                      </div>
                    </div>
                  </Can>
                </div>
              ))}
            </div>
          )}
        </TabPanel>

        {/* ── TAB: Notas ── */}
        <TabPanel id="notes">
          <div className="mb-4 flex items-center justify-between">
            <p className="text-[13px] text-[#5B6472]">
              {notes.length === 0 ? "Sin notas." : `${notes.length} nota${notes.length !== 1 ? "s" : ""}`}
            </p>
            <Can permission="ManageClients">
              <Button size="sm" onClick={() => setNoteFormOpen(true)}>
                <Plus className="h-3.5 w-3.5" />
                Nueva nota
              </Button>
            </Can>
          </div>

          {notesLoading ? (
            <Spinner className="text-[#0F5C6B]" />
          ) : (
            <div className="space-y-3">
              {pinnedNotes.length > 0 && (
                <div className="space-y-2">
                  <p className="flex items-center gap-1.5 text-[10px] font-700 uppercase tracking-wider text-[#5B6472]">
                    <Pin className="h-3 w-3" />
                    Fijadas
                  </p>
                  {pinnedNotes.map((note) => (
                    <NoteCard key={note.id} content={note.content} createdAt={note.createdAt} pinned />
                  ))}
                </div>
              )}
              {regularNotes.length > 0 && (
                <div className="space-y-2">
                  {pinnedNotes.length > 0 && (
                    <p className="text-[10px] font-700 uppercase tracking-wider text-[#5B6472]">Otras notas</p>
                  )}
                  {regularNotes.map((note) => (
                    <NoteCard key={note.id} content={note.content} createdAt={note.createdAt} />
                  ))}
                </div>
              )}
              {notes.length === 0 && (
                <div className="rounded-card border border-dashed border-[#E3E6EC] py-10 text-center">
                  <p className="text-[13px] text-[#5B6472]">Sin notas registradas.</p>
                </div>
              )}
            </div>
          )}
        </TabPanel>

        {/* ── TAB: Historial ── */}
        <TabPanel id="history">
          <div className="rounded-card border border-dashed border-[#E3E6EC] py-12 text-center">
            <p className="text-[13px] text-[#5B6472]">
              El historial de cambios estará disponible próximamente.
            </p>
          </div>
        </TabPanel>
      </Tabs>

      {/* Drawers / Dialogs */}
      <ClientForm open={editOpen} onOpenChange={setEditOpen} editingClient={client} />
      <ContactForm
        open={contactFormOpen}
        onOpenChange={(v) => { setContactFormOpen(v); if (!v) setEditingContact(null); }}
        clientId={params.id}
        editingContact={editingContact}
      />
      <NoteForm open={noteFormOpen} onOpenChange={setNoteFormOpen} clientId={params.id} />
    </div>
  );
}

/* ── Sub-components ── */

function InfoCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="rounded-card border border-[#E3E6EC] bg-white p-4 shadow-dp1">
      <p className="mb-3 text-[11px] font-700 uppercase tracking-wider text-[#5B6472]">{title}</p>
      <div className="space-y-2">{children}</div>
    </div>
  );
}

function InfoRow({
  icon,
  label,
  value,
  mono = false,
}: {
  icon: React.ReactNode;
  label: string;
  value: string;
  mono?: boolean;
}) {
  return (
    <div className="flex items-start gap-2.5">
      <span className="mt-0.5 shrink-0 text-[#5B6472]">{icon}</span>
      <div className="min-w-0">
        <p className="text-[10px] uppercase tracking-wider text-[#5B6472]">{label}</p>
        <p className={cn("mt-0.5 text-[13px] text-[#16181D]", mono && "font-mono")}>{value}</p>
      </div>
    </div>
  );
}

function NoteCard({
  content,
  createdAt,
  pinned = false,
}: {
  content: string;
  createdAt: string;
  pinned?: boolean;
}) {
  return (
    <div
      className={cn(
        "rounded-card border bg-white p-4 shadow-dp1",
        pinned ? "border-[#0F5C6B]/20 bg-[#EFF9F9]/40" : "border-[#E3E6EC]"
      )}
    >
      <p className="whitespace-pre-wrap text-[13px] text-[#3A3F4B]">{content}</p>
      <p className="mt-2 text-[11px] text-[#5B6472]">{formatDateTime(createdAt)}</p>
    </div>
  );
}
