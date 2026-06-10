import { Routes, Route } from 'react-router-dom'
import AppShell from './components/layout/AppShell'
import ProtectedRoute from './components/common/ProtectedRoute'
import LoginPage from './pages/LoginPage'
import DashboardPage from './pages/DashboardPage'
import TendersListPage from './pages/TendersListPage'
import TenderDetailPage from './pages/TenderDetailPage'
import TenderFormPage from './pages/TenderFormPage'
import DraftingPage from './pages/DraftingPage'
import DraftWorkspacePage from './pages/DraftWorkspacePage'
import ScraperPage from './pages/ScraperPage'
import QuotationsPage from './pages/QuotationsPage'
import QuotationBuilderPage from './pages/QuotationBuilderPage'
import InventoryPage from './pages/InventoryPage'
import DeadlinesPage from './pages/DeadlinesPage'
import NotFoundPage from './pages/NotFoundPage'

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<ProtectedRoute><AppShell /></ProtectedRoute>}>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/tenders" element={<TendersListPage />} />
        <Route path="/tenders/new" element={<TenderFormPage />} />
        <Route path="/tenders/:id" element={<TenderDetailPage />} />
        <Route path="/tenders/:id/edit" element={<TenderFormPage />} />

        <Route path="/drafting" element={<DraftingPage />} />
        <Route path="/drafting/:id" element={<DraftWorkspacePage />} />

        <Route path="/scraper" element={<ScraperPage />} />

        <Route path="/quotations" element={<QuotationsPage />} />
        <Route path="/quotations/:id" element={<QuotationBuilderPage />} />

        <Route path="/inventory" element={<InventoryPage />} />
        <Route path="/deadlines" element={<DeadlinesPage />} />

        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
