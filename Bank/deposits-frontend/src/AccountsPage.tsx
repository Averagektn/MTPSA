import { Fragment, useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchAccounts } from './api';
import { useLocale } from './i18n/LocaleContext';
import { translateError } from './i18n/translate';
import type { AccountReportItem } from './types';
import { formatMoney } from './validation';

const DEPOSIT_CODES = new Set(['3404', '3414', '3470', '3471']);
const CREDIT_CODES = new Set(['2410', '2420', '2471', '2472']);
const BANK_CLIENT_CODE = '00000';

type AccountKind = 'deposit' | 'credit' | 'other';

interface AccountGroup {
  key: string;
  title: string;
  accounts: AccountReportItem[];
}

function moneyCell(amount: number, byn: number, currency: string) {
  const foreign = currency.toUpperCase() !== 'BYN';
  return (
    <td>
      {formatMoney(amount)}
      {foreign && <span className="money-eq">{formatMoney(byn)} BYN</span>}
    </td>
  );
}

function clientCode(number: string): string {
  return number.length >= 9 ? number.slice(4, 9) : '';
}

function accountKind(account: AccountReportItem): AccountKind {
  if (DEPOSIT_CODES.has(account.chartCode)) {
    return 'deposit';
  }
  if (CREDIT_CODES.has(account.chartCode)) {
    return 'credit';
  }
  if (account.chartCode === '3014') {
    const name = account.chartName.toLowerCase();
    return name.includes('card') || name.includes('карт') ? 'credit' : 'deposit';
  }
  return 'other';
}

function groupAccounts(accounts: AccountReportItem[], bankTitle: string): AccountGroup[] {
  const bank: AccountReportItem[] = [];
  const clients = new Map<string, AccountReportItem[]>();

  for (const account of accounts) {
    const code = clientCode(account.number);
    if (code === BANK_CLIENT_CODE) {
      bank.push(account);
      continue;
    }
    const list = clients.get(code) ?? [];
    list.push(account);
    clients.set(code, list);
  }

  const byNumber = (left: AccountReportItem, right: AccountReportItem) =>
    left.number.localeCompare(right.number);

  const groups: AccountGroup[] = [];
  if (bank.length > 0) {
    groups.push({ key: 'bank', title: bankTitle, accounts: [...bank].sort(byNumber) });
  }

  for (const code of [...clients.keys()].sort()) {
    const items = clients.get(code) ?? [];
    const title = items.find((item) => item.name.trim())?.name ?? code;
    groups.push({ key: code, title, accounts: [...items].sort(byNumber) });
  }

  return groups;
}

export function AccountsPage() {
  const { locale, t } = useLocale();
  const navigate = useNavigate();
  const [accounts, setAccounts] = useState<AccountReportItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setAccounts(await fetchAccounts());
    } catch (err) {
      setError(err instanceof Error ? err.message : t.loadAccountsFailed);
    } finally {
      setLoading(false);
    }
  }, [t.loadAccountsFailed]);

  useEffect(() => {
    void load();
  }, [load, locale]);

  const groups = useMemo(() => groupAccounts(accounts, t.bankAccounts), [accounts, t.bankAccounts]);

  return (
    <section className="panel" aria-labelledby="accounts-heading">
      <div className="card">
        <div className="section-header">
          <h2 id="accounts-heading" className="section-title">{t.accountsReport}</h2>
          <button className="btn btn-secondary" type="button" onClick={() => navigate('/')}>
            {t.contracts}
          </button>
        </div>

        {error && (
          <div className="error-message" role="alert">
            <span>{translateError(t, error) ?? error}</span>
          </div>
        )}

        {loading && accounts.length === 0 ? (
          <p className="muted">{t.loading}</p>
        ) : (
          <div className="table-wrap">
            <table className="clients-table" data-testid="accounts-table">
              <thead>
                <tr>
                  <th>{t.accountNumber}</th>
                  <th>{t.accountName}</th>
                  <th>{t.chartCode}</th>
                  <th>{t.chartName}</th>
                  <th>{t.nature}</th>
                  <th>{t.currency}</th>
                  <th>{t.debit}</th>
                  <th>{t.credit}</th>
                  <th>{t.saldo}</th>
                </tr>
              </thead>
              <tbody>
                {groups.map((group) => (
                  <Fragment key={group.key}>
                    <tr className="account-group" data-testid="account-group">
                      <td colSpan={9}>{group.title}</td>
                    </tr>
                    {group.accounts.map((account) => {
                      const kind = accountKind(account);
                      return (
                        <tr
                          key={account.number}
                          data-testid="account-row"
                          className={kind === 'other' ? undefined : `account-${kind}`}
                        >
                          <td>{account.number}</td>
                          <td>{account.name}</td>
                          <td>{account.chartCode}</td>
                          <td>{account.chartName}</td>
                          <td>{account.nature}</td>
                          <td>{account.currencyCode}</td>
                          {moneyCell(account.debit, account.debitByn, account.currencyCode)}
                          {moneyCell(account.credit, account.creditByn, account.currencyCode)}
                          {moneyCell(account.saldo, account.saldoByn, account.currencyCode)}
                        </tr>
                      );
                    })}
                  </Fragment>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </section>
  );
}
