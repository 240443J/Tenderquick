import { Navigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'

// Route-level guard: redirects to `redirectTo` if the user's role is not allowed.
export default function RoleRoute({ roles, children, redirectTo = '/tenders' }) {
  const { user } = useAuth()
  if (!user || !roles.includes(user.role)) return <Navigate to={redirectTo} replace />
  return children
}
