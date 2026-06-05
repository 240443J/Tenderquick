import axios from './axios'

const BASE = '/api/audit'

export const getRecent = (limit = 50) => axios.get(`${BASE}/recent`, { params: { limit } })
