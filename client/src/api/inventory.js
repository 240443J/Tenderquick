// PROTOTYPE: backed by the in-memory mock DB.
import { db, nextId } from '../mock/db'
import { respond } from '../mock/respond'

// Equipment
export const getEquipment = () => respond([...db.equipment])
export const createEquipment = (data) => {
  const item = { id: nextId(), updatedAt: new Date().toISOString(), ...data }
  db.equipment.unshift(item)
  return respond(item)
}
export const updateEquipment = (id, data) => {
  const item = db.equipment.find((e) => String(e.id) === String(id))
  Object.assign(item, data, { updatedAt: new Date().toISOString() })
  return respond(item)
}
export const removeEquipment = (id) => {
  const i = db.equipment.findIndex((e) => String(e.id) === String(id))
  if (i >= 0) db.equipment.splice(i, 1)
  return respond({ ok: true })
}

// Labour rates
export const getLabour = () => respond([...db.labour])
export const createLabour = (data) => {
  const item = { id: nextId(), ...data }
  db.labour.unshift(item)
  return respond(item)
}
export const updateLabour = (id, data) => {
  const item = db.labour.find((l) => String(l.id) === String(id))
  Object.assign(item, data)
  return respond(item)
}
export const removeLabour = (id) => {
  const i = db.labour.findIndex((l) => String(l.id) === String(id))
  if (i >= 0) db.labour.splice(i, 1)
  return respond({ ok: true })
}
