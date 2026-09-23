import { getApiLocale } from './i18n/apiLocale';
import type { AtmSession, ValidationProblem } from './types';

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

async function readSession(response: Response): Promise<AtmSession> {
  if (!response.ok) {
    throw new Error(await readError(response));
  }
  return response.json() as Promise<AtmSession>;
}

export async function insertCard(cardNumber: string): Promise<AtmSession> {
  const response = await fetch('/api/atm/sessions', {
    method: 'POST',
    headers: languageHeaders(true),
    body: JSON.stringify({ cardNumber }),
  });
  return readSession(response);
}

export async function fetchSession(id: string): Promise<AtmSession> {
  const response = await fetch(`/api/atm/sessions/${id}`, { headers: languageHeaders() });
  return readSession(response);
}

export async function submitInput(id: string, kind: string, value: string): Promise<AtmSession> {
  const response = await fetch(`/api/atm/sessions/${id}/input`, {
    method: 'POST',
    headers: languageHeaders(true),
    body: JSON.stringify({ kind, value }),
  });
  return readSession(response);
}
