import { useEffect, useMemo, useRef, useState } from 'react';
import { insertCard, submitInput } from './api';
import { useLocale } from './i18n/LocaleContext';
import type { AtmDepositItem, AtmSession } from './types';
import type { UiMessages } from './i18n/types';

const DEMO_CARD = '4277000011112222';
const CARD_LENGTH = 16;
const PIN_LENGTH = 4;
const PHONE_LENGTH = 10;
const AMOUNT_LENGTH = 8;
const SUCCESS_MS = 2000;

function field(session: AtmSession, key: string): string {
  return session.fields.find((item) => item.key === key)?.value ?? '';
}

function digitSlots(value: string, length: number, options?: { mask?: boolean; group?: number }): string {
  const filled = value.replace(/\D/g, '').slice(0, length);
  const cells = Array.from({ length }, (_, index) => {
    if (index >= filled.length) {
      return '_';
    }
    return options?.mask ? '•' : filled[index];
  });
  const group = options?.group;
  if (!group) {
    return cells.join(' ');
  }
  const parts: string[] = [];
  for (let index = 0; index < cells.length; index += group) {
    parts.push(cells.slice(index, index + group).join(''));
  }
  return parts.join(' ');
}

function maxLengthFor(screen: string): number | null {
  switch (screen) {
    case 'card':
      return CARD_LENGTH;
    case 'pin':
      return PIN_LENGTH;
    case 'paymentPhone':
      return PHONE_LENGTH;
    case 'withdrawAmount':
    case 'paymentAmount':
      return AMOUNT_LENGTH;
    default:
      return null;
  }
}

function money(value?: number | null, currency = 'BYN'): string {
  if (value === null || value === undefined) {
    return '—';
  }
  return `${value.toFixed(2)} ${currency}`;
}

function message(t: UiMessages, key?: string | null): string | null {
  if (!key) {
    return null;
  }
  return (t as unknown as Record<string, string>)[key] ?? key;
}

export function AtmPage() {
  const { locale, t } = useLocale();
  const [session, setSession] = useState<AtmSession | null>(null);
  const [buffer, setBuffer] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [receiptDismissed, setReceiptDismissed] = useState(false);

  const operatorName = useMemo(() => {
    if (!session) {
      return '';
    }
    const code = field(session, 'operatorCode');
    const found = session.operators.find((item) => item.code === code);
    if (!found) {
      return code;
    }
    return locale === 'ru' ? found.nameRu : found.nameEn;
  }, [locale, session]);

  async function run(action: () => Promise<AtmSession>) {
    setBusy(true);
    setError(null);
    try {
      const next = await action();
      setSession(next);
      setBuffer('');
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : t.requestFailed);
    } finally {
      setBusy(false);
    }
  }

  const runRef = useRef(run);
  runRef.current = run;

  const screen = session?.screen ?? 'card';
  const receiptOpen = Boolean(session?.receipt) && !receiptDismissed;
  const operationSucceeded = screen === 'result' && session?.lastResult?.success === true;

  useEffect(() => {
    setReceiptDismissed(false);
  }, [session?.screen, session?.receipt?.printedAt, session?.lastResult?.messageKey]);

  useEffect(() => {
    if (!session || !operationSucceeded || receiptOpen) {
      return;
    }

    const sessionId = session.id;
    const timer = window.setTimeout(() => {
      void runRef.current(() => submitInput(sessionId, 'continue', 'ok'));
    }, SUCCESS_MS);
    return () => window.clearTimeout(timer);
  }, [operationSucceeded, receiptOpen, session]);

  function press(digit: string) {
    const max = maxLengthFor(screen);
    setBuffer((current) => {
      if (max !== null && current.length >= max) {
        return current;
      }
      return `${current}${digit}`;
    });
  }

  return (
    <section className="atm-shell" data-testid="atm-shell">
      <div className="atm-bezel">
        <div className="atm-screen" data-testid="atm-screen">
          {busy && <p className="atm-status">{t.loading}</p>}
          {(error || session?.messageKey) && screen !== 'result' && (
            <p className="atm-error" data-testid="atm-message">
              {error ?? message(t, session?.messageKey)}
            </p>
          )}

          {screen === 'card' && (
            <>
              <h2>{t.insertCard}</h2>
              <p className="muted">{t.cardHint}</p>
              <div className="atm-value" data-testid="card-input">{digitSlots(buffer, CARD_LENGTH, { group: 4 })}</div>
              <button type="button" className="btn btn-secondary" data-testid="demo-card" onClick={() => setBuffer(DEMO_CARD)}>
                {t.demoCard}
              </button>
              <button
                type="button"
                className="btn btn-primary"
                data-testid="insert-card"
                disabled={busy}
                onClick={() => run(() => insertCard(buffer))}
              >
                {t.enter}
              </button>
            </>
          )}

          {screen === 'pin' && (
            <>
              <h2>{t.pinPrompt}</h2>
              <p className="muted">{t.attemptsLeft}: {session?.pinAttemptsLeft}</p>
              <div className="atm-value" data-testid="pin-input">{digitSlots(buffer, PIN_LENGTH, { mask: true })}</div>
              <button
                type="button"
                className="btn btn-primary"
                data-testid="submit-pin"
                disabled={busy}
                onClick={() => session && run(() => submitInput(session.id, 'pin', buffer))}
              >
                {t.enter}
              </button>
            </>
          )}

          {screen === 'locked' && (
            <>
              <h2 data-testid="atm-locked">{t.cardLocked}</h2>
              <button type="button" className="btn btn-primary" data-testid="new-session" onClick={() => { setSession(null); setBuffer(''); }}>
                {t.newSession}
              </button>
            </>
          )}

          {screen === 'menu' && session && (
            <>
              <h2>{t.menuTitle}</h2>
              <p className="muted">{session.account?.clientName} · {session.cardNumberMasked}</p>
              <div className="atm-menu">
                <button type="button" className="btn btn-primary" data-testid="menu-withdraw" onClick={() => run(() => submitInput(session.id, 'menu', 'withdraw'))}>{t.withdraw}</button>
                <button type="button" className="btn btn-secondary" data-testid="menu-balance" onClick={() => run(() => submitInput(session.id, 'menu', 'balance'))}>{t.creditBalance}</button>
                <button type="button" className="btn btn-secondary" data-testid="menu-deposit" onClick={() => run(() => submitInput(session.id, 'menu', 'depositBalance'))}>{t.depositBalance}</button>
                <button type="button" className="btn btn-secondary" data-testid="menu-payment" onClick={() => run(() => submitInput(session.id, 'menu', 'payment'))}>{t.payment}</button>
                <button type="button" className="btn btn-danger" data-testid="menu-eject" onClick={() => run(() => submitInput(session.id, 'menu', 'eject'))}>{t.eject}</button>
              </div>
            </>
          )}

          {(screen === 'withdrawAmount' || screen === 'paymentAmount') && session && (
            <>
              <h2>{t.amountPrompt}</h2>
              <div className="atm-value" data-testid="amount-input">{digitSlots(buffer, AMOUNT_LENGTH)}</div>
              <button type="button" className="btn btn-primary" data-testid="submit-amount" onClick={() => run(() => submitInput(session.id, 'amount', buffer))}>{t.enter}</button>
            </>
          )}

          {screen === 'paymentOperator' && session && (
            <>
              <h2>{t.operatorPrompt}</h2>
              <div className="atm-menu">
                {session.operators.map((item) => (
                  <button
                    key={item.code}
                    type="button"
                    className="btn btn-secondary"
                    data-testid={`operator-${item.code}`}
                    onClick={() => run(() => submitInput(session.id, 'operator', item.code))}
                  >
                    {locale === 'ru' ? item.nameRu : item.nameEn}
                  </button>
                ))}
              </div>
            </>
          )}

          {screen === 'paymentPhone' && session && (
            <>
              <h2>{t.phonePrompt}</h2>
              <div className="atm-value" data-testid="phone-input">{digitSlots(buffer, PHONE_LENGTH)}</div>
              <button type="button" className="btn btn-primary" data-testid="submit-phone" onClick={() => run(() => submitInput(session.id, 'phone', buffer))}>{t.enter}</button>
            </>
          )}

          {screen === 'paymentConfirm' && session && (
            <>
              <h2>{t.confirmPayment}</h2>
              <ul className="atm-details">
                <li>{t.operator}: {operatorName}</li>
                <li>{t.phone}: {field(session, 'phone')}</li>
                <li>{t.amount}: {field(session, 'amount')} BYN</li>
              </ul>
              <div className="atm-menu">
                <button type="button" className="btn btn-primary" data-testid="confirm-payment" onClick={() => run(() => submitInput(session.id, 'confirm', 'yes'))}>{t.confirm}</button>
                <button type="button" className="btn btn-secondary" data-testid="reenter-payment" onClick={() => run(() => submitInput(session.id, 'confirm', 'no'))}>{t.reenter}</button>
              </div>
            </>
          )}

          {screen === 'receiptChoice' && session && (
            <>
              <h2>{t.printReceipt}</h2>
              <div className="atm-menu">
                <button type="button" className="btn btn-primary" data-testid="receipt-yes" onClick={() => run(() => submitInput(session.id, 'receipt', 'yes'))}>{t.yes}</button>
                <button type="button" className="btn btn-secondary" data-testid="receipt-no" onClick={() => run(() => submitInput(session.id, 'receipt', 'no'))}>{t.no}</button>
              </div>
            </>
          )}

          {screen === 'result' && session && operationSucceeded && receiptOpen && session.receipt && (
            <>
              <BalanceOnScreen session={session} t={t} />
              <ReceiptView receipt={session.receipt} deposits={session.lastResult?.deposits} t={t} />
              <div className="atm-menu">
                <button
                  type="button"
                  className="btn btn-primary"
                  data-testid="close-receipt"
                  onClick={() => setReceiptDismissed(true)}
                >
                  {t.closeReceipt}
                </button>
              </div>
            </>
          )}

          {screen === 'result' && session && operationSucceeded && !receiptOpen && (
            <>
              <h2 data-testid="atm-success">{t.operationDone}</h2>
              <BalanceOnScreen session={session} t={t} />
            </>
          )}

          {screen === 'result' && session && !operationSucceeded && (
            <>
              <h2 data-testid="atm-result">{message(t, session.messageKey ?? session.lastResult?.messageKey) ?? t.requestFailed}</h2>
              <div className="atm-menu">
                {session.account && (
                  <button type="button" className="btn btn-primary" data-testid="continue" onClick={() => run(() => submitInput(session.id, 'continue', 'ok'))}>{t.continue}</button>
                )}
                <button type="button" className="btn btn-secondary" data-testid="eject" onClick={() => run(() => submitInput(session.id, 'eject', 'ok'))}>{t.eject}</button>
              </div>
            </>
          )}
        </div>

        <div className="atm-keypad" role="group" aria-label="keypad">
          {['1', '2', '3', '4', '5', '6', '7', '8', '9', '0'].map((digit) => (
            <button key={digit} type="button" className="key" data-testid={`key-${digit}`} onClick={() => press(digit)}>
              {digit}
            </button>
          ))}
          <button type="button" className="key key-clear" data-testid="key-clear" onClick={() => setBuffer('')}>{t.clear}</button>
        </div>
      </div>
    </section>
  );
}

function BalanceOnScreen({ session, t }: { session: AtmSession; t: UiMessages }) {
  const result = session.lastResult;
  if (!result) {
    return null;
  }
  if (result.operation === 'balance' && result.availableBalance != null) {
    return (
      <p data-testid="result-balance">
        {t.balance}: {money(result.availableBalance, result.currencyCode ?? 'BYN')}
      </p>
    );
  }
  if (result.operation !== 'depositBalance') {
    return null;
  }
  return (
    <>
      {result.deposits?.map((item) => (
        <p key={item.accountNumber} data-testid="deposit-item">
          {item.productName}: {money(item.amount, item.currencyCode)} ({item.accountNumber})
        </p>
      ))}
    </>
  );
}

function ReceiptView({
  receipt,
  deposits,
  t,
}: {
  receipt: NonNullable<AtmSession['receipt']>;
  deposits?: AtmDepositItem[] | null;
  t: UiMessages;
}) {
  return (
    <article className="atm-receipt" data-testid="atm-receipt">
      <h3>{t.receiptTitle}</h3>
      <p>{message(t, receipt.titleKey)}</p>
      <p>{t.card}: {receipt.cardNumberMasked}</p>
      <p>{t.client}: {receipt.clientName}</p>
      {receipt.creditAccountNumber && <p>{t.account}: {receipt.creditAccountNumber}</p>}
      {receipt.depositAccountNumber && <p>{t.account}: {receipt.depositAccountNumber}</p>}
      {receipt.operatorCode && <p>{t.operator}: {receipt.operatorCode}</p>}
      {receipt.phone && <p>{t.phone}: {receipt.phone}</p>}
      {receipt.amount !== undefined && receipt.amount !== null && <p>{t.amount}: {money(receipt.amount)}</p>}
      {receipt.balance !== undefined && receipt.balance !== null && <p>{t.balance}: {money(receipt.balance)}</p>}
      {deposits?.map((item) => (
        <p key={item.accountNumber} data-testid="deposit-item">
          {item.productName}: {money(item.amount, item.currencyCode)} ({item.accountNumber})
        </p>
      ))}
    </article>
  );
}
