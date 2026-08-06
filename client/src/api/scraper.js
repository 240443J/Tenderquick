import axios from './axios'

const BASE = '/api/scraper'

export const getSources = () => axios.get(`${BASE}/sources`)

export const scan = (keyword, sources, limit = 25) =>
  axios.post(`${BASE}/scan`, { keyword, sources, limit })

// Creates a tender plus its closing deadline in one call.
export const importResult = (id) => axios.post(`${BASE}/import/${id}`)

export const getWatchlist   = ()     => axios.get(`${BASE}/watchlist`)
export const createWatch    = (data) => axios.post(`${BASE}/watchlist`, data)
export const removeWatch    = (id)   => axios.delete(`${BASE}/watchlist/${id}`)
