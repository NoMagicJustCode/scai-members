import { Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import { AuthProvider } from './lib/auth'
import AdminLayout from './pages/admin/AdminLayout'
import AdminLogin from './pages/admin/AdminLogin'
import Applications from './pages/admin/Applications'
import Members from './pages/admin/Members'
import Settings from './pages/admin/Settings'
import Apply from './pages/Apply'
import ApplyConfirm from './pages/ApplyConfirm'
import Imprint from './pages/Imprint'
import Landing from './pages/Landing'
import NotFound from './pages/NotFound'
import Privacy from './pages/Privacy'

export default function App() {
  return (
    <AuthProvider>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<Landing />} />
          <Route path="/apply" element={<Apply />} />
          <Route path="/apply/confirm" element={<ApplyConfirm />} />
          <Route path="/privacy" element={<Privacy />} />
          <Route path="/impressum" element={<Imprint />} />

          <Route path="/admin/login" element={<AdminLogin />} />
          <Route path="/admin" element={<AdminLayout />}>
            <Route index element={<Applications />} />
            <Route path="members" element={<Members />} />
            <Route path="settings" element={<Settings />} />
          </Route>

          {/* Phase 4: /login, /me, /documents · Phase 5: /f/:slug */}
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </AuthProvider>
  )
}
