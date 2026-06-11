import { Navigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'

// Route-level guard: assumes already inside ProtectedRoute (user exists).
// Redirects to dashboard if the role isn't allowed.
export default function RoleRoute({ allow, children }) {
  const { user } = useAuth()
  if (!user || !allow.includes(user.role)) return <Navigate to="/" replace />
  return children
}
