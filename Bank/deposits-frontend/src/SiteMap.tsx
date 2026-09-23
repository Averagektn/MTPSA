import { useLocale } from './i18n/LocaleContext';
import { withLang } from './i18n/localeStorage';

export type SiteSection = 'clients' | 'deposits' | 'credits' | 'atm';

const homeUrl = import.meta.env.VITE_HOME_URL || 'http://localhost:5173';
const clientsUrl = import.meta.env.VITE_CLIENTS_URL || 'http://localhost:5177';
const depositsUrl = import.meta.env.VITE_DEPOSITS_URL || 'http://localhost:5174';
const creditsUrl = import.meta.env.VITE_CREDITS_URL || 'http://localhost:5175';
const atmUrl = import.meta.env.VITE_ATM_URL || 'http://localhost:5176';

export function SiteMap({ current }: { current: SiteSection }) {
  const { locale, t } = useLocale();
  const items = [
    { id: 'home' as const, href: homeUrl, label: t.siteHome, testId: 'go-to-home' },
    { id: 'clients' as const, href: clientsUrl, label: t.siteClients, testId: 'go-to-clients' },
    { id: 'deposits' as const, href: depositsUrl, label: t.siteDeposits, testId: 'go-to-deposits' },
    { id: 'credits' as const, href: creditsUrl, label: t.siteCredits, testId: 'go-to-credits' },
    { id: 'atm' as const, href: atmUrl, label: t.siteAtm, testId: 'go-to-atm' },
  ];

  return (
    <nav className="site-map" aria-label={t.siteMap}>
      <div className="site-map-card">
        <h2 className="site-map-title">{t.siteMap}</h2>
        <ul className="site-map-links">
          {items.map((item) => (
            <li key={item.id}>
              {item.id === current ? (
                <span className="site-map-current" aria-current="page">{item.label}</span>
              ) : (
                <a className="site-map-link" href={withLang(item.href, locale)} data-testid={item.testId}>
                  {item.label}
                </a>
              )}
            </li>
          ))}
        </ul>
      </div>
    </nav>
  );
}
