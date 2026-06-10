// PROTOTYPE: backed by the in-memory mock DB. Swap the bodies for axios calls
// (see git history / CLAUDE.md) once the backend is ready — the return shape
// ({ data }) already matches axios.
import { db, nextId, logAudit } from '../mock/db'
import { respond } from '../mock/respond'

const byId = (id) => db.tenders.find((t) => String(t.id) === String(id))

export const getAll = (params = {}) => {
  let rows = [...db.tenders]
  if (params.status) rows = rows.filter((t) => t.status === params.status)
  if (params.search) {
    const q = params.search.toLowerCase()
    rows = rows.filter((t) =>
      [t.reference, t.title, t.agency].some((v) => v.toLowerCase().includes(q)))
  }
  rows.sort((a, b) => new Date(a.closingAt) - new Date(b.closingAt))
  return respond(rows)
}

export const getById = (id) => respond(byId(id))

export const create = (data) => {
  const now = new Date().toISOString()
  const tender = {
    id: nextId(),
    status: 'Interested',
    source: 'Manual',
    specs: [],
    ...data,
    createdAt: now,
    updatedAt: now,
  }
  db.tenders.unshift(tender)
  logAudit('Tender created', tender.reference)
  return respond(tender)
}

export const update = (id, data) => {
  const tender = byId(id)
  Object.assign(tender, data, { updatedAt: new Date().toISOString() })
  logAudit('Tender updated', tender.reference)
  return respond(tender)
}

export const remove = (id) => {
  const i = db.tenders.findIndex((t) => String(t.id) === String(id))
  if (i >= 0) db.tenders.splice(i, 1)
  return respond({ ok: true })
}
