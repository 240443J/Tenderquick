import axios from './axios'

export const recent = (limit = 20) => axios.get('/api/audit/recent', { params: { limit } })
