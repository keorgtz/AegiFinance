"use client";

import { useState } from "react";
import { Download, FileText, Trash2, Upload } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Can } from "@/lib/auth/can";
import { clientsApi } from "@/lib/api/clients";
import { useDeleteClientDocument, useUploadClientDocument } from "@/hooks/use-clients";
import { formatDateTime } from "@/lib/utils/format";
import type { ClientDocumentDto } from "@/types/api";

export function ClientDocumentsPanel({ clientId, documents }: { clientId: string; documents: ClientDocumentDto[] }) {
  const [file, setFile] = useState<File | null>(null);
  const [description, setDescription] = useState("");
  const upload = useUploadClientDocument();
  const remove = useDeleteClientDocument();

  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!file) return;
    await upload.mutateAsync({ clientId, file, description: description || undefined });
    setFile(null);
    setDescription("");
  };

  const download = async (document: ClientDocumentDto) => {
    try {
      const blob = await clientsApi.downloadDocument(clientId, document.id);
      const url = URL.createObjectURL(blob);
      const anchor = window.document.createElement("a");
      anchor.href = url;
      anchor.download = document.name;
      anchor.click();
      URL.revokeObjectURL(url);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "No se pudo descargar el documento.");
    }
  };

  return (
    <div className="space-y-4">
      <Can permission="ManageClientDocuments">
        <form onSubmit={submit} className="grid grid-cols-1 gap-3 rounded-card border border-border bg-surface p-4 shadow-dp1 md:grid-cols-[1fr_1fr_auto] md:items-end">
          <Input controlKey="clients.documents.file" permission="ManageClientDocuments" label="Archivo" type="file" accept=".pdf,.png,.jpg,.jpeg,.doc,.docx,.xls,.xlsx,.csv,.txt" onChange={(event) => setFile(event.target.files?.[0] ?? null)} hint="PDF, imágenes, Office, CSV o TXT; máximo 10 MB." />
          <Input controlKey="clients.documents.description" permission="ManageClientDocuments" label="Descripción" value={description} onChange={(event) => setDescription(event.target.value)} placeholder="Opcional" />
          <Button controlKey="clients.documents.upload" permission="ManageClientDocuments" type="submit" loading={upload.isPending} disabled={!file}>
            <Upload className="h-4 w-4" /> Cargar
          </Button>
        </form>
      </Can>

      {documents.length === 0 ? (
        <div className="rounded-card border border-dashed border-border py-10 text-center">
          <FileText className="mx-auto mb-2 h-6 w-6 text-muted" />
          <p className="text-sm text-muted">Todavía no hay documentos para este cliente.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-3 lg:grid-cols-2">
          {documents.map((document) => (
            <article key={document.id} className="flex min-w-0 items-center gap-3 rounded-card border border-border bg-surface p-4 shadow-dp1">
              <FileText className="h-5 w-5 shrink-0 text-action" />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-semibold text-foreground">{document.name}</p>
                <p className="text-xs text-muted">{formatSize(document.sizeBytes)} · {formatDateTime(document.createdAt)}</p>
                {document.description && <p className="mt-1 text-xs text-foreground-secondary">{document.description}</p>}
              </div>
              <Button controlKey="clients.documents.download" permission="ViewClientDocuments" type="button" variant="ghost" size="icon" aria-label={`Descargar ${document.name}`} onClick={() => download(document)}>
                <Download className="h-4 w-4" />
              </Button>
              <Button controlKey="clients.documents.delete" permission="ManageClientDocuments" type="button" variant="ghost" size="icon" aria-label={`Retirar ${document.name}`} onClick={() => confirm(`¿Retirar el documento “${document.name}”?`) && remove.mutate({ clientId, documentId: document.id })}>
                <Trash2 className="h-4 w-4 text-danger" />
              </Button>
            </article>
          ))}
        </div>
      )}
    </div>
  );
}

function formatSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
