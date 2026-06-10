// PROTOTYPE: backed by the in-memory mock DB.
import { db, nextId, logAudit } from '../mock/db'
import { respond } from '../mock/respond'
import { runScrape } from '../mock/ai'

export const getSources = () => respond([...db.scrapeSources])

export const scan = (keyword, sourceIds) =>
  respond(runScrape(keyword, sourceIds), 1100)

// Import a discovered tender into the workspace (creates a tender + closing deadline).
export const importResult = (resultId) => {
  const r = db.scrapeResults.find((x) => String(x.id) === String(resultId))
  if (!r) return respond({ ok: false })
  r.imported = true
  const now = new Date().toISOString()
  const tender = {
    id: nextId(),
    reference: r.reference,
    title: r.title,
    agency: r.agency,
    source: db.scrapeSources.find((s) => s.id === r.source)?.label || 'GeBiz',
    status: 'Interested',
    estValue: null,
    closingAt: r.closingAt,
    createdAt: now,
    updatedAt: now,
    notes: r.summary,
    specs: [],
  }
  db.tenders.unshift(tender)
  db.deadlines.unshift({
    id: nextId(),
    tenderId: tender.id,
    tenderRef: tender.reference,
    title: `${tender.title} — Tender Closing`,
    type: 'Closing',
    dueAt: r.closingAt,
    addedToCalendar: false,
    priority: 'medium',
  })
  logAudit('Tender imported from ' + tender.source, r.reference, 'AI Scraper')
  return respond({ ok: true, tender })
}
