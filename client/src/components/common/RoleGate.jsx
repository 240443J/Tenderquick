import { useAuth } from '../../context/AuthContext'

// Renders children only if the current user's role is in `roles`.
// UI convenience only — the API is the real authorization source of truth.
export default function RoleGate({ roles, children, fallback = null }) {
  const { user } = useAuth()
  if (!user || !roles.includes(user.role)) return fallback
  return children
}

export function useHasRole(...roles) {
  const { user } = useAuth()
  return !!user && roles.includes(user.role)
}
