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

export interface DepositProduct {
  id: number;
  code: string;
  name: string;
  revocable: boolean;
  interestSchedule: string;
  termMonths: number;
  annualRate: number;
  minAmount: number;
  maxAmount: number;
  currencyId: number;
}

export interface DepositDictionaries {
  products: DepositProduct[];
  currencies: LookupItem[];
  clients: ClientListItem[];
}

export interface DepositContractListItem {
  id: number;
  number: string;
  clientId: number;
  clientName: string;
  productName: string;
  currencyCode: string;
  startDate: string;
  endDate: string;
  termMonths: number;
  amount: number;
  annualRate: number;
  status: string;
  principalAccountNumber: string;
  interestAccountNumber: string;
  nextPaymentDate: string | null;
}

export interface DepositContractPayload {
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
