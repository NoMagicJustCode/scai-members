import { useEffect, useState, type FormEvent } from 'react'
import { fetchSettings, saveSettings, type FieldErrors, type PlatformSettings } from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { formatDate } from '../../lib/format'

type FeeKey = 'joiningFee' | 'annualFeeOrdinary' | 'annualFeeSupporting'
const FEE_KEYS: FeeKey[] = ['joiningFee', 'annualFeeOrdinary', 'annualFeeSupporting']

/** Fees are decided by the General Assembly (§12(f)); the board records them here. */
export default function Settings() {
  const { expire } = useAuth()
  const [form, setForm] = useState<PlatformSettings | null>(null)
  // Fee inputs keep the typed text ("36." mid-typing) and are parsed on save.
  const [fees, setFees] = useState<Record<FeeKey, string>>({ joiningFee: '', annualFeeOrdinary: '', annualFeeSupporting: '' })
  const [errors, setErrors] = useState<FieldErrors>({})
  const [message, setMessage] = useState<{ text: string; error: boolean } | null>(null)
  const [busy, setBusy] = useState(false)

  const load = (s: PlatformSettings) => {
    setForm(s)
    setFees({
      joiningFee: String(s.joiningFee),
      annualFeeOrdinary: String(s.annualFeeOrdinary),
      annualFeeSupporting: String(s.annualFeeSupporting),
    })
  }

  useEffect(() => {
    fetchSettings().then((r) => {
      if (r.ok) load(r.data)
      else if (r.status === 401) expire()
      else setMessage({ text: r.message, error: true })
    })
  }, [expire])

  if (!form) return message ? <div className="notice notice--error"><p>{message.text}</p></div> : <p className="admin-muted">Loading…</p>

  const set = <K extends keyof PlatformSettings>(key: K, value: PlatformSettings[K]) =>
    setForm((f) => (f ? { ...f, [key]: value } : f))

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    if (!form) return
    setErrors({})
    setMessage(null)

    const parsed = { ...form }
    const bad: FieldErrors = {}
    for (const key of FEE_KEYS) {
      const text = fees[key].trim().replace(',', '.')
      const value = text === '' ? 0 : Number(text)
      if (!Number.isFinite(value) || value < 0) bad[key.charAt(0).toUpperCase() + key.slice(1)] = 'Enter an amount, e.g. 36 or 36.50.'
      else parsed[key] = value
    }
    if (Object.keys(bad).length > 0) {
      setErrors(bad)
      setMessage({ text: 'Please check the highlighted amounts.', error: true })
      return
    }

    setBusy(true)
    const r = await saveSettings(parsed)
    setBusy(false)
    if (r.ok) {
      load(r.data)
      setMessage({ text: 'Saved. The public pages show the new values immediately.', error: false })
    } else if (r.status === 401) expire()
    else {
      setErrors(r.fieldErrors)
      setMessage({ text: r.message, error: true })
    }
  }

  const money = (key: FeeKey, label: string) => {
    const error = errors[key.charAt(0).toUpperCase() + key.slice(1)]
    return (
      <div className={error ? 'field field--invalid' : 'field'}>
        <label htmlFor={key}>{label}</label>
        <input
          id={key}
          type="text"
          inputMode="decimal"
          value={fees[key]}
          onChange={(e) => setFees((f) => ({ ...f, [key]: e.target.value }))}
        />
        {error
          ? <div className="field__error">{error}</div>
          : <div className="field__hint">{form.currency}. 0 = not yet set.</div>}
      </div>
    )
  }

  return (
    <section>
      <div className="admin-toolbar">
        <h1 className="admin-title">Settings</h1>
        <span className="admin-muted">Last changed {formatDate(form.updatedAt)}</span>
      </div>

      <form className="form admin-form" onSubmit={onSubmit}>
        <fieldset>
          <legend>Fees (§12(f) — as decided by the General Assembly)</legend>
          {money('joiningFee', 'Joining fee (one-time)')}
          {money('annualFeeOrdinary', 'Annual fee — ordinary members')}
          {money('annualFeeSupporting', 'Annual fee — extraordinary members')}
          <p className="admin-muted small-print">
            While all fees are 0, the public pages say that fees will be published.
          </p>
        </fieldset>

        <fieldset>
          <legend>Document versions</legend>
          <div className="field">
            <label htmlFor="statutesVersion">Statutes version</label>
            <input id="statutesVersion" type="text" value={form.statutesVersion}
              onChange={(e) => set('statutesVersion', e.target.value)} />
          </div>
          <div className="field">
            <label htmlFor="privacyPolicyVersion">Privacy policy version</label>
            <input id="privacyPolicyVersion" type="text" value={form.privacyPolicyVersion}
              onChange={(e) => set('privacyPolicyVersion', e.target.value)} />
            <div className="field__hint">
              Applicants consent to these versions. Change them only together with the published
              documents — the privacy policy page states its own version.
            </div>
          </div>
        </fieldset>

        {message && (
          <div className={message.error ? 'notice notice--error' : 'notice'} role="status">
            <p>{message.text}</p>
          </div>
        )}

        <button type="submit" className="cta cta--primary" disabled={busy}>
          {busy ? 'Saving…' : 'Save settings'}
        </button>
      </form>
    </section>
  )
}
