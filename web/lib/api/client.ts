import type { ApiError } from "@/types/api";

const API_BASE = "/api";

// Token en memoria — nunca en localStorage
let accessToken: string | null = null;
let isRefreshing = false;
let refreshQueue: Array<(token: string | null) => void> = [];

export function setAccessToken(token: string | null) {
  accessToken = token;
}

export function getAccessToken(): string | null {
  return accessToken;
}

function drainQueue(token: string | null) {
  refreshQueue.forEach((resolve) => resolve(token));
  refreshQueue = [];
}

async function refreshAccessToken(): Promise<string | null> {
  if (isRefreshing) {
    return new Promise((resolve) => {
      refreshQueue.push(resolve);
    });
  }

  isRefreshing = true;

  try {
    const res = await fetch(`${API_BASE}/auth/refresh`, {
      method: "POST",
      credentials: "include",
    });

    if (!res.ok) {
      drainQueue(null);
      accessToken = null;
      return null;
    }

    const data = await res.json();
    accessToken = data.accessToken;
    drainQueue(accessToken);
    return accessToken;
  } catch {
    drainQueue(null);
    accessToken = null;
    return null;
  } finally {
    isRefreshing = false;
  }
}

export class ApiRequestError extends Error {
  constructor(
    public readonly status: number,
    public readonly error: ApiError
  ) {
    const validationMessage = error.errors
      ? Object.values(error.errors).flat().filter(Boolean).join(" ")
      : undefined;
    super(validationMessage ?? error.detail ?? error.title ?? error.message ?? "Error desconocido");
  }
}

interface FetchOptions extends Omit<RequestInit, "body"> {
  body?: unknown;
}

async function apiFetch<T>(
  path: string,
  options: FetchOptions = {},
  retry = true
): Promise<T> {
  const { body, ...rest } = options;

  const isForm = typeof FormData !== "undefined" && body instanceof FormData;
  const headers: Record<string, string> = isForm ? {} : { "Content-Type": "application/json" };

  if (accessToken) {
    headers["Authorization"] = `Bearer ${accessToken}`;
  }

  const res = await fetch(`${API_BASE}${path}`, {
    ...rest,
    credentials: "include",
    headers: {
      ...headers,
      ...(rest.headers as Record<string, string> | undefined),
    },
    body: body !== undefined ? (isForm ? body as FormData : JSON.stringify(body)) : undefined,
  });

  // Token expirado — intentar refresh una vez
  if (res.status === 401 && retry) {
    const newToken = await refreshAccessToken();

    if (newToken) {
      return apiFetch<T>(path, options, false);
    }

    // Redirect a login — dispatched como evento para que el contexto lo maneje
    if (typeof window !== "undefined") {
      window.dispatchEvent(new CustomEvent("aegifinance:unauthorized"));
    }

    throw new ApiRequestError(401, { message: "Sesión expirada" });
  }

  if (res.status === 204) {
    return undefined as T;
  }

  const json = await res.json().catch(() => ({}));

  if (!res.ok) {
    throw new ApiRequestError(res.status, json as ApiError);
  }

  return json as T;
}

export const api = {
  get<T>(path: string, params?: Record<string, string | number | boolean | undefined>) {
    const url = params
      ? `${path}?${new URLSearchParams(
          Object.fromEntries(
            Object.entries(params)
              .filter(([, v]) => v !== undefined && v !== "")
              .map(([k, v]) => [k, String(v)])
          )
        ).toString()}`
      : path;
    return apiFetch<T>(url, { method: "GET" });
  },

  post<T>(path: string, body?: unknown) {
    return apiFetch<T>(path, { method: "POST", body });
  },

  put<T>(path: string, body?: unknown) {
    return apiFetch<T>(path, { method: "PUT", body });
  },

  delete<T = void>(path: string) {
    return apiFetch<T>(path, { method: "DELETE" });
  },

  postForm<T>(path: string, body: FormData) {
    return apiFetch<T>(path, { method: "POST", body });
  },

  async blob(path: string, retry = true): Promise<Blob> {
    const res = await fetch(`${API_BASE}${path}`, { credentials: "include", headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {} });
    if (res.status === 401 && retry && await refreshAccessToken()) return api.blob(path, false);
    if (!res.ok) throw new ApiRequestError(res.status, await res.json().catch(() => ({ message: "No se pudo descargar el documento" })));
    return res.blob();
  },
};
