import { getApiLocale } from './i18n/apiLocale';
import type {
  AccountReportItem,
  BankingDay,
  DepositContractListItem,
  DepositContractPayload,
  DepositDictionaries,
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

export interface SaveResult {
  ok: boolean;
  fieldErrors: FieldErrors;
  message?: string;
}

export async function fetchDepositDictionaries(): Promise<DepositDictionaries> {
  const response = await fetch('/api/deposits/dictionaries', { headers: languageHeaders() });
  if (!response.ok) {
    throw new Error(await readError(response));
  }
  return response.json();
}

export async function fetchContracts(): Promise<DepositContractListItem[]> {
  const response = await fetch('/api/deposits/contracts', { headers: languageHeaders() });
  if (!response.ok) {
    throw new Error(await readError(response));
  }
  return response.json();
}

export async function createContract(payload: DepositContractPayload): Promise<SaveResult> {
  const response = await fetch('/api/deposits/contracts', {
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

export async function fetchAccounts(): Promise<AccountReportItem[]> {
  const response = await fetch('/api/accounts', { headers: languageHeaders() });
  if (!response.ok) {
    throw new Error(await readError(response));
  }
  return response.json();
}

export async function fetchBankingDay(): Promise<BankingDay> {
  const response = await fetch('/api/banking-day', { headers: languageHeaders() });
  if (!response.ok) {
    throw new Error(await readError(response));
  }
  return response.json();
}

export async function closeBankingDay(days = 1): Promise<BankingDay> {
  const response = await fetch(`/api/banking-day/close?days=${days}`, {
    method: 'POST',
    headers: languageHeaders(),
  });
  if (!response.ok) {
    throw new Error(await readError(response));
  }
  return response.json();
}
