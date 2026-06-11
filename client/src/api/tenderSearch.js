import axios from './axios'

const BASE = '/api/tender-search'

export const search = (keyword, sources, limit = 50) =>
  axios.get(BASE, { params: { keyword, sources: sources.join(','), limit } })

export const importResults = (items) =>
  axios.post(`${BASE}/import`, { items })
