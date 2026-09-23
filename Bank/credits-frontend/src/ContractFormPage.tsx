import { useNavigate } from 'react-router-dom';
import { ContractForm } from './ContractForm';
import { useLocale } from './i18n/LocaleContext';

export function ContractFormPage() {
  const navigate = useNavigate();
  const { t } = useLocale();

  return (
    <section className="panel" aria-labelledby="contract-form-heading">
      <div className="card">
        <div className="section-header">
          <h2 id="contract-form-heading" className="section-title">{t.newContract}</h2>
        </div>
        <ContractForm onCancel={() => navigate('/')} onSaved={() => navigate('/')} />
      </div>
    </section>
  );
}
