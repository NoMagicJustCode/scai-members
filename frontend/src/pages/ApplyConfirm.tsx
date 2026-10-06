import { useEffect, useRef, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import PageHero from '../components/PageHero'
import { verifyEmail } from '../lib/api'

type State = { kind: 'working' } | { kind: 'ok' } | { kind: 'failed'; message: string }

export default function ApplyConfirm() {
  const [params] = useSearchParams()
  const token = params.get('token')
  const [state, setState] = useState<State>(
    token ? { kind: 'working' } : { kind: 'failed', message: 'This confirmation link is incomplete.' },
  )
  // Tokens are single-use: StrictMode's double effect run must not spend it twice.
  const sent = useRef(false)

  useEffect(() => {
    if (!token || sent.current) return
    sent.current = true
    verifyEmail(token).then((r) =>
      setState(r.ok ? { kind: 'ok' } : { kind: 'failed', message: r.message }),
    )
  }, [token])

  if (state.kind === 'working') return <PageHero eyebrow="Application" title="Confirming…" />

  if (state.kind === 'ok')
    return (
      <PageHero
        eyebrow="Application"
        title="Thank you"
        intro="Your email address is confirmed and your application is now with the board. You will hear from us by email once the board has decided."
      />
    )

  return (
    <PageHero
      eyebrow="Application"
      title="Link not valid"
      intro={state.message}
      meta="Confirmation links work once and expire after 48 hours. If yours has expired, simply apply again."
    >
      <div className="cta-row">
        <Link to="/apply" className="cta cta--primary">Apply again</Link>
      </div>
    </PageHero>
  )
}
