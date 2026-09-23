import { getApiLocale } from './i18n/apiLocale';
import type {
  ClientDetails,
  ClientListItem,
  ClientPayload,
  Dictionaries,
  FieldErrors,
  ValidationProblem,
} from './types';

function languageHeaders(json = false): HeadersInit {
  const headers: Record<string, string> = {
    'Accept-Language': getApiLocale(),
  };
  if (json) {
    headers['Content-Type'] = 'application/json';
  }
  return headers;
}

async function readError(response: Response): Promise<string> {
  try {
    const problem = (await response.json()) as ValidationProblem;
    const first = problem.errors
      ? Object.values(problem.errors).flat()[0]
      : undefined;
    return first ?? problem.title ?? `Request failed (${response.status})`;
  } catch {
    return `Request failed (${response.status})`;
  }
}

export function problemToFieldErrors(problem: ValidationProblem): FieldErrors {
  const errors: FieldErrors = {};
  if (!problem.errors) {
    return errors;
  }

  for (const [key, messages] of Object.entries(problem.errors)) {
    if (messages.length > 0) {
      errors[key] = messages[0];
    }
  }
  return errors;
}

export async function fetchDictionaries(): Promise<Dictionaries> {
  const response = await fetch('/api/dictionaries', { headers: languageHeaders() });
  if (!response.ok) {
    throw new Error(await readError(response));
  }
  return response.json();
}

export async function fetchClients(): Promise<ClientListItem[]> {
  const response = await fetch('/api/clients', { headers: languageHeaders() });
  if (!response.ok) {
    throw new Error(await readError(response));
  }
  return response.json();
}

export async function fetchClient(id: number): Promise<ClientDetails> {
  const response = await fetch(`/api/clients/${id}`, { headers: languageHeaders() });
  if (!response.ok) {
    throw new Error(await readError(response));
  }
  return response.json();
}

export interface SaveResult {
  ok: boolean;
  fieldErrors: FieldErrors;
  message?: string;
}

export async function createClient(payload: ClientPayload): Promise<SaveResult> {
  const response = await fetch('/api/clients', {
    method: 'POST',
    headers: languageHeaders(true),
    body: JSON.stringify(payload),
  });

  if (response.ok) {
    return { ok: true, fieldErrors: {} };
  }

  if (response.status === 400) {
    const problem = (await response.json()) as ValidationProblem;
    return { ok: false, fieldErrors: problemToFieldErrors(problem), message: problem.title };
  }

  return { ok: false, fieldErrors: {}, message: await readError(response) };
}

export async function updateClient(id: number, payload: ClientPayload): Promise<SaveResult> {
  const response = await fetch(`/api/clients/${id}`, {
    method: 'PUT',
    headers: languageHeaders(true),
    body: JSON.stringify(payload),
  });

  if (response.ok) {
    return { ok: true, fieldErrors: {} };
  }

  if (response.status === 400) {
    const problem = (await response.json()) as ValidationProblem;
    return { ok: false, fieldErrors: problemToFieldErrors(problem), message: problem.title };
  }

  return { ok: false, fieldErrors: {}, message: await readError(response) };
}

export async function deleteClient(id: number): Promise<void> {
  const response = await fetch(`/api/clients/${id}`, {
    method: 'DELETE',
    headers: languageHeaders(),
  });
  if (!response.ok && response.status !== 204) {
    throw new Error(await readError(response));
  }
}
