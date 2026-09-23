import type { DepositProduct, FieldErrors } from './types';

export function parseDate(value: string): Date | null {
  const trimmed = value.trim();
  const iso = /^(\d{4})-(\d{2})-(\d{2})$/.exec(trimmed);
  const dmy = /^(\d{1,2})\.(\d{1,2})\.(\d{4})$/.exec(trimmed);

  let year: number;
  let month: number;
  let day: number;

  if (iso) {
    year = Number(iso[1]);
    month = Number(iso[2]);
    day = Number(iso[3]);
  } else if (dmy) {
    day = Number(dmy[1]);
    month = Number(dmy[2]);
    year = Number(dmy[3]);
  } else {
    return null;
  }

  const date = new Date(year, month - 1, day);
  if (date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day) {
    return null;
  }

  return date;
}

export function isoToDisplay(iso: string): string {
  const date = parseDate(iso);
  if (!date) {
    return iso;
  }

  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  return `${day}.${month}.${date.getFullYear()}`;
}

export function daysBetween(fromIso: string, toIso: string): number {
  const from = parseDate(fromIso);
  const to = parseDate(toIso);
  if (!from || !to) {
    return 1;
  }

  const ms = Date.UTC(to.getFullYear(), to.getMonth(), to.getDate())
    - Date.UTC(from.getFullYear(), from.getMonth(), from.getDate());
  return Math.max(1, Math.round(ms / 86_400_000));
}

export function addMonths(isoDate: string, months: number): string {
  const date = parseDate(isoDate);
  if (!date) {
    return '';
  }

  const result = new Date(date.getFullYear(), date.getMonth() + months, date.getDate());
  if (result.getDate() !== date.getDate()) {
    result.setDate(0);
  }

  const y = result.getFullYear();
  const m = String(result.getMonth() + 1).padStart(2, '0');
  const d = String(result.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export function formatMoney(value: number): string {
  return value.toLocaleString('ru-BY', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

export interface ContractFormValues {
  clientId: string;
  productId: string;
  number: string;
  currencyId: string;
  startDate: string;
  endDate: string;
  termMonths: string;
  amount: string;
  annualRate: string;
}

export function emptyContractForm(startIso: string): ContractFormValues {
  return {
    clientId: '',
    productId: '',
    number: '',
    currencyId: '',
    startDate: isoToDisplay(startIso),
    endDate: '',
    termMonths: '',
    amount: '',
    annualRate: '',
  };
}

export function applyProduct(values: ContractFormValues, product: DepositProduct): ContractFormValues {
  const start = parseDate(values.startDate);
  const startIso = start
    ? `${start.getFullYear()}-${String(start.getMonth() + 1).padStart(2, '0')}-${String(start.getDate()).padStart(2, '0')}`
    : '';
  const endIso = startIso ? addMonths(startIso, product.termMonths) : '';
  return {
    ...values,
    productId: String(product.id),
    currencyId: String(product.currencyId),
    termMonths: String(product.termMonths),
    annualRate: String(product.annualRate),
    endDate: endIso ? isoToDisplay(endIso) : values.endDate,
  };
}

export function validateContractForm(values: ContractFormValues, product: DepositProduct | undefined): FieldErrors {
  const errors: FieldErrors = {};

  if (!values.clientId) {
    errors.clientId = 'clientRequired';
  }
  if (!values.productId) {
    errors.productId = 'productRequired';
  }
  if (values.number.trim().length > 32) {
    errors.number = 'contractNumberTooLong';
  }
  if (!values.currencyId) {
    errors.currencyId = 'currencyRequired';
  }
  if (!values.startDate.trim()) {
    errors.startDate = 'startDateRequired';
  } else if (!parseDate(values.startDate)) {
    errors.startDate = 'startDateInvalid';
  }
  if (!values.endDate.trim()) {
    errors.endDate = 'endDateRequired';
  } else if (!parseDate(values.endDate)) {
    errors.endDate = 'endDateInvalid';
  }
  if (!values.termMonths.trim() || Number(values.termMonths) <= 0) {
    errors.termMonths = 'termRequired';
  }
  const amount = Number(values.amount.trim().replace(',', '.'));
  if (!values.amount.trim() || Number.isNaN(amount) || amount <= 0) {
    errors.amount = 'amountRequired';
  } else if (product && amount < product.minAmount) {
    errors.amount = 'amountBelowMin';
  } else if (product && amount > product.maxAmount) {
    errors.amount = 'amountAboveMax';
  }
  if (!values.annualRate.trim() || Number(values.annualRate.replace(',', '.')) <= 0) {
    errors.annualRate = 'rateRequired';
  }

  return errors;
}
