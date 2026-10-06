import { useState, type FormEvent } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import PageHero, { Divider } from '../../components/PageHero'
import { useAuth } from '../../lib/auth'

export default function AdminLogin() {
  const { session, login } = useAuth()
  const navigate = useNavigate()
  const from = (useLocation().state as { from?: string } | null)?.from ?? '/admin'
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  if (session) return <Navigate to={from} replace />

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    const r = await login(email.trim(), password)
    setBusy(false)
    if (r.ok) navigate(from, { replace: true })
    else setError(r.message)
  }

  return (
    <>
      <PageHero eyebrow="Board" title="Sign in" intro="For members of the board of Second Circuit." />
      <Divider />
      <section className="form-section" style={{ maxWidth: 480 }}>
        <form className="form" onSubmit={onSubmit}>
          <div className="field">
            <label htmlFor="email">Email address</label>
            <input id="email" type="email" autoComplete="username" value={email}
              onChange={(e) => setEmail(e.target.value)} required />
          </div>
          <div className="field">
            <label htmlFor="password">Password</label>
            <input id="password" type="password" autoComplete="current-password" value={password}
              onChange={(e) => setPassword(e.target.value)} required />
          </div>
          {error && (
            <div className="notice notice--error" role="alert">
              <p>{error}</p>
            </div>
          )}
          <button type="submit" className="cta cta--primary" disabled={busy}>
            {busy ? 'Signing in…' : 'Sign in'}
          </button>
        </form>
      </section>
    </>
  )
}
