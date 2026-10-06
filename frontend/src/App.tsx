import { Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import Apply from './pages/Apply'
import ApplyConfirm from './pages/ApplyConfirm'
import Imprint from './pages/Imprint'
import Landing from './pages/Landing'
import NotFound from './pages/NotFound'
import Privacy from './pages/Privacy'

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Landing />} />
        <Route path="/apply" element={<Apply />} />
        <Route path="/apply/confirm" element={<ApplyConfirm />} />
        <Route path="/privacy" element={<Privacy />} />
        <Route path="/impressum" element={<Imprint />} />
        {/* Phase 4: /login, /me, /documents · Phase 5: /f/:slug */}
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  )
}
