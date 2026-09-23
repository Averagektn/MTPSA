import { useEffect, useMemo, useState, type FormEvent, type ReactNode } from 'react';
import { createContract, fetchBankingDay, fetchDepositDictionaries } from './api';
import { useLocale } from './i18n/LocaleContext';
import { translateError } from './i18n/translate';
import type { ClientListItem, DepositDictionaries, DepositProduct, FieldErrors } from './types';
import {
  applyProduct,
  emptyContractForm,
  validateContractForm,
  type ContractFormValues,
} from './validation';

interface ContractFormProps {
  onCancel: () => void;
  onSaved: () => void;
}

function Field({
  name,
  label,
  hint,
  error,
  children,
}: {
  name: string;
  label: string;
  hint?: string;
  error?: string;
  children: ReactNode;
}) {
  return (
    <div className={`field${error ? ' field-invalid' : ''}`}>
      <label className="field-label" htmlFor={name}>
        <span className="field-label-text">
          {label} <span className="required">*</span>
        </span>
        <span className="field-hint">{hint ?? '\u00a0'}</span>
      </label>
      {children}
      {error ? <span className="field-error" data-testid={`error-${name}`}>{error}</span> : null}
    </div>
  );
}

export function ContractForm({ onCancel, onSaved }: ContractFormProps) {
  const { t, locale } = useLocale();
  const [dictionaries, setDictionaries] = useState<DepositDictionaries | null>(null);
  const [clients, setClients] = useState<ClientListItem[]>([]);
  const [values, setValues] = useState<ContractFormValues>(emptyContractForm(''));
  const [errors, setErrors] = useState<FieldErrors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setLoading(true);
      setFormError(null);
      try {
        const [dicts, day] = await Promise.all([
          fetchDepositDictionaries(),
          fetchBankingDay(),
        ]);
        if (cancelled) {
          return;
        }
        setDictionaries(dicts);
        setClients(dicts.clients);
        setValues(emptyContractForm(day.currentDate));
      } catch (err) {
        if (!cancelled) {
          setFormError(err instanceof Error ? err.message : t.loadFailed);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    void load();
    return () => {
      cancelled = true;
    };
  }, [locale, t.loadFailed]);

  const selectedProduct = useMemo<DepositProduct | undefined>(
    () => dictionaries?.products.find((p) => String(p.id) === values.productId),
    [dictionaries, values.productId],
  );

  const setField = (name: keyof ContractFormValues, value: string) => {
    setValues((current) => {
      const next = { ...current, [name]: value };
      if (name === 'productId' && dictionaries) {
        const product = dictionaries.products.find((p) => String(p.id) === value);
        return product ? applyProduct(next, product) : next;
      }
      if (name === 'startDate' && selectedProduct) {
        return applyProduct({ ...next, startDate: value }, selectedProduct);
      }
      return next;
    });
  };

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();
    const nextErrors = validateContractForm(values, selectedProduct);
    setErrors(nextErrors);
    if (Object.keys(nextErrors).length > 0) {
      return;
    }

    setSaving(true);
    setFormError(null);
    const result = await createContract({
      clientId: Number(values.clientId),
      productId: Number(values.productId),
      number: values.number.trim(),
      currencyId: Number(values.currencyId),
      startDate: values.startDate.trim(),
      endDate: values.endDate.trim(),
      termMonths: Number(values.termMonths),
      amount: Number(values.amount.trim().replace(',', '.')),
      annualRate: Number(values.annualRate.trim().replace(',', '.')),
    });
    setSaving(false);

    if (result.ok) {
      onSaved();
      return;
    }

    setErrors(result.fieldErrors);
    setFormError(result.message ? (translateError(t, result.message) ?? result.message) : t.saveFailed);
  };

  if (loading) {
    return <p className="muted">{t.loading}</p>;
  }

  return (
    <form className="client-form" onSubmit={(event) => void onSubmit(event)} noValidate>
      {formError && (
        <div className="error-message" role="alert">
          <span>{formError}</span>
        </div>
      )}

      <fieldset>
        <legend>{t.newContract}</legend>
        <div className="form-grid">
          <Field name="clientId" label={t.client} error={translateError(t, errors.clientId)}>
            <select
              id="clientId"
              name="clientId"
              value={values.clientId}
              onChange={(e) => setField('clientId', e.target.value)}
            >
              <option value="">{t.select}</option>
              {clients.map((client) => (
                <option key={client.id} value={client.id}>
                  {client.lastName} {client.firstName} {client.patronymic}
                </option>
              ))}
            </select>
          </Field>
          <Field name="productId" label={t.product} error={translateError(t, errors.productId)}>
            <select
              id="productId"
              name="productId"
              value={values.productId}
              onChange={(e) => setField('productId', e.target.value)}
            >
              <option value="">{t.select}</option>
              {dictionaries?.products.map((product) => (
                <option key={product.id} value={product.id}>
                  {product.name}
                </option>
              ))}
            </select>
          </Field>
          <Field
            name="number"
            label={t.contractNumber}
            hint={t.contractNumberHint}
            error={translateError(t, errors.number)}
          >
            <input
              id="number"
              name="number"
              value={values.number}
              onChange={(e) => setField('number', e.target.value)}
            />
          </Field>
          <Field name="currencyId" label={t.currency} error={translateError(t, errors.currencyId)}>
            <select
              id="currencyId"
              name="currencyId"
              value={values.currencyId}
              onChange={(e) => setField('currencyId', e.target.value)}
            >
              <option value="">{t.select}</option>
              {dictionaries?.currencies.map((currency) => (
                <option key={currency.id} value={currency.id}>
                  {currency.name}
                </option>
              ))}
            </select>
          </Field>
          <Field name="startDate" label={t.startDate} hint={t.dateHint} error={translateError(t, errors.startDate)}>
            <input
              id="startDate"
              name="startDate"
              value={values.startDate}
              onChange={(e) => setField('startDate', e.target.value)}
            />
          </Field>
          <Field name="endDate" label={t.endDate} hint={t.dateHint} error={translateError(t, errors.endDate)}>
            <input
              id="endDate"
              name="endDate"
              value={values.endDate}
              onChange={(e) => setField('endDate', e.target.value)}
            />
          </Field>
          <Field name="termMonths" label={t.termMonths} error={translateError(t, errors.termMonths)}>
            <input
              id="termMonths"
              name="termMonths"
              value={values.termMonths}
              onChange={(e) => setField('termMonths', e.target.value)}
            />
          </Field>
          <Field name="amount" label={t.amount} error={translateError(t, errors.amount)}>
            <input
              id="amount"
              name="amount"
              value={values.amount}
              onChange={(e) => setField('amount', e.target.value)}
            />
          </Field>
          <Field name="annualRate" label={t.annualRate} error={translateError(t, errors.annualRate)}>
            <input
              id="annualRate"
              name="annualRate"
              value={values.annualRate}
              readOnly
            />
          </Field>
        </div>
      </fieldset>

      <div className="form-actions">
        <button className="btn btn-secondary" type="button" onClick={onCancel}>
          {t.cancel}
        </button>
        <button className="btn btn-primary" type="submit" disabled={saving} data-testid="save-contract">
          {saving ? t.saving : t.save}
        </button>
      </div>
    </form>
  );
}
