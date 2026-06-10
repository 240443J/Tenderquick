// PROTOTYPE: backed by the in-memory mock DB.
import { db, nextId, logAudit } from '../mock/db'
import { respond } from '../mock/respond'
import { generateQuotationLines } from '../mock/ai'

const byId = (id) => db.quotations.find((q) => String(q.id) === String(id))

export const getAll = () => respond([...db.quotations])
export const getById = (id) => respond(byId(id))

// AI: build a draft quotation from a tender's specs + inventory pricing.
export const generateFromTender = (tenderId) => {
  const tender = db.tenders.find((t) => String(t.id) === String(tenderId))
  const lineItems = generateQuotationLines(tender)
  const quote = {
    id: nextId(),
    quoteNo: `TQ-Q2026-${Math.floor(Math.random() * 900) + 100}`,
    tenderId: tender.id,
    tenderRef: tender.reference,
    title: tender.title,
    client: tender.agency,
    status: 'Draft',
    verified: false,
    verifiedBy: null,
    verifiedAt: null,
    markupPct: 15,
    gstPct: 9,
    createdAt: new Date().toISOString(),
    lineItems,
  }
  db.quotations.unshift(quote)
  logAudit('AI quotation drafted', quote.quoteNo, 'AI Assistant')
  return respond(quote, 900)
}

export const update = (id, data) => {
  const q = byId(id)
  Object.assign(q, data)
  return respond(q)
}

export const verify = (id, verifiedBy) => {
  const q = byId(id)
  q.verified = true
  q.status = 'Verified'
  q.verifiedBy = verifiedBy
  q.verifiedAt = new Date().toISOString()
  logAudit('Quotation verified', q.quoteNo, verifiedBy)
  return respond(q)
}

export const remove = (id) => {
  const i = db.quotations.findIndex((q) => String(q.id) === String(id))
  if (i >= 0) db.quotations.splice(i, 1)
  return respond({ ok: true })
}
