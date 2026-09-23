export type Locale = 'en' | 'ru';

export const LOCALE_STORAGE_KEY = 'bank.locale';

export interface UiMessages {
  appTitle: string;
  appSubtitle: string;
  theme: string;
  themeDark: string;
  themeLight: string;
  language: string;
  goToClients: string;
  goToDeposits: string;
  goToCredits: string;
  goToAtm: string;
}
