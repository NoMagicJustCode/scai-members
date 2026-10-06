import { Route, Routes } from 'react-router-dom'
import Landing from './pages/Landing'

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Landing />} />
      {/* Phase 2: /apply · Phase 4: /login, /me, /documents · Phase 5: /f/:slug */}
    </Routes>
  )
}
