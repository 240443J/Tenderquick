import { createContext, useContext, useState } from 'react'
import { db } from '../mock/db'

const AuthContext = createContext(null)

// PROTOTYPE: offline auth. Starts signed in as the seeded admin so the demo
// opens straight into the workspace. Real JWT login wires back in here later.
export function AuthProvider({ children }) {
  const [user, setUser] = useState(db.user)

  const login = async () => {
    setUser(db.user)
    return { user: db.user }
  }

  const logout = () => setUser(null)

  return (
    <AuthContext.Provider value={{ user, loading: false, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider')
  return ctx
}
