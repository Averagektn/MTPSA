/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_HOME_URL?: string;
  readonly VITE_CLIENTS_URL?: string;
  readonly VITE_DEPOSITS_URL?: string;
  readonly VITE_CREDITS_URL?: string;
  readonly VITE_ATM_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}