import { Navigate, NavLink, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../../lib/auth'

/** Guards the board area and provides its sub-navigation. */
export default function AdminLayout() {
  const { session, logout } = useAuth()
  const location = useLocation()

  if (session === undefined) return <div className="admin-shell"><p className="admin-muted">Loading…</p></div>
  if (session === null) return <Navigate to="/admin/login" replace state={{ from: location.pathname }} />

  return (
    <div className="admin-shell">
      <header className="admin-header">
        <div>
          <div className="eyebrow" style={{ marginBottom: '0.3rem' }}>Board</div>
          <nav className="admin-tabs">
            <NavLink to="/admin" end>Applications</NavLink>
            <NavLink to="/admin/members">Members</NavLink>
            <NavLink to="/admin/settings">Settings</NavLink>
          </nav>
        </div>
        <div className="admin-user">
          <span>{session.name}</span>
          <button type="button" className="cta cta--small" onClick={logout}>Sign out</button>
        </div>
      </header>
      <Outlet />
    </div>
  )
}
