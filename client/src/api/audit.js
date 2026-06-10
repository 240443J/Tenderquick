// PROTOTYPE: backed by the in-memory mock DB.
import { db } from '../mock/db'
import { respond } from '../mock/respond'

export const getRecent = (limit = 50) => respond(db.audit.slice(0, limit))
