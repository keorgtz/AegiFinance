export type OfflineDraft<T> = {
  id: string;
  kind: string;
  scope: string;
  savedAt: string;
  value: T;
};

export type DraftScope = { organizationId: string; userId: string };

const PREFIX = "aegifinance:draft:";

function scopeKey(scope: DraftScope) {
  if (!scope.organizationId || !scope.userId) throw new Error("Offline drafts require organization and user scope.");
  return `${scope.organizationId}:${scope.userId}`;
}

export function saveExplicitDraft<T>(kind: string, scope: DraftScope, value: T, id = crypto.randomUUID()): OfflineDraft<T> {
  const key = scopeKey(scope);
  const draft = { id, kind, scope: key, savedAt: new Date().toISOString(), value };
  localStorage.setItem(`${PREFIX}${key}:${kind}:${id}`, JSON.stringify(draft));
  return draft;
}

export function listExplicitDrafts<T>(kind: string, scope: DraftScope): OfflineDraft<T>[] {
  const prefix = `${PREFIX}${scopeKey(scope)}:${kind}:`;
  return Object.keys(localStorage).filter((key) => key.startsWith(prefix)).flatMap((key) => {
    try { return [JSON.parse(localStorage.getItem(key) ?? "") as OfflineDraft<T>]; } catch { return []; }
  }).sort((left, right) => right.savedAt.localeCompare(left.savedAt));
}

export function removeExplicitDraft(kind: string, scope: DraftScope, id: string) {
  localStorage.removeItem(`${PREFIX}${scopeKey(scope)}:${kind}:${id}`);
}
