import { Link, Navigate, Route, Routes } from 'react-router-dom';
import { AccountsPage } from './AccountsPage';
import { ContractDetailPage } from './ContractDetailPage';
import { ContractFormPage } from './ContractFormPage';
import { ContractsPage } from './ContractsPage';
import { useLocale } from './i18n/LocaleContext';
import { SiteMap } from './SiteMap';
import { useTheme } from './theme/ThemeContext';
import './App.css';

function App() {
  const { locale, t, setLocale } = useLocale();
  const { theme, setTheme } = useTheme();

  return (
    <div className="app-container">
      <header className="app-header">
        <div className="header-inner">
          <div className="header-titles">
            <h1 className="app-title">
              <Link to="/" className="app-title-link">{t.appTitle}</Link>
            </h1>
            <p className="app-subtitle">{t.appSubtitle}</p>
          </div>
          <div className="header-controls">
            <div className="seg-switch" role="group" aria-label={t.theme}>
              <button
                type="button"
                className={`seg-option${theme === 'dark' ? ' active' : ''}`}
                aria-pressed={theme === 'dark'}
                onClick={() => setTheme('dark')}
              >
                {t.themeDark}
              </button>
              <button
                type="button"
                className={`seg-option${theme === 'light' ? ' active' : ''}`}
                aria-pressed={theme === 'light'}
                onClick={() => setTheme('light')}
              >
                {t.themeLight}
              </button>
            </div>
            <div className="seg-switch" role="group" aria-label={t.language}>
              <button
                type="button"
                className={`seg-option${locale === 'en' ? ' active' : ''}`}
                aria-pressed={locale === 'en'}
                onClick={() => setLocale('en')}
              >
                EN
              </button>
              <button
                type="button"
                className={`seg-option${locale === 'ru' ? ' active' : ''}`}
                aria-pressed={locale === 'ru'}
                onClick={() => setLocale('ru')}
              >
                RU
              </button>
            </div>
          </div>
        </div>
      </header>

      <main className="main-content">
        <Routes>
          <Route path="/" element={<ContractsPage />} />
          <Route path="/contracts/new" element={<ContractFormPage />} />
          <Route path="/contracts/:id" element={<ContractDetailPage />} />
          <Route path="/accounts" element={<AccountsPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
      <SiteMap current="credits" />
    </div>
  );
}

export default App;
