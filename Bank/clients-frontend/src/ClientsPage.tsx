import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { deleteClient, fetchClients } from './api';
import { ClientList } from './ClientList';
import { useLocale } from './i18n/LocaleContext';
import type { ClientListItem } from './types';

export function ClientsPage() {
  const { locale, t } = useLocale();
  const navigate = useNavigate();
  const [clients, setClients] = useState<ClientListItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadClients = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setClients(await fetchClients());
    } catch (err) {
      setError(err instanceof Error ? err.message : t.loadClientsFailed);
    } finally {
      setLoading(false);
    }
  }, [t.loadClientsFailed]);

  useEffect(() => {
    void loadClients();
  }, [loadClients, locale]);

  const handleDelete = async (id: number) => {
    if (!window.confirm(t.deleteConfirm)) {
      return;
    }

    try {
      await deleteClient(id);
      await loadClients();
    } catch (err) {
      setError(err instanceof Error ? err.message : t.deleteFailed);
    }
  };

  return (
    <ClientList
      clients={clients}
      loading={loading}
      error={error}
      onAdd={() => navigate('/clients/new')}
      onEdit={(id) => navigate(`/clients/${id}/edit`)}
      onDelete={handleDelete}
    />
  );
}
