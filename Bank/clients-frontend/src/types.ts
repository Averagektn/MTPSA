export interface LookupItem {
  id: number;
  name: string;
}

export interface Dictionaries {
  cities: LookupItem[];
  maritalStatuses: LookupItem[];
  citizenships: LookupItem[];
  disabilities: LookupItem[];
}

export interface ClientListItem {
  id: number;
  lastName: string;
  firstName: string;
  patronymic: string;
  birthDate: string;
  passportSeries: string;
  passportNumber: string;
  identificationNumber: string;
  residenceCity: string;
  mobilePhone: string | null;
  email: string | null;
}

export interface ClientDetails {
  id: number;
  lastName: string;
  firstName: string;
  patronymic: string;
  birthDate: string;
  passportSeries: string;
  passportNumber: string;
  issuedBy: string;
  issueDate: string;
  identificationNumber: string;
  birthPlace: string;
  residenceCityId: number;
  residenceCity: string;
  residenceAddress: string;
  registrationCityId: number;
  registrationCity: string;
  homePhone: string | null;
  mobilePhone: string | null;
  email: string | null;
  workplace: string | null;
  position: string | null;
  maritalStatusId: number;
  maritalStatus: string;
  citizenshipId: number;
  citizenship: string;
  disabilityId: number;
  disability: string;
  pensioner: boolean;
  monthlyIncome: number | null;
}

export interface ClientPayload {
  lastName: string;
  firstName: string;
  patronymic: string;
  birthDate: string;
  passportSeries: string;
  passportNumber: string;
  issuedBy: string;
  issueDate: string;
  identificationNumber: string;
  birthPlace: string;
  residenceCityId: number;
  residenceAddress: string;
  registrationCityId: number;
  homePhone: string | null;
  mobilePhone: string | null;
  email: string | null;
  workplace: string | null;
  position: string | null;
  maritalStatusId: number;
  citizenshipId: number;
  disabilityId: number;
  pensioner: boolean;
  monthlyIncome: number | null;
}

export interface ValidationProblem {
  title?: string;
  errors?: Record<string, string[]>;
}

/** Field name → i18n message key (from the API or client-side validation). */
export type FieldErrors = Record<string, string>;
