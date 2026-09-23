export const PATTERNS = {
  personName: /^[A-Za-zА-Яа-яЁёІіЎў''-]+$/,
  passportSeries: /^[A-Za-z]{2}$/,
  passportNumber: /^\d{7}$/,
  identificationNumber: /^\d{7}[A-Za-z]\d{3}[A-Za-z]{2}\d$/,
  homePhone: /^\+375 \(\d{2}\) \d{3}-\d{2}-\d{2}$/,
  mobilePhone: /^\+375 \((29|33|44|25)\) \d{3}-\d{2}-\d{2}$/,
  email: /^[^@\s]+@[^@\s]+\.[^@\s]+$/,
};

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

export function emptyToNull(value: string): string | null {
  const trimmed = value.trim();
  return trimmed === '' ? null : trimmed;
}

export const PHONE_PREFIX = '+375';

/** Digits after the country code, at most 9. */
export function nationalPhoneDigits(value: string): string {
  let rest = value.trim();
  while (rest.startsWith(PHONE_PREFIX)) {
    rest = rest.slice(PHONE_PREFIX.length).trimStart();
  }

  let digits = rest.replace(/\D/g, '');
  if (digits.startsWith('375') && digits.length > 9) {
    digits = digits.slice(3);
  }

  return digits.slice(0, 9);
}

/** +375, then (XX), then XXX-XX-XX. Groups appear only as their digits are entered. */
export function formatBelarusPhone(value: string): string {
  const digits = nationalPhoneDigits(value);
  if (digits.length === 0) {
    return PHONE_PREFIX;
  }

  let formatted = `${PHONE_PREFIX} (${digits.slice(0, Math.min(2, digits.length))}`;
  if (digits.length < 2) {
    return formatted;
  }

  formatted += ')';
  if (digits.length === 2) {
    return formatted;
  }

  formatted += ` ${digits.slice(2, Math.min(5, digits.length))}`;
  if (digits.length <= 5) {
    return formatted;
  }

  formatted += `-${digits.slice(5, Math.min(7, digits.length))}`;
  if (digits.length <= 7) {
    return formatted;
  }

  return `${formatted}-${digits.slice(7, 9)}`;
}

/** Keeps the +375 prefix and drops a digit when Backspace removes a bracket or dash. */
export function applyPhoneMask(previous: string, next: string): string {
  const prevDigits = nationalPhoneDigits(previous);
  let nextDigits = nationalPhoneDigits(next);
  const removedFormatting = next.length < previous.length && nextDigits === prevDigits;
  if (removedFormatting && prevDigits.length > 0) {
    nextDigits = prevDigits.slice(0, -1);
  }

  return formatBelarusPhone(nextDigits);
}

export function phoneOrNull(value: string): string | null {
  const formatted = formatBelarusPhone(value);
  return nationalPhoneDigits(formatted).length === 0 ? null : formatted;
}

export interface ClientFormValues {
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
  residenceCityId: string;
  residenceAddress: string;
  registrationCityId: string;
  homePhone: string;
  mobilePhone: string;
  email: string;
  workplace: string;
  position: string;
  maritalStatusId: string;
  citizenshipId: string;
  disabilityId: string;
  pensioner: boolean;
  monthlyIncome: string;
}

export const emptyForm = (): ClientFormValues => ({
  lastName: '',
  firstName: '',
  patronymic: '',
  birthDate: '',
  passportSeries: '',
  passportNumber: '',
  issuedBy: '',
  issueDate: '',
  identificationNumber: '',
  birthPlace: '',
  residenceCityId: '',
  residenceAddress: '',
  registrationCityId: '',
  homePhone: PHONE_PREFIX,
  mobilePhone: PHONE_PREFIX,
  email: '',
  workplace: '',
  position: '',
  maritalStatusId: '',
  citizenshipId: '',
  disabilityId: '',
  pensioner: false,
  monthlyIncome: '',
});

function requiredKey(value: string, key: string): string | undefined {
  if (value.trim() === '') {
    return key;
  }
  return undefined;
}

function nameError(value: string, required: string): string | undefined {
  const missing = requiredKey(value, required);
  if (missing) {
    return missing;
  }
  if (!PATTERNS.personName.test(value.trim())) {
    return 'personNameInvalid';
  }
  return undefined;
}

function optionalPattern(value: string, pattern: RegExp, key: string): string | undefined {
  if (value.trim() === '') {
    return undefined;
  }
  if (!pattern.test(value.trim())) {
    return key;
  }
  return undefined;
}

function optionalPhone(value: string, pattern: RegExp, key: string): string | undefined {
  if (nationalPhoneDigits(value).length === 0) {
    return undefined;
  }
  if (!pattern.test(value.trim())) {
    return key;
  }
  return undefined;
}

export function validateClientForm(values: ClientFormValues): Record<string, string> {
  const errors: Record<string, string> = {};
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const minDate = new Date(1900, 0, 1);

  const lastName = nameError(values.lastName, 'lastNameRequired');
  if (lastName) errors.lastName = lastName;

  const firstName = nameError(values.firstName, 'firstNameRequired');
  if (firstName) errors.firstName = firstName;

  const patronymic = nameError(values.patronymic, 'patronymicRequired');
  if (patronymic) errors.patronymic = patronymic;

  if (values.birthDate.trim() === '') {
    errors.birthDate = 'birthDateRequired';
  } else {
    const birth = parseDate(values.birthDate);
    if (!birth) {
      errors.birthDate = 'birthDateInvalid';
    } else if (birth > today) {
      errors.birthDate = 'birthDateFuture';
    } else if (birth < minDate) {
      errors.birthDate = 'birthDateTooEarly';
    }
  }

  if (values.passportSeries.trim() === '') {
    errors.passportSeries = 'passportSeriesRequired';
  } else if (!PATTERNS.passportSeries.test(values.passportSeries.trim())) {
    errors.passportSeries = 'passportSeriesInvalid';
  }

  if (values.passportNumber.trim() === '') {
    errors.passportNumber = 'passportNumberRequired';
  } else if (!PATTERNS.passportNumber.test(values.passportNumber.trim())) {
    errors.passportNumber = 'passportNumberInvalid';
  }

  if (values.issuedBy.trim() === '') {
    errors.issuedBy = 'issuedByRequired';
  }

  if (values.issueDate.trim() === '') {
    errors.issueDate = 'issueDateRequired';
  } else {
    const issue = parseDate(values.issueDate);
    if (!issue) {
      errors.issueDate = 'issueDateInvalid';
    } else if (issue > today) {
      errors.issueDate = 'issueDateFuture';
    } else {
      const birth = parseDate(values.birthDate);
      if (birth && issue <= birth) {
        errors.issueDate = 'issueDateBeforeBirth';
      }
    }
  }

  if (values.identificationNumber.trim() === '') {
    errors.identificationNumber = 'identificationNumberRequired';
  } else if (!PATTERNS.identificationNumber.test(values.identificationNumber.trim())) {
    errors.identificationNumber = 'identificationNumberInvalid';
  }

  if (values.birthPlace.trim() === '') {
    errors.birthPlace = 'birthPlaceRequired';
  }

  if (values.residenceCityId === '') {
    errors.residenceCityId = 'residenceCityRequired';
  }

  if (values.residenceAddress.trim() === '') {
    errors.residenceAddress = 'residenceAddressRequired';
  }

  if (values.registrationCityId === '') {
    errors.registrationCityId = 'registrationCityRequired';
  }

  const homePhone = optionalPhone(values.homePhone, PATTERNS.homePhone, 'homePhoneInvalid');
  if (homePhone) errors.homePhone = homePhone;

  const mobilePhone = optionalPhone(values.mobilePhone, PATTERNS.mobilePhone, 'mobilePhoneInvalid');
  if (mobilePhone) errors.mobilePhone = mobilePhone;

  const email = optionalPattern(values.email, PATTERNS.email, 'emailInvalid');
  if (email) errors.email = email;

  if (values.maritalStatusId === '') {
    errors.maritalStatusId = 'maritalStatusRequired';
  }

  if (values.citizenshipId === '') {
    errors.citizenshipId = 'citizenshipRequired';
  }

  if (values.disabilityId === '') {
    errors.disabilityId = 'disabilityRequired';
  }

  if (values.monthlyIncome.trim() !== '') {
    const income = Number(values.monthlyIncome.replace(',', '.'));
    if (Number.isNaN(income) || income < 0) {
      errors.monthlyIncome = 'incomeNegative';
    }
  }

  return errors;
}
