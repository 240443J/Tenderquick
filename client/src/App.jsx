import { Routes, Route } from 'react-router-dom'
import ProtectedRoute from './components/common/ProtectedRoute'
import RoleRoute from './components/common/RoleRoute'
import AppShell from './components/layout/AppShell'
import LoginPage from './pages/LoginPage'
import DashboardPage from './pages/DashboardPage'
import TendersListPage from './pages/TendersListPage'
import TenderDetailPage from './pages/TenderDetailPage'
import TenderFormPage from './pages/TenderFormPage'
import TenderSearchPage from './pages/TenderSearchPage'
import NotFoundPage from './pages/NotFoundPage'

const EDITORS = ['Admin', 'Estimator']

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<ProtectedRoute><AppShell /></ProtectedRoute>}>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/tenders" element={<TendersListPage />} />
        <Route path="/tenders/new" element={<RoleRoute allow={EDITORS}><TenderFormPage /></RoleRoute>} />
        <Route path="/tenders/:id" element={<TenderDetailPage />} />
        <Route path="/tenders/:id/edit" element={<RoleRoute allow={EDITORS}><TenderFormPage /></RoleRoute>} />
        <Route path="/search" element={<RoleRoute allow={EDITORS}><TenderSearchPage /></RoleRoute>} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
