import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { closeBankingDay, fetchBankingDay, fetchContracts } from './api';
import { useLocale } from './i18n/LocaleContext';
import { translateError } from './i18n/translate';
import type { BankingDay, CreditContractListItem } from './types';
import { formatMoney, isoToDisplay } from './validation';

const MAX_CLOSE_DAYS = 400;

export function ContractsPage() {
  const { locale, t } = useLocale();
  const navigate = useNavigate();
  const [contracts, setContracts] = useState<CreditContractListItem[]>([]);
  const [day, setDay] = useState<BankingDay | null>(null);
  const [daysToClose, setDaysToClose] = useState(1);
  const [daysDraft, setDaysDraft] = useState('1');
  const [loading, setLoading] = useState(false);
  const [closing, setClosing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [items, bankingDay] = await Promise.all([fetchContracts(), fetchBankingDay()]);
      setContracts(items);
      setDay(bankingDay);
    } catch (err) {
      setError(err instanceof Error ? err.message : t.loadFailed);
    } finally {
      setLoading(false);
    }
  }, [t.loadFailed]);

  useEffect(() => {
    void load();
  }, [load, locale]);

  const handleCloseDay = async () => {
    setClosing(true);
    setError(null);
    try {
      const next = await closeBankingDay(daysToClose);
      setDay(next);
      setContracts(await fetchContracts());
    } catch (err) {
      setError(err instanceof Error ? err.message : t.closeDayFailed);
    } finally {
      setClosing(false);
    }
  };

  const setDays = (value: number) => {
    const next = Math.min(MAX_CLOSE_DAYS, Math.max(1, Math.round(value)));
    setDaysToClose(next);
    setDaysDraft(String(next));
  };

  const repaymentLabel = (value: string) => (value === 'interestOnly' ? t.interestOnly : t.annuity);

  return (
    <section className="panel" aria-labelledby="contracts-heading">
      <div className="card">
        <div className="section-header">
          <div>
            <h2 id="contracts-heading" className="section-title">{t.contracts}</h2>
            <p className="muted" data-testid="bank-date">
              {t.bankDate}: {day ? isoToDisplay(day.currentDate) : '—'}
            </p>
          </div>
          <div className="actions">
            <button className="btn btn-secondary" type="button" onClick={() => navigate('/accounts')}>
              {t.accountsReport}
            </button>
            <button className="btn btn-primary" type="button" onClick={() => navigate('/contracts/new')}>
              {t.newContract}
            </button>
          </div>
        </div>

        <div className="day-slider">
          <label className="field-label" htmlFor="daysToClose">
            {t.daysToClose}: <strong data-testid="days-to-close-value">{daysToClose}</strong>
          </label>
          <div className="day-slider-row">
            <input
              id="daysToClose"
              name="daysToClose"
              type="range"
              min={1}
              max={MAX_CLOSE_DAYS}
              step={1}
              value={daysToClose}
              onChange={(event) => setDays(Number(event.target.value))}
              disabled={closing}
              data-testid="close-days"
            />
            <input
              id="daysToCloseNumber"
              name="daysToCloseNumber"
              type="number"
              min={1}
              max={MAX_CLOSE_DAYS}
              step={1}
              value={daysDraft}
              onChange={(event) => {
                const raw = event.target.value;
                setDaysDraft(raw);
                const parsed = Number(raw);
                if (Number.isInteger(parsed) && parsed >= 1 && parsed <= MAX_CLOSE_DAYS) {
                  setDaysToClose(parsed);
                }
              }}
              onBlur={() => {
                const parsed = Number(daysDraft);
                if (!Number.isFinite(parsed)) {
                  setDaysDraft(String(daysToClose));
                  return;
                }
                setDays(parsed);
              }}
              disabled={closing}
              data-testid="close-days-input"
            />
          </div>
          <div className="day-slider-marks muted">
            <span>1</span>
            <span>30</span>
            <span>400</span>
          </div>
          <button
            className="btn btn-secondary"
            type="button"
            onClick={() => void handleCloseDay()}
            disabled={closing}
            data-testid="close-day"
          >
            {closing ? t.closingDay : t.closeBankingDay}
          </button>
        </div>

        {error && (
          <div className="error-message" role="alert">
            <span>{translateError(t, error) ?? error}</span>
          </div>
        )}

        {loading && contracts.length === 0 ? (
          <p className="muted">{t.loadingContracts}</p>
        ) : contracts.length === 0 ? (
          <p className="muted">{t.noContracts}</p>
        ) : (
          <div className="table-wrap">
            <table className="clients-table">
              <thead>
                <tr>
                  <th>{t.contractNumber}</th>
                  <th>{t.client}</th>
                  <th>{t.product}</th>
                  <th>{t.repayment}</th>
                  <th>{t.amount}</th>
                  <th>{t.remainingPrincipal}</th>
                  <th>{t.annualRate}</th>
                  <th>{t.startDate}</th>
                  <th>{t.endDate}</th>
                  <th>{t.nextPayment}</th>
                  <th>{t.status}</th>
                  <th>{t.principalAccount}</th>
                  <th>{t.interestAccount}</th>
                </tr>
              </thead>
              <tbody>
                {contracts.map((contract) => (
                  <tr
                    key={contract.id}
                    data-testid="contract-row"
                    className="clickable-row"
                    onClick={() => navigate(`/contracts/${contract.id}`)}
                  >
                    <td>{contract.number}</td>
                    <td>{contract.clientName}</td>
                    <td>{contract.productName}</td>
                    <td>{repaymentLabel(contract.repaymentSchedule)}</td>
                    <td>{formatMoney(contract.amount)} {contract.currencyCode}</td>
                    <td>{formatMoney(contract.remainingPrincipal)}</td>
                    <td>{contract.annualRate}</td>
                    <td>{isoToDisplay(contract.startDate)}</td>
                    <td>{isoToDisplay(contract.endDate)}</td>
                    <td>{contract.nextPaymentDate ? isoToDisplay(contract.nextPaymentDate) : '—'}</td>
                    <td>{contract.status === 'closed' ? t.closed : t.active}</td>
                    <td>{contract.principalAccountNumber}</td>
                    <td>{contract.interestAccountNumber}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </section>
  );
}
