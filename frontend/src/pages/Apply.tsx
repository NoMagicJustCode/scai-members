import { useEffect, useState, type FormEvent, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import {
  fetchPublicConfig,
  submitApplication,
  type ApplicableClass,
  type FieldErrors,
  type MemberType,
  type PublicConfig,
} from '../lib/api'
import PageHero, { Divider } from '../components/PageHero'
import { feesAreSet, formatFee } from '../lib/fees'
import { ORG } from '../lib/org'

const MOTIVATION_MAX = 1000

interface FormState {
  memberType: MemberType
  name: string
  representative: string
  dateOfBirth: string
  email: string
  postalAddress: string
  country: string
  requestedClass: ApplicableClass
  motivation: string
  sharesValues: boolean
  hasReadStatutes: boolean
  acceptsPrivacyPolicy: boolean
}

const EMPTY: FormState = {
  memberType: 'Person',
  name: '',
  representative: '',
  dateOfBirth: '',
  email: '',
  postalAddress: '',
  country: '',
  requestedClass: 'Ordinary',
  motivation: '',
  sharesValues: false,
  hasReadStatutes: false,
  acceptsPrivacyPolicy: false,
}

/** Latest birth date that is 18 today, as yyyy-mm-dd, for the date input's max. */
function latestAdultBirthDate() {
  const d = new Date()
  d.setFullYear(d.getFullYear() - 18)
  return d.toISOString().slice(0, 10)
}

const blankToUndefined = (s: string) => (s.trim() === '' ? undefined : s.trim())

export default function Apply() {
  const [config, setConfig] = useState<PublicConfig | null>(null)
  const [configFailed, setConfigFailed] = useState(false)
  const [form, setForm] = useState<FormState>(EMPTY)
  const [errors, setErrors] = useState<FieldErrors>({})
  const [formError, setFormError] = useState<{ message: string; stale: boolean } | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [done, setDone] = useState(false)

  useEffect(() => {
    fetchPublicConfig().then(setConfig).catch(() => setConfigFailed(true))
  }, [])

  const set = <K extends keyof FormState>(key: K, value: FormState[K]) =>
    setForm((f) => ({ ...f, [key]: value }))

  const isPerson = form.memberType === 'Person'

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    if (!config) return
    setSubmitting(true)
    setErrors({})
    setFormError(null)

    const result = await submitApplication({
      memberType: form.memberType,
      name: form.name.trim(),
      representative: isPerson ? undefined : blankToUndefined(form.representative),
      dateOfBirth: isPerson ? blankToUndefined(form.dateOfBirth) : undefined,
      email: form.email.trim(),
      postalAddress: blankToUndefined(form.postalAddress),
      country: blankToUndefined(form.country),
      requestedClass: form.requestedClass,
      motivation: blankToUndefined(form.motivation),
      sharesValues: form.sharesValues,
      hasReadStatutes: form.hasReadStatutes,
      acceptsPrivacyPolicy: form.acceptsPrivacyPolicy,
      statutesVersion: config.statutesVersion,
      privacyPolicyVersion: config.privacyPolicyVersion,
    })

    setSubmitting(false)
    if (result.ok) {
      setDone(true)
      window.scrollTo(0, 0)
      return
    }
    setErrors(result.fieldErrors)
    if (Object.keys(result.fieldErrors).length === 0)
      setFormError({ message: result.message, stale: result.status === 409 })
    else
      setFormError({ message: 'Please check the highlighted fields.', stale: false })
  }

  if (done) {
    return (
      <PageHero
        eyebrow="Application"
        title="Check your inbox"
        intro={
          <>
            We have sent an email to <strong>{form.email.trim()}</strong>. Please open the
            link in it to confirm your address — your application reaches the board only
            after that.
          </>
        }
        meta="The link is valid for 48 hours. If nothing arrives within a few minutes, check your spam folder."
      />
    )
  }

  return (
    <>
      <PageHero
        eyebrow="Application"
        title="Apply for membership"
        intro={`Membership is open to natural persons aged 18 or over and to organisations who share the values and goals of ${ORG.shortName} (§7(1) of the statutes). The board decides on admission.`}
      />
      <Divider />

      <section className="form-section">
      {configFailed && (
        <div className="notice notice--error">
          <p>The application form is unavailable right now. Please try again later.</p>
        </div>
      )}

      {config && (
        <form className="form" onSubmit={onSubmit} noValidate>
          <fieldset>
            <legend>I am applying as</legend>
            <div className="choice-row">
              <Choice
                type="radio"
                name="memberType"
                checked={isPerson}
                onChange={() => set('memberType', 'Person')}
                title="A person"
              />
              <Choice
                type="radio"
                name="memberType"
                checked={!isPerson}
                onChange={() => set('memberType', 'Organisation')}
                title="An organisation"
              />
            </div>
          </fieldset>

          <fieldset>
            <legend>{isPerson ? 'About you' : 'About the organisation'}</legend>

            <Field id="name" label={isPerson ? 'Full name' : 'Registered name of the organisation'} error={errors.Name}>
              <input
                id="name"
                type="text"
                autoComplete={isPerson ? 'name' : 'organization'}
                maxLength={200}
                value={form.name}
                onChange={(e) => set('name', e.target.value)}
                required
              />
            </Field>

            {isPerson ? (
              <Field
                id="dateOfBirth"
                label="Date of birth"
                hint="Members must be at least 18 years old."
                error={errors.DateOfBirth}
              >
                <input
                  id="dateOfBirth"
                  type="date"
                  autoComplete="bday"
                  max={latestAdultBirthDate()}
                  value={form.dateOfBirth}
                  onChange={(e) => set('dateOfBirth', e.target.value)}
                  required
                />
              </Field>
            ) : (
              <Field
                id="representative"
                label="Representative"
                hint="The person who represents the organisation towards the association."
                error={errors.Representative}
              >
                <input
                  id="representative"
                  type="text"
                  autoComplete="name"
                  maxLength={200}
                  value={form.representative}
                  onChange={(e) => set('representative', e.target.value)}
                  required
                />
              </Field>
            )}

            <Field
              id="email"
              label="Email address"
              hint="Invitations to the General Assembly are sent to this address (§11(3))."
              error={errors.Email}
            >
              <input
                id="email"
                type="email"
                autoComplete="email"
                maxLength={254}
                value={form.email}
                onChange={(e) => set('email', e.target.value)}
                required
              />
            </Field>

            <Field id="postalAddress" label="Postal address" optional error={errors.PostalAddress}>
              <textarea
                id="postalAddress"
                autoComplete="street-address"
                maxLength={500}
                rows={3}
                style={{ minHeight: 0 }}
                value={form.postalAddress}
                onChange={(e) => set('postalAddress', e.target.value)}
              />
            </Field>

            <Field id="country" label="Country" optional error={errors.Country}>
              <input
                id="country"
                type="text"
                autoComplete="country-name"
                maxLength={100}
                value={form.country}
                onChange={(e) => set('country', e.target.value)}
              />
            </Field>
          </fieldset>

          <fieldset>
            <legend>Membership</legend>
            <div className="choice-group">
              <Choice
                type="radio"
                name="requestedClass"
                checked={form.requestedClass === 'Ordinary'}
                onChange={() => set('requestedClass', 'Ordinary')}
                title="Ordinary member"
                description={withFee(
                  'Takes part fully in the work of the association.',
                  config,
                  config.annualFeeOrdinary,
                )}
              />
              <Choice
                type="radio"
                name="requestedClass"
                checked={form.requestedClass === 'Supporting'}
                onChange={() => set('requestedClass', 'Supporting')}
                title="Extraordinary member"
                description={withFee(
                  'Supports the association primarily through an increased membership fee.',
                  config,
                  config.annualFeeSupporting,
                )}
              />
            </div>
            {errors.RequestedClass && <div className="field__error">{errors.RequestedClass}</div>}

            <Field
              id="motivation"
              label="Why would you like to join?"
              optional
              hint={`${form.motivation.length} / ${MOTIVATION_MAX}`}
              error={errors.Motivation}
            >
              <textarea
                id="motivation"
                maxLength={MOTIVATION_MAX}
                value={form.motivation}
                onChange={(e) => set('motivation', e.target.value)}
              />
            </Field>
          </fieldset>

          <fieldset>
            <legend>Declarations</legend>
            <div className="choice-group">
              <Choice
                type="checkbox"
                checked={form.sharesValues}
                onChange={(v) => set('sharesValues', v)}
                title="I share the values and goals of the association."
                error={errors.SharesValues}
              />
              <Choice
                type="checkbox"
                checked={form.hasReadStatutes}
                onChange={(v) => set('hasReadStatutes', v)}
                title={
                  <>
                    I have read the{' '}
                    <a href={ORG.statutesUrl} target="_blank" rel="noreferrer">statutes</a>{' '}
                    <span className="choice__version">(version {config.statutesVersion})</span>.
                  </>
                }
                error={errors.HasReadStatutes}
              />
              <Choice
                type="checkbox"
                checked={form.acceptsPrivacyPolicy}
                onChange={(v) => set('acceptsPrivacyPolicy', v)}
                title={
                  <>
                    I consent to the processing of my data as described in the{' '}
                    <Link to="/privacy" target="_blank">privacy policy</Link>{' '}
                    <span className="choice__version">(version {config.privacyPolicyVersion})</span>.
                  </>
                }
                error={errors.AcceptsPrivacyPolicy}
              />
            </div>
          </fieldset>

          {formError && (
            <div className="notice notice--error" role="alert">
              <p>{formError.message}</p>
              {formError.stale && (
                <button type="button" className="cta cta--small" onClick={() => window.location.reload()}>
                  Reload the form
                </button>
              )}
            </div>
          )}

          <button type="submit" className="cta cta--primary" disabled={submitting}>
            {submitting ? 'Sending…' : 'Submit application'}
          </button>
        </form>
      )}
      </section>
    </>
  )
}

function withFee(text: string, cfg: PublicConfig, amount: number) {
  return feesAreSet(cfg) ? `${text} ${formatFee(amount, cfg.currency)} per year.` : text
}

function Field(props: {
  id: string
  label: string
  optional?: boolean
  hint?: string
  error?: string
  children: ReactNode
}) {
  return (
    <div className={props.error ? 'field field--invalid' : 'field'}>
      <label htmlFor={props.id}>
        {props.label} {props.optional && <span className="optional">(optional)</span>}
      </label>
      {props.children}
      {props.error ? (
        <div className="field__error">{props.error}</div>
      ) : (
        props.hint && <div className="field__hint">{props.hint}</div>
      )}
    </div>
  )
}

function Choice(props: {
  type: 'radio' | 'checkbox'
  name?: string
  checked: boolean
  onChange: (checked: boolean) => void
  title: ReactNode
  description?: string
  error?: string
}) {
  return (
    <div>
      <label className="choice">
        <input
          type={props.type}
          name={props.name}
          checked={props.checked}
          onChange={(e) => props.onChange(e.target.checked)}
        />
        <span>
          <span className="choice__title">{props.title}</span>
          {props.description && <span className="choice__desc">{props.description}</span>}
        </span>
      </label>
      {props.error && <div className="field__error">{props.error}</div>}
    </div>
  )
}
