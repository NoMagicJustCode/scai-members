import { useEffect, useState } from 'react'
import { fetchStatus, type ApiStatus } from '../lib/api'

export default function Landing() {
  const [status, setStatus] = useState<ApiStatus | null>(null)
  const [error, setError] = useState(false)

  useEffect(() => {
    fetchStatus().then(setStatus).catch(() => setError(true))
  }, [])

  return (
    <main className="container" style={{ paddingTop: '14vh', paddingBottom: '10vh' }}>
      <span className="badge">Membership</span>
      <h1 style={{ marginTop: '1rem' }}>Second Circuit</h1>
      <p style={{ fontFamily: 'var(--font-display)', fontSize: '1.35rem', color: 'var(--ink-soft)', margin: 0 }}>
        Association for Digital Freedom of Thought
      </p>
      <hr className="rule-gold" />
      <p style={{ maxWidth: '58ch' }}>
        The membership platform of Second Circuit — Verein für digitale
        Gedankenfreiheit, Vienna (ZVR 1684464197) — is under construction.
        Applications, the member area and shared documents will live here.
      </p>
      <p style={{ fontSize: '0.9rem', color: 'var(--ink-soft)' }}>
        {error && <span className="status-bad">API: unreachable</span>}
        {status && (
          <>
            API: <span className="status-ok">up</span> · MongoDB:{' '}
            <span className={status.mongo === 'ok' ? 'status-ok' : 'status-bad'}>
              {status.mongo}
            </span>{' '}
            · phase {status.phase}
          </>
        )}
        {!error && !status && 'Checking API…'}
      </p>
    </main>
  )
}
