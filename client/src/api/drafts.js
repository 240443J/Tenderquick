import axios from './axios'

const BASE = '/api/drafts'

export const getAll  = (params)   => axios.get(BASE, { params })
export const getById = (id)       => axios.get(`${BASE}/${id}`)
export const create  = (data)     => axios.post(BASE, data)
export const update  = (id, data) => axios.put(`${BASE}/${id}`, data)
export const remove  = (id)       => axios.delete(`${BASE}/${id}`)

// Returns sections without saving them — the workspace streams them in, then persists.
export const generateSections = (tenderId) => axios.post(`${BASE}/generate/${tenderId}`)

export const getMemory     = ()     => axios.get(`${BASE}/memory`)
export const learnFromEdit = (text) => axios.post(`${BASE}/memory/learn`, { text: text ?? null })
