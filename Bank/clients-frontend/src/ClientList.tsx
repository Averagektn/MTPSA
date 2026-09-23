import { useLocale } from './i18n/LocaleContext';
import type { ClientListItem } from './types';
import { isoToDisplay } from './validation';

interface ClientListProps {
  clients: ClientListItem[];
  loading: boolean;
  error: string | null;
  onAdd: () => void;
  onEdit: (id: number) => void;
  onDelete: (id: number) => void;
}

function fullName(client: ClientListItem): string {
  return `${client.lastName} ${client.firstName} ${client.patronymic}`;
}

export function ClientList({ clients, loading, error, onAdd, onEdit, onDelete }: ClientListProps) {
  const { t } = useLocale();

  return (
    <section className="panel" aria-labelledby="clients-heading">
      <div className="card">
        <div className="section-header">
          <h2 id="clients-heading" className="section-title">{t.clients}</h2>
          <button className="btn btn-primary" type="button" onClick={onAdd}>
            {t.addClient}
          </button>
        </div>

        {error && (
          <div className="error-message" role="alert">
            <span>{error}</span>
          </div>
        )}

        {loading && clients.length === 0 ? (
          <p className="muted">{t.loadingClients}</p>
        ) : clients.length === 0 ? (
          <p className="muted">{t.noClients}</p>
        ) : (
          <div className="table-wrap">
            <table className="clients-table">
              <thead>
                <tr>
                  <th>{t.fullName}</th>
                  <th>{t.dateOfBirth}</th>
                  <th>{t.passport}</th>
                  <th>{t.idNumber}</th>
                  <th>{t.city}</th>
                  <th>{t.mobile}</th>
                  <th aria-label={t.actions} />
                </tr>
              </thead>
              <tbody>
                {clients.map((client) => (
                  <tr key={client.id}>
                    <td>{fullName(client)}</td>
                    <td>{isoToDisplay(client.birthDate)}</td>
                    <td>
                      {client.passportSeries} {client.passportNumber}
                    </td>
                    <td>{client.identificationNumber}</td>
                    <td>{client.residenceCity}</td>
                    <td>{client.mobilePhone ?? '—'}</td>
                    <td className="actions">
                      <button className="btn btn-secondary" type="button" onClick={() => onEdit(client.id)}>
                        {t.edit}
                      </button>
                      <button className="btn btn-danger" type="button" onClick={() => onDelete(client.id)}>
                        {t.delete}
                      </button>
                    </td>
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
