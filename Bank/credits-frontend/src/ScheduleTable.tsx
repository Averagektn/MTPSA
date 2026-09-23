import { useLocale } from './i18n/LocaleContext';
import type { PaymentScheduleItem } from './types';
import { formatMoney, isoToDisplay } from './validation';

export function ScheduleTable({ items }: { items: PaymentScheduleItem[] }) {
  const { t } = useLocale();

  if (items.length === 0) {
    return null;
  }

  return (
    <div className="table-wrap" data-testid="schedule-table">
      <table className="clients-table">
        <thead>
          <tr>
            <th>{t.period}</th>
            <th>{t.startDate}</th>
            <th>{t.principalPart}</th>
            <th>{t.interestPart}</th>
            <th>{t.payment}</th>
            <th>{t.remaining}</th>
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.period} data-testid="schedule-row">
              <td>{item.period}</td>
              <td>{isoToDisplay(item.date)}</td>
              <td>{formatMoney(item.principal)}</td>
              <td>{formatMoney(item.interest)}</td>
              <td>{formatMoney(item.payment)}</td>
              <td>{formatMoney(item.remaining)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
