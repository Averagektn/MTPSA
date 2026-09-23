import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { fetchContract } from './api';
import { useLocale } from './i18n/LocaleContext';
import { translateError } from './i18n/translate';
import { ScheduleTable } from './ScheduleTable';
import type { CreditContractDetail } from './types';
import { formatMoney, isoToDisplay } from './validation';

export function ContractDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { locale, t } = useLocale();
  const [contract, setContract] = useState<CreditContractDetail | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function load() {
      setError(null);
      try {
        const item = await fetchContract(Number(id));
        if (!cancelled) {
          setContract(item);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : t.loadFailed);
        }
      }
    }
    void load();
    return () => {
      cancelled = true;
    };
  }, [id, locale, t.loadFailed]);

  return (
    <section className="panel" aria-labelledby="contract-detail-heading">
      <div className="card">
        <div className="section-header">
          <h2 id="contract-detail-heading" className="section-title">{t.schedule}</h2>
          <button className="btn btn-secondary" type="button" onClick={() => navigate('/')}>
            {t.contracts}
          </button>
        </div>

        {error && (
          <div className="error-message" role="alert">
            <span>{translateError(t, error) ?? error}</span>
          </div>
        )}

        {!contract && !error ? <p className="muted">{t.loading}</p> : null}

        {contract ? (
          <>
            <p className="muted" data-testid="contract-detail-number">
              {contract.number} — {contract.clientName} — {formatMoney(contract.amount)} {contract.currencyCode}
            </p>
            <p className="muted">
              {t.startDate}: {isoToDisplay(contract.startDate)}; {t.endDate}: {isoToDisplay(contract.endDate)}; {t.remainingPrincipal}: {formatMoney(contract.remainingPrincipal)}
            </p>
            <ScheduleTable items={contract.schedule} />
          </>
        ) : null}
      </div>
    </section>
  );
}
