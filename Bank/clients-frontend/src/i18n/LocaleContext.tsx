import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from 'react';
import { setApiLocale } from './apiLocale';
import { en } from './en';
import { consumeLangQuery, readStoredLocale, writeStoredLocale } from './localeStorage';
import { ru } from './ru';
import { type Locale, type UiMessages } from './types';

const catalogs: Record<Locale, UiMessages> = { en, ru };

function applyLocale(locale: Locale, titleRu: string, titleEn: string): void {
  setApiLocale(locale);
  document.documentElement.lang = locale;
  document.title = locale === 'ru' ? titleRu : titleEn;
  writeStoredLocale(locale);
  consumeLangQuery();
}

interface LocaleContextValue {
  locale: Locale;
  t: UiMessages;
  setLocale: (locale: Locale) => void;
}

const LocaleContext = createContext<LocaleContextValue | null>(null);

export function LocaleProvider({ children }: { children: ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(() => {
    const initial = readStoredLocale();
    applyLocale(initial, 'Банк — Клиенты', 'Bank — Clients');
    return initial;
  });

  const setLocale = useCallback((next: Locale) => {
    applyLocale(next, 'Банк — Клиенты', 'Bank — Clients');
    setLocaleState(next);
  }, []);

  const value = useMemo<LocaleContextValue>(
    () => ({
      locale,
      t: catalogs[locale],
      setLocale,
    }),
    [locale, setLocale],
  );

  return <LocaleContext.Provider value={value}>{children}</LocaleContext.Provider>;
}

export function useLocale(): LocaleContextValue {
  const context = useContext(LocaleContext);
  if (!context) {
    throw new Error('useLocale must be used within LocaleProvider');
  }
  return context;
}
