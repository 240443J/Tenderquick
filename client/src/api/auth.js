// PROTOTYPE: offline mock auth. Always resolves the seeded admin user.
import { db } from '../mock/db'
import { respond } from '../mock/respond'

export const login = () => respond({ token: 'mock-token', user: db.user })
export const me = () => respond(db.user)
export const listUsers = () => respond([db.user])
export const createUser = (data) => respond({ id: 2, ...data })
