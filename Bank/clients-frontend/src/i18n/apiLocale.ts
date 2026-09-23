import { readStoredLocale } from './localeStorage';
import { type Locale } from './types';

let currentLocale: Locale = readStoredLocale();

export function setApiLocale(locale: Locale): void {
  currentLocale = locale;
}

export function getApiLocale(): Locale {
  return currentLocale;
}
