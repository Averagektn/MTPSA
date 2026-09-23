import type { CreditProduct, FieldErrors, PaymentScheduleItem } from './types';

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

export function toIso(value: string): string {
  const date = parseDate(value);
  if (!date) {
    return '';
  }

  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
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

function roundMoney(value: number): number {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}

function pow(value: number, exponent: number): number {
  let result = 1;
  for (let i = 0; i < exponent; i += 1) {
    result *= value;
  }
  return result;
}

export function buildSchedule(
  amount: number,
  annualRate: number,
  startIso: string,
  termMonths: number,
  repaymentSchedule: string,
): PaymentScheduleItem[] {
  if (!(amount > 0) || termMonths <= 0 || !startIso) {
    return [];
  }

  const monthlyRate = annualRate / 100 / 12;
  const items: PaymentScheduleItem[] = [];

  if (repaymentSchedule === 'interestOnly') {
    for (let period = 1; period <= termMonths; period += 1) {
      const interest = roundMoney(amount * monthlyRate);
      const principal = period === termMonths ? amount : 0;
      items.push({
        period,
        date: addMonths(startIso, period),
        principal,
        interest,
        payment: roundMoney(principal + interest),
        remaining: period === termMonths ? 0 : amount,
      });
    }
    return items;
  }

  const payment = monthlyRate === 0
    ? roundMoney(amount / termMonths)
    : roundMoney(amount * monthlyRate * pow(1 + monthlyRate, termMonths)
      / (pow(1 + monthlyRate, termMonths) - 1));
  let remaining = amount;
  for (let period = 1; period <= termMonths; period += 1) {
    const interest = roundMoney(remaining * monthlyRate);
    let principal: number;
    let pay: number;
    if (period === termMonths) {
      principal = remaining;
      pay = roundMoney(principal + interest);
      remaining = 0;
    } else {
      principal = roundMoney(payment - interest);
      if (principal > remaining) {
        principal = remaining;
      }
      pay = roundMoney(principal + interest);
      remaining = roundMoney(remaining - principal);
    }
    items.push({
      period,
      date: addMonths(startIso, period),
      principal,
      interest,
      payment: pay,
      remaining,
    });
  }
  return items;
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

export function applyProduct(values: ContractFormValues, product: CreditProduct): ContractFormValues {
  const startIso = toIso(values.startDate);
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

export function validateContractForm(values: ContractFormValues, product: CreditProduct | undefined): FieldErrors {
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
