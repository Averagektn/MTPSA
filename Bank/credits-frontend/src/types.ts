export interface LookupItem {
  id: number;
  name: string;
}

export interface ClientListItem {
  id: number;
  lastName: string;
  firstName: string;
  patronymic: string;
}

export interface CreditProduct {
  id: number;
  code: string;
  name: string;
  repaymentSchedule: string;
  termMonths: number;
  annualRate: number;
  minAmount: number;
  maxAmount: number;
  currencyId: number;
}

export interface CreditDictionaries {
  products: CreditProduct[];
  currencies: LookupItem[];
  clients: ClientListItem[];
}

export interface PaymentScheduleItem {
  period: number;
  date: string;
  principal: number;
  interest: number;
  payment: number;
  remaining: number;
}

export interface CreditContractListItem {
  id: number;
  number: string;
  clientId: number;
  clientName: string;
  productName: string;
  repaymentSchedule: string;
  currencyCode: string;
  startDate: string;
  endDate: string;
  termMonths: number;
  amount: number;
  remainingPrincipal: number;
  annualRate: number;
  status: string;
  principalAccountNumber: string;
  interestAccountNumber: string;
  nextPaymentDate: string | null;
}

export interface CreditContractDetail extends CreditContractListItem {
  productId: number;
  currencyId: number;
  principalAccountId: number;
  interestAccountId: number;
  schedule: PaymentScheduleItem[];
}

export interface CreditContractPayload {
  clientId: number;
  productId: number;
  number: string;
  currencyId: number;
  startDate: string;
  endDate: string;
  termMonths: number;
  amount: number;
  annualRate: number;
}

export interface AccountReportItem {
  number: string;
  name: string;
  chartCode: string;
  chartName: string;
  nature: string;
  debit: number;
  credit: number;
  saldo: number;
  currencyCode: string;
  debitByn: number;
  creditByn: number;
  saldoByn: number;
}

export interface BankingDay {
  currentDate: string;
}

export interface ValidationProblem {
  title?: string;
  errors?: Record<string, string[]>;
}

export type FieldErrors = Record<string, string>;
