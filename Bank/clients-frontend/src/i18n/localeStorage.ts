import { LOCALE_STORAGE_KEY, type Locale } from './types';

const COOKIE_MAX_AGE = 60 * 60 * 24 * 365;

function isLocale(value: string | null | undefined): value is Locale {
  return value === 'ru' || value === 'en';
}

function readQueryLocale(): Locale | null {
  try {
    const lang = new URLSearchParams(window.location.search).get('lang');
    return isLocale(lang) ? lang : null;
  } catch {
    return null;
  }
}

function readCookieLocale(): Locale | null {
  try {
    const prefix = `${LOCALE_STORAGE_KEY}=`;
    for (const part of document.cookie.split(';')) {
      const value = part.trim();
      if (value.startsWith(prefix)) {
        const locale = decodeURIComponent(value.slice(prefix.length));
        return isLocale(locale) ? locale : null;
      }
    }
  } catch {
    // ignore cookie errors
  }
  return null;
}

function readLocalStorageLocale(): Locale | null {
  try {
    const stored = localStorage.getItem(LOCALE_STORAGE_KEY);
    return isLocale(stored) ? stored : null;
  } catch {
    return null;
  }
}

export function readStoredLocale(): Locale {
  return readQueryLocale() ?? readCookieLocale() ?? readLocalStorageLocale() ?? 'en';
}

export function writeStoredLocale(locale: Locale): void {
  try {
    localStorage.setItem(LOCALE_STORAGE_KEY, locale);
  } catch {
    // ignore storage errors
  }

  try {
    document.cookie = `${LOCALE_STORAGE_KEY}=${encodeURIComponent(locale)}; Path=/; Max-Age=${COOKIE_MAX_AGE}; SameSite=Lax`;
  } catch {
    // ignore cookie errors
  }
}

export function consumeLangQuery(): void {
  try {
    const url = new URL(window.location.href);
    if (!url.searchParams.has('lang')) {
      return;
    }
    url.searchParams.delete('lang');
    const query = url.searchParams.toString();
    window.history.replaceState(null, '', `${url.pathname}${query ? `?${query}` : ''}${url.hash}`);
  } catch {
    // ignore history errors
  }
}

export function withLang(href: string, locale: Locale): string {
  try {
    const url = new URL(href, window.location.href);
    url.searchParams.set('lang', locale);
    return url.toString();
  } catch {
    const join = href.includes('?') ? '&' : '?';
    return `${href}${join}lang=${locale}`;
  }
}
