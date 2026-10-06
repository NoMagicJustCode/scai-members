import { useEffect } from 'react'
import { Link, Outlet, useLocation } from 'react-router-dom'
import { ORG } from '../lib/org'

/** Nav and footer follow scai.world's components/Nav.tsx and Footer.tsx. */
export default function Layout() {
  const { pathname } = useLocation()
  useEffect(() => {
    window.scrollTo(0, 0)
  }, [pathname])

  return (
    <div className="page">
      <nav className="nav">
        <div className="nav-inner">
          <Link to="/" className="nav-logo">
            SCAI
            <span className="nav-logo-sub">Members</span>
          </Link>
          <ul className="nav-links">
            <li><a href={ORG.statutesUrl}>Statutes</a></li>
            <li><Link to="/apply">Apply</Link></li>
          </ul>
        </div>
      </nav>

      <main>
        <Outlet />
      </main>

      <footer className="footer">
        <div className="footer-left">
          © 2026 {ORG.shortName} · ZVR {ORG.zvr}
        </div>
        <div className="footer-links">
          <a href={ORG.website}>scai.world</a>
          <Link to="/impressum">Impressum</Link>
          <Link to="/privacy">Privacy Policy</Link>
        </div>
      </footer>
    </div>
  )
}
