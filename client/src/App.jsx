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
import ScraperPage from './pages/ScraperPage'
import DeadlinesPage from './pages/DeadlinesPage'
import InventoryPage from './pages/InventoryPage'
import QuotationsPage from './pages/QuotationsPage'
import QuotationBuilderPage from './pages/QuotationBuilderPage'
import DraftingPage from './pages/DraftingPage'
import DraftWorkspacePage from './pages/DraftWorkspacePage'
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
        <Route path="/scraper" element={<RoleRoute allow={EDITORS}><ScraperPage /></RoleRoute>} />
        <Route path="/deadlines" element={<DeadlinesPage />} />
        <Route path="/inventory" element={<InventoryPage />} />
        <Route path="/quotations" element={<QuotationsPage />} />
        <Route path="/quotations/:id" element={<RoleRoute allow={EDITORS}><QuotationBuilderPage /></RoleRoute>} />
        <Route path="/drafting" element={<RoleRoute allow={EDITORS}><DraftingPage /></RoleRoute>} />
        <Route path="/drafting/:id" element={<RoleRoute allow={EDITORS}><DraftWorkspacePage /></RoleRoute>} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
