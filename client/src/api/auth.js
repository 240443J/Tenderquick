import axios from './axios'

const BASE = '/api/auth'

export const login    = (data) => axios.post(`${BASE}/login`, data)
export const register = (data) => axios.post(`${BASE}/register`, data)
export const me       = ()     => axios.get(`${BASE}/me`)
