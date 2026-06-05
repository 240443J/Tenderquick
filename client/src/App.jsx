import { Routes, Route } from 'react-router-dom'
import AppShell from './components/layout/AppShell'
import ProtectedRoute from './components/common/ProtectedRoute'
import RoleRoute from './components/common/RoleRoute'
import LoginPage from './pages/LoginPage'
import DashboardPage from './pages/DashboardPage'
import TendersListPage from './pages/TendersListPage'
import TenderDetailPage from './pages/TenderDetailPage'
import TenderFormPage from './pages/TenderFormPage'
import NotFoundPage from './pages/NotFoundPage'

const editorRoles = ['Admin', 'Estimator']

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<ProtectedRoute><AppShell /></ProtectedRoute>}>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/tenders" element={<TendersListPage />} />
        <Route
          path="/tenders/new"
          element={<RoleRoute roles={editorRoles}><TenderFormPage /></RoleRoute>}
        />
        <Route path="/tenders/:id" element={<TenderDetailPage />} />
        <Route
          path="/tenders/:id/edit"
          element={<RoleRoute roles={editorRoles}><TenderFormPage /></RoleRoute>}
        />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
