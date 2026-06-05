import axios from './axios'

const BASE = '/api/auth'

export const login = (data) => axios.post(`${BASE}/login`, data)
export const me = () => axios.get(`${BASE}/me`)
export const listUsers = () => axios.get(`${BASE}/users`)
export const createUser = (data) => axios.post(`${BASE}/users`, data)
