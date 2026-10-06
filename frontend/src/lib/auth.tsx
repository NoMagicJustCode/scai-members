import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react'
import { fetchSession, login as apiLogin, logout as apiLogout, type ApiResult, type Session } from './api'

interface AuthState {
  /** undefined while the first check is running; null when signed out. */
  session: Session | null | undefined
  login: (email: string, password: string) => Promise<ApiResult<Session>>
  logout: () => Promise<void>
  /** Call when an API request answers 401: the session has ended. */
  expire: () => void
}

const AuthContext = createContext<AuthState | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null | undefined>(undefined)

  useEffect(() => {
    fetchSession().then((r) => setSession(r.ok ? r.data : null))
  }, [])

  const login = useCallback(async (email: string, password: string) => {
    const r = await apiLogin(email, password)
    if (r.ok) setSession(r.data)
    return r
  }, [])

  const logout = useCallback(async () => {
    await apiLogout()
    setSession(null)
  }, [])

  const expire = useCallback(() => setSession(null), [])

  return <AuthContext.Provider value={{ session, login, logout, expire }}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
