// PROTOTYPE: backed by the in-memory mock DB.
import { db, nextId, logAudit } from '../mock/db'
import { respond } from '../mock/respond'
import { generateDraftSections } from '../mock/ai'

const byId = (id) => db.drafts.find((d) => String(d.id) === String(id))

export const getAll = () => respond([...db.drafts])
export const getById = (id) => respond(byId(id))

// AI: generate a fresh set of draft sections for a tender. Returns the raw
// sections (the page streams them in); persistence happens via create/update.
export const generateSections = (tenderId) => {
  const tender = db.tenders.find((t) => String(t.id) === String(tenderId))
  return respond({ sections: generateDraftSections(tender), tender }, 700)
}

export const create = (data) => {
  const draft = {
    id: nextId(),
    type: 'Technical Proposal',
    status: 'Draft',
    version: 1,
    updatedAt: new Date().toISOString(),
    sections: [],
    ...data,
  }
  db.drafts.unshift(draft)
  logAudit('AI draft created', draft.title, 'AI Assistant')
  return respond(draft)
}

export const update = (id, data) => {
  const draft = byId(id)
  Object.assign(draft, data, {
    updatedAt: new Date().toISOString(),
    version: (draft.version || 0) + (data.bumpVersion ? 1 : 0),
  })
  return respond(draft)
}

export const getMemory = () => respond(db.draftMemory)

// Simulates the AI "learning" from a user edit — grows the memory.
export const learnFromEdit = (text) => {
  db.draftMemory.samplesLearned += 1
  db.draftMemory.lastUpdated = new Date().toISOString()
  if (text) {
    db.draftMemory.preferences.unshift({
      id: nextId(),
      text,
      confidence: 0.55,
      source: 'Learned just now from your edit',
    })
  }
  return respond(db.draftMemory)
}
