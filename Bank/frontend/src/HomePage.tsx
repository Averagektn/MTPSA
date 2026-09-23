import { useLocale } from './i18n/LocaleContext';
import { withLang } from './i18n/localeStorage';

const clientsUrl = import.meta.env.VITE_CLIENTS_URL || 'http://localhost:5177';
const depositsUrl = import.meta.env.VITE_DEPOSITS_URL || 'http://localhost:5174';
const creditsUrl = import.meta.env.VITE_CREDITS_URL || 'http://localhost:5175';
const atmUrl = import.meta.env.VITE_ATM_URL || 'http://localhost:5176';

export function HomePage() {
  const { locale, t } = useLocale();

  return (
    <section className="panel home-panel">
      <div className="card home-card">
        <h2 className="section-title">{t.appTitle}</h2>
        <p className="muted">{t.appSubtitle}</p>
        <div className="home-actions">
          <a className="btn btn-primary home-cta" href={withLang(clientsUrl, locale)} data-testid="go-to-clients">
            {t.goToClients}
          </a>
          <a className="btn btn-primary home-cta" href={withLang(depositsUrl, locale)} data-testid="go-to-deposits">
            {t.goToDeposits}
          </a>
          <a className="btn btn-primary home-cta" href={withLang(creditsUrl, locale)} data-testid="go-to-credits">
            {t.goToCredits}
          </a>
          <a className="btn btn-primary home-cta" href={withLang(atmUrl, locale)} data-testid="go-to-atm">
            {t.goToAtm}
          </a>
        </div>
      </div>
    </section>
  );
}
