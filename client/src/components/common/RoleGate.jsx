import { useAuth } from '../../context/AuthContext'

// Renders children only if the current user's role is in `allow`.
// UI convenience only — the API is the real gate.
export default function RoleGate({ allow, children, fallback = null }) {
  const { user } = useAuth()
  if (!user || !allow.includes(user.role)) return fallback
  return children
}
