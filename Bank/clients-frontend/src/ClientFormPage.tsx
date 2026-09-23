import { Navigate, useNavigate, useParams } from 'react-router-dom';
import { ClientForm } from './ClientForm';

export function ClientFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const clientId = id === undefined ? null : Number(id);

  if (clientId !== null && !Number.isInteger(clientId)) {
    return <Navigate to="/clients" replace />;
  }

  const goToList = () => navigate('/clients');

  return (
    <ClientForm
      clientId={clientId}
      onCancel={goToList}
      onSaved={goToList}
    />
  );
}
