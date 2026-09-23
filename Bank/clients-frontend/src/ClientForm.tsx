import { useEffect, useLayoutEffect, useRef, useState, type FormEvent, type ReactNode } from 'react';
import { createClient, fetchClient, fetchDictionaries, updateClient } from './api';
import { useLocale } from './i18n/LocaleContext';
import { translateError } from './i18n/translate';
import type { ClientPayload, Dictionaries, FieldErrors } from './types';
import {
  applyPhoneMask,
  emptyForm,
  emptyToNull,
  formatBelarusPhone,
  isoToDisplay,
  nationalPhoneDigits,
  phoneOrNull,
  validateClientForm,
  type ClientFormValues,
} from './validation';

interface ClientFormProps {
  clientId: number | null;
  onCancel: () => void;
  onSaved: () => void;
}

function withFormattedPhones(values: ClientFormValues): ClientFormValues {
  return {
    ...values,
    homePhone: formatBelarusPhone(values.homePhone),
    mobilePhone: formatBelarusPhone(values.mobilePhone),
  };
}

function toPayload(values: ClientFormValues): ClientPayload {
  const incomeRaw = values.monthlyIncome.trim().replace(',', '.');
  return {
    lastName: values.lastName.trim(),
    firstName: values.firstName.trim(),
    patronymic: values.patronymic.trim(),
    birthDate: values.birthDate.trim(),
    passportSeries: values.passportSeries.trim().toUpperCase(),
    passportNumber: values.passportNumber.trim(),
    issuedBy: values.issuedBy.trim(),
    issueDate: values.issueDate.trim(),
    identificationNumber: values.identificationNumber.trim().toUpperCase(),
    birthPlace: values.birthPlace.trim(),
    residenceCityId: Number(values.residenceCityId) || 0,
    residenceAddress: values.residenceAddress.trim(),
    registrationCityId: Number(values.registrationCityId) || 0,
    homePhone: phoneOrNull(values.homePhone),
    mobilePhone: phoneOrNull(values.mobilePhone),
    email: emptyToNull(values.email),
    workplace: emptyToNull(values.workplace),
    position: emptyToNull(values.position),
    maritalStatusId: Number(values.maritalStatusId) || 0,
    citizenshipId: Number(values.citizenshipId) || 0,
    disabilityId: Number(values.disabilityId) || 0,
    pensioner: values.pensioner,
    monthlyIncome: incomeRaw === '' ? null : Number(incomeRaw),
  };
}

export function ClientForm({ clientId, onCancel, onSaved }: ClientFormProps) {
  const { t, locale } = useLocale();
  const [dictionaries, setDictionaries] = useState<Dictionaries | null>(null);
  const [values, setValues] = useState<ClientFormValues>(emptyForm());
  const [errors, setErrors] = useState<FieldErrors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadDictionaries() {
      try {
        const dicts = await fetchDictionaries();
        if (!cancelled) {
          setDictionaries(dicts);
        }
      } catch (err) {
        if (!cancelled) {
          setFormError(err instanceof Error ? err.message : t.loadFailed);
        }
      }
    }

    void loadDictionaries();
    return () => {
      cancelled = true;
    };
  }, [locale, t.loadFailed]);

  useEffect(() => {
    let cancelled = false;

    async function loadClient() {
      setLoading(true);
      setFormError(null);
      setErrors({});
      try {
        if (clientId !== null) {
          const client = await fetchClient(clientId);
          if (cancelled) return;
          setValues({
            lastName: client.lastName,
            firstName: client.firstName,
            patronymic: client.patronymic,
            birthDate: isoToDisplay(client.birthDate),
            passportSeries: client.passportSeries,
            passportNumber: client.passportNumber,
            issuedBy: client.issuedBy,
            issueDate: isoToDisplay(client.issueDate),
            identificationNumber: client.identificationNumber,
            birthPlace: client.birthPlace,
            residenceCityId: String(client.residenceCityId),
            residenceAddress: client.residenceAddress,
            registrationCityId: String(client.registrationCityId),
            homePhone: formatBelarusPhone(client.homePhone ?? ''),
            mobilePhone: formatBelarusPhone(client.mobilePhone ?? ''),
            email: client.email ?? '',
            workplace: client.workplace ?? '',
            position: client.position ?? '',
            maritalStatusId: String(client.maritalStatusId),
            citizenshipId: String(client.citizenshipId),
            disabilityId: String(client.disabilityId),
            pensioner: client.pensioner,
            monthlyIncome: client.monthlyIncome == null ? '' : String(client.monthlyIncome),
          });
        } else {
          setValues(emptyForm());
        }
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

    void loadClient();
    return () => {
      cancelled = true;
    };
  }, [clientId, t.loadFailed]);

  const setField = <K extends keyof ClientFormValues>(key: K, value: ClientFormValues[K]) => {
    setValues((current) => ({ ...current, [key]: value }));
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setFormError(null);

    const next = withFormattedPhones(values);
    setValues(next);

    const clientErrors = validateClientForm(next);
    if (Object.keys(clientErrors).length > 0) {
      setErrors(clientErrors);
      return;
    }

    setErrors({});
    setSaving(true);
    try {
      const payload = toPayload(next);
      const result = clientId === null
        ? await createClient(payload)
        : await updateClient(clientId, payload);

      if (!result.ok) {
        setErrors(result.fieldErrors);
        if (Object.keys(result.fieldErrors).length === 0) {
          setFormError(result.message ?? t.saveFailed);
        }
        return;
      }

      onSaved();
    } catch (err) {
      setFormError(err instanceof Error ? err.message : t.saveFailed);
    } finally {
      setSaving(false);
    }
  };

  const title = clientId === null ? t.addClient : t.editClient;

  return (
    <section className="panel" aria-labelledby="form-heading">
      <div className="card">
        <div className="section-header">
          <h2 id="form-heading" className="section-title">{title}</h2>
          <button className="btn btn-secondary" type="button" onClick={onCancel}>
            {t.backToList}
          </button>
        </div>

        {formError && (
          <div className="error-message" role="alert">
            <span>{formError}</span>
          </div>
        )}

        {loading || !dictionaries ? (
          <p className="muted">{t.loading}</p>
        ) : (
          <form className="client-form" onSubmit={handleSubmit} noValidate data-testid="client-form">
            <fieldset>
              <legend>{t.personalData}</legend>
              <div className="form-grid">
                <Field label={t.lastName} required error={errors.lastName}>
                  <input
                    name="lastName"
                    value={values.lastName}
                    onChange={(e) => setField('lastName', e.target.value)}
                    autoComplete="family-name"
                  />
                </Field>
                <Field label={t.firstName} required error={errors.firstName}>
                  <input
                    name="firstName"
                    value={values.firstName}
                    onChange={(e) => setField('firstName', e.target.value)}
                    autoComplete="given-name"
                  />
                </Field>
                <Field label={t.patronymic} required error={errors.patronymic}>
                  <input
                    name="patronymic"
                    value={values.patronymic}
                    onChange={(e) => setField('patronymic', e.target.value)}
                    autoComplete="additional-name"
                  />
                </Field>
                <Field label={t.dateOfBirth} required hint={t.dateHint} error={errors.birthDate}>
                  <input
                    name="birthDate"
                    value={values.birthDate}
                    onChange={(e) => setField('birthDate', e.target.value)}
                    placeholder="15.05.2003"
                    inputMode="numeric"
                  />
                </Field>
                <Field label={t.placeOfBirth} required error={errors.birthPlace}>
                  <input
                    name="birthPlace"
                    value={values.birthPlace}
                    onChange={(e) => setField('birthPlace', e.target.value)}
                  />
                </Field>
              </div>
            </fieldset>

            <fieldset>
              <legend>{t.passportSection}</legend>
              <div className="form-grid">
                <Field label={t.passportSeries} required hint="AB" error={errors.passportSeries}>
                  <input
                    name="passportSeries"
                    value={values.passportSeries}
                    onChange={(e) => setField('passportSeries', e.target.value)}
                    maxLength={2}
                    placeholder="AB"
                    pattern="[A-Za-z]{2}"
                  />
                </Field>
                <Field label={t.passportNumber} required hint={t.passportNumberHint} error={errors.passportNumber}>
                  <input
                    name="passportNumber"
                    value={values.passportNumber}
                    onChange={(e) => setField('passportNumber', e.target.value)}
                    maxLength={7}
                    placeholder="1234567"
                    inputMode="numeric"
                    pattern="\d{7}"
                  />
                </Field>
                <Field label={t.issuedBy} required error={errors.issuedBy}>
                  <input
                    name="issuedBy"
                    value={values.issuedBy}
                    onChange={(e) => setField('issuedBy', e.target.value)}
                  />
                </Field>
                <Field label={t.issueDate} required hint={t.dateHint} error={errors.issueDate}>
                  <input
                    name="issueDate"
                    value={values.issueDate}
                    onChange={(e) => setField('issueDate', e.target.value)}
                    placeholder="20.06.2019"
                    inputMode="numeric"
                  />
                </Field>
                <Field
                  label={t.identificationNumber}
                  required
                  hint="#######X###XX#"
                  error={errors.identificationNumber}
                >
                  <input
                    name="identificationNumber"
                    value={values.identificationNumber}
                    onChange={(e) => setField('identificationNumber', e.target.value)}
                    maxLength={14}
                    placeholder="1505033A015PB7"
                    pattern="\d{7}[A-Za-z]\d{3}[A-Za-z]{2}\d"
                  />
                </Field>
              </div>
            </fieldset>

            <fieldset>
              <legend>{t.address}</legend>
              <div className="form-grid">
                <Field label={t.cityOfResidence} required error={errors.residenceCityId}>
                  <select
                    name="residenceCityId"
                    value={values.residenceCityId}
                    onChange={(e) => setField('residenceCityId', e.target.value)}
                  >
                    <option value="">{t.select}</option>
                    {dictionaries.cities.map((city) => (
                      <option key={city.id} value={city.id}>{city.name}</option>
                    ))}
                  </select>
                </Field>
                <Field label={t.residenceAddress} required error={errors.residenceAddress}>
                  <input
                    name="residenceAddress"
                    value={values.residenceAddress}
                    onChange={(e) => setField('residenceAddress', e.target.value)}
                  />
                </Field>
                <Field label={t.cityOfRegistration} required error={errors.registrationCityId}>
                  <select
                    name="registrationCityId"
                    value={values.registrationCityId}
                    onChange={(e) => setField('registrationCityId', e.target.value)}
                  >
                    <option value="">{t.select}</option>
                    {dictionaries.cities.map((city) => (
                      <option key={city.id} value={city.id}>{city.name}</option>
                    ))}
                  </select>
                </Field>
              </div>
            </fieldset>

            <fieldset>
              <legend>{t.contactsAndWork}</legend>
              <div className="form-grid">
                <Field label={t.homePhone} hint="+375 (17) XXX-XX-XX" error={errors.homePhone}>
                  <PhoneInput
                    name="homePhone"
                    value={values.homePhone}
                    onValue={(next) => setField('homePhone', next)}
                  />
                </Field>
                <Field label={t.mobilePhone} hint="+375 (29) XXX-XX-XX" error={errors.mobilePhone}>
                  <PhoneInput
                    name="mobilePhone"
                    value={values.mobilePhone}
                    onValue={(next) => setField('mobilePhone', next)}
                  />
                </Field>
                <Field label={t.email} error={errors.email}>
                  <input
                    name="email"
                    type="email"
                    value={values.email}
                    onChange={(e) => setField('email', e.target.value)}
                    placeholder="name@example.com"
                  />
                </Field>
                <Field label={t.workplace} error={errors.workplace}>
                  <input
                    name="workplace"
                    value={values.workplace}
                    onChange={(e) => setField('workplace', e.target.value)}
                  />
                </Field>
                <Field label={t.position} error={errors.position}>
                  <input
                    name="position"
                    value={values.position}
                    onChange={(e) => setField('position', e.target.value)}
                  />
                </Field>
                <Field label={t.monthlyIncome} error={errors.monthlyIncome}>
                  <input
                    name="monthlyIncome"
                    value={values.monthlyIncome}
                    onChange={(e) => setField('monthlyIncome', e.target.value)}
                    inputMode="decimal"
                    placeholder="0"
                  />
                </Field>
              </div>
            </fieldset>

            <fieldset>
              <legend>{t.other}</legend>
              <div className="form-grid">
                <Field label={t.maritalStatus} required error={errors.maritalStatusId}>
                  <select
                    name="maritalStatusId"
                    value={values.maritalStatusId}
                    onChange={(e) => setField('maritalStatusId', e.target.value)}
                  >
                    <option value="">{t.select}</option>
                    {dictionaries.maritalStatuses.map((item) => (
                      <option key={item.id} value={item.id}>{item.name}</option>
                    ))}
                  </select>
                </Field>
                <Field label={t.citizenship} required error={errors.citizenshipId}>
                  <select
                    name="citizenshipId"
                    value={values.citizenshipId}
                    onChange={(e) => setField('citizenshipId', e.target.value)}
                  >
                    <option value="">{t.select}</option>
                    {dictionaries.citizenships.map((item) => (
                      <option key={item.id} value={item.id}>{item.name}</option>
                    ))}
                  </select>
                </Field>
                <Field label={t.disability} required error={errors.disabilityId}>
                  <select
                    name="disabilityId"
                    value={values.disabilityId}
                    onChange={(e) => setField('disabilityId', e.target.value)}
                  >
                    <option value="">{t.select}</option>
                    {dictionaries.disabilities.map((item) => (
                      <option key={item.id} value={item.id}>{item.name}</option>
                    ))}
                  </select>
                </Field>
                <label className="checkbox-field">
                  <input
                    name="pensioner"
                    type="checkbox"
                    checked={values.pensioner}
                    onChange={(e) => setField('pensioner', e.target.checked)}
                  />
                  {t.pensioner}
                </label>
              </div>
            </fieldset>

            <div className="form-actions">
              <button className="btn btn-secondary" type="button" onClick={onCancel} disabled={saving}>
                {t.cancel}
              </button>
              <button className="btn btn-primary" type="submit" disabled={saving} data-testid="save-client">
                {saving ? t.saving : t.save}
              </button>
            </div>
          </form>
        )}
      </div>
    </section>
  );
}

function PhoneInput({ name, value, onValue }: { name: string; value: string; onValue: (next: string) => void }) {
  const ref = useRef<HTMLInputElement>(null);

  useLayoutEffect(() => {
    const input = ref.current;
    if (!input || document.activeElement !== input) {
      return;
    }

    const pos = input.value.length;
    input.setSelectionRange(pos, pos);
  }, [value]);

  return (
    <input
      ref={ref}
      name={name}
      value={value}
      inputMode="tel"
      autoComplete="tel"
      onFocus={(event) => {
        const pos = event.target.value.length;
        event.target.setSelectionRange(pos, pos);
      }}
      onKeyDown={(event) => {
        if (event.key === 'Backspace' && nationalPhoneDigits(value).length === 0) {
          event.preventDefault();
        }
      }}
      onChange={(event) => {
        const masked = applyPhoneMask(value, event.target.value);
        if (masked === value) {
          event.target.value = value;
          event.target.setSelectionRange(value.length, value.length);
          return;
        }

        onValue(masked);
      }}
    />
  );
}

interface FieldProps {
  label: string;
  required?: boolean;
  hint?: string;
  error?: string;
  children: ReactNode;
}

function Field({ label, required, hint, error, children }: FieldProps) {
  const { t } = useLocale();
  const errorText = translateError(t, error);

  return (
    <label className={`field${errorText ? ' field-invalid' : ''}`}>
      <span className="field-label">
        {label}
        {required ? <span className="required"> *</span> : null}
        {hint ? <span className="field-hint"> {hint}</span> : null}
      </span>
      {children}
      {errorText ? <span className="field-error">{errorText}</span> : null}
    </label>
  );
}
