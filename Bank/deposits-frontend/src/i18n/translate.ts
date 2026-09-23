import type { UiMessages } from './types';

export function translateError(t: UiMessages, key: string | undefined): string | undefined {
  if (!key) {
    return undefined;
  }

  return key in t ? t[key as keyof UiMessages] : key;
}
