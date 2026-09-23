export interface AtmOperator {
  code: string;
  nameEn: string;
  nameRu: string;
}

export interface AtmAuthorize {
  cardNumber: string;
  cardNumberMasked: string;
  clientName: string;
  clientId: number;
  contractId: number;
  contractNumber: string;
  creditAccountNumber: string;
  availableBalance: number;
  currencyCode: string;
}

export interface AtmReceipt {
  titleKey: string;
  printedAt: string;
  cardNumberMasked: string;
  operation: string;
  amount?: number | null;
  balance?: number | null;
  operatorCode?: string | null;
  phone?: string | null;
  creditAccountNumber?: string | null;
  clientName: string;
  depositAccountNumber?: string | null;
}

export interface AtmDepositItem {
  contractNumber: string;
  accountNumber: string;
  productName: string;
  amount: number;
  currencyCode: string;
}

export interface AtmTransactionResult {
  success: boolean;
  messageKey: string;
  operation: string;
  availableBalance?: number | null;
  creditAccountNumber?: string | null;
  clientName?: string | null;
  currencyCode?: string | null;
  receipt?: AtmReceipt | null;
  deposits?: AtmDepositItem[] | null;
}

export interface AtmField {
  key: string;
  value: string;
}

export interface AtmSession {
  id: string;
  screen: string;
  cardNumberMasked?: string | null;
  pinAttemptsLeft: number;
  messageKey?: string | null;
  fields: AtmField[];
  account?: AtmAuthorize | null;
  lastResult?: AtmTransactionResult | null;
  receipt?: AtmReceipt | null;
  operators: AtmOperator[];
}

export interface ValidationProblem {
  title?: string;
  errors?: Record<string, string[]>;
}
