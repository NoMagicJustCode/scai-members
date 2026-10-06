import { useCallback, useEffect, useState } from 'react'
import {
  approveApplication,
  fetchApplications,
  rejectApplication,
  type AdminApplication,
  type ApiResult,
  type DecisionResponse,
} from '../../lib/api'
import { useAuth } from '../../lib/auth'
import { className, formatDate } from '../../lib/format'

type Decision = 'approve' | 'reject'

/** §7(2): the board decides on admission and may refuse without stating grounds. */
export default function Applications() {
  const { expire } = useAuth()
  const [pending, setPending] = useState(true)
  const [items, setItems] = useState<AdminApplication[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [notice, setNotice] = useState<{ text: string; warn: boolean } | null>(null)

  const load = useCallback(async () => {
    setItems(null)
    setError(null)
    const r = await fetchApplications(pending)
    if (r.ok) setItems(r.data)
    else if (r.status === 401) expire()
    else setError(r.message)
  }, [pending, expire])

  useEffect(() => {
    load()
  }, [load])

  async function decide(app: AdminApplication, decision: Decision) {
    setNotice(null)
    const r: ApiResult<DecisionResponse> =
      decision === 'approve' ? await approveApplication(app.id) : await rejectApplication(app.id)
    if (r.ok) setNotice({ text: `${app.name}: ${r.data.message}`, warn: !r.data.emailSent })
    else if (r.status === 401) return expire()
    else setNotice({ text: `${app.name}: ${r.message}`, warn: true })
    load()
  }

  return (
    <section>
      <div className="admin-toolbar">
        <h1 className="admin-title">Applications</h1>
        <div className="segmented">
          <button type="button" aria-pressed={pending} onClick={() => setPending(true)}>Pending</button>
          <button type="button" aria-pressed={!pending} onClick={() => setPending(false)}>Decided</button>
        </div>
      </div>

      {notice && (
        <div className={notice.warn ? 'notice notice--error' : 'notice'} role="status">
          <p>{notice.text}</p>
        </div>
      )}
      {error && <div className="notice notice--error"><p>{error}</p></div>}
      {items === null && !error && <p className="admin-muted">Loading…</p>}
      {items?.length === 0 && (
        <p className="admin-muted">
          {pending ? 'No applications are waiting for a decision.' : 'No decisions in the last 30 days.'}
        </p>
      )}

      <div className="app-list">
        {items?.map((a) => (
          <ApplicationCard key={a.id} app={a} onDecide={pending ? (d) => decide(a, d) : undefined} />
        ))}
      </div>

      {pending && (
        <p className="admin-muted small-print">
          Only applications with a confirmed email address are shown. Decisions are logged with
          your name; the applicant is notified by email. Declined applications are deleted after
          30 days.
        </p>
      )}
    </section>
  )
}

function ApplicationCard(props: { app: AdminApplication; onDecide?: (d: Decision) => Promise<void> }) {
  const { app, onDecide } = props
  const [confirming, setConfirming] = useState<Decision | null>(null)
  const [busy, setBusy] = useState(false)

  async function confirm() {
    if (!confirming || !onDecide) return
    setBusy(true)
    await onDecide(confirming)
    setBusy(false)
    setConfirming(null)
  }

  return (
    <article className="app-card">
      <div className="app-card__head">
        <div>
          <h3>{app.name}</h3>
          <div className="admin-muted">
            {app.memberType === 'Organisation' ? `Organisation · represented by ${app.representative}` : 'Person'}
            {' · '}
            {className(app.requestedClass)} member
          </div>
        </div>
        {app.state !== 'Pending' ? (
          <span className={`state-pill state-pill--${app.state.toLowerCase()}`}>
            {app.state === 'Approved' ? 'Admitted' : app.state === 'Rejected' ? 'Declined' : app.state}
          </span>
        ) : (
          <span className="admin-muted">Submitted {formatDate(app.submittedAt)}</span>
        )}
      </div>

      <dl className="app-card__facts">
        <dt>Email</dt>
        <dd>{app.email}</dd>
        {app.dateOfBirth && (<><dt>Date of birth</dt><dd>{formatDate(app.dateOfBirth)}</dd></>)}
        {app.postalAddress && (<><dt>Address</dt><dd>{app.postalAddress}</dd></>)}
        {app.country && (<><dt>Country</dt><dd>{app.country}</dd></>)}
        {app.decidedAt && (<><dt>Decided</dt><dd>{formatDate(app.decidedAt)} by {app.decidedBy ?? 'unknown'}</dd></>)}
      </dl>

      {app.motivation && <blockquote className="app-card__motivation">{app.motivation}</blockquote>}

      {onDecide && (
        <div className="app-card__actions">
          {confirming === null ? (
            <>
              <button type="button" className="cta cta--primary cta--small" onClick={() => setConfirming('approve')}>Admit</button>
              <button type="button" className="cta cta--small" onClick={() => setConfirming('reject')}>Decline</button>
            </>
          ) : (
            <>
              <span className="app-card__confirm">
                {confirming === 'approve'
                  ? `Admit ${app.name} as ${className(app.requestedClass).toLowerCase()} member?`
                  : `Decline ${app.name}? No reason is given (§7(2)).`}
              </span>
              <button type="button" className="cta cta--primary cta--small" disabled={busy} onClick={confirm}>
                {busy ? 'Saving…' : 'Confirm'}
              </button>
              <button type="button" className="cta cta--small" disabled={busy} onClick={() => setConfirming(null)}>Cancel</button>
            </>
          )}
        </div>
      )}
    </article>
  )
}
