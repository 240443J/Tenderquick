// PROTOTYPE: backed by the in-memory mock DB.
import { db, logAudit } from '../mock/db'
import { respond } from '../mock/respond'

export const getAll = () => {
  const rows = [...db.deadlines].sort((a, b) => new Date(a.dueAt) - new Date(b.dueAt))
  return respond(rows)
}

export const getCalendar = () => respond(db.calendar)

export const connectCalendar = () => {
  db.calendar = { connected: true, account: db.user.email }
  logAudit('Connected Google Calendar', db.user.email)
  return respond(db.calendar, 800)
}

export const disconnectCalendar = () => {
  db.calendar = { connected: false, account: null }
  return respond(db.calendar)
}

export const addToCalendar = (id) => {
  const d = db.deadlines.find((x) => String(x.id) === String(id))
  d.addedToCalendar = true
  logAudit('Deadline added to Google Calendar', d.title)
  return respond(d, 600)
}

export const syncAllToCalendar = () => {
  db.deadlines.forEach((d) => { d.addedToCalendar = true })
  logAudit('Synced all deadlines to Google Calendar', `${db.deadlines.length} events`)
  return respond([...db.deadlines], 900)
}
