import axios from './axios'

const BASE = '/api/tenders'

export const getAll = (params) => axios.get(BASE, { params })
export const getById = (id) => axios.get(`${BASE}/${id}`)
export const create = (data) => axios.post(BASE, data)
export const update = (id, data) => axios.put(`${BASE}/${id}`, data)
export const remove = (id) => axios.delete(`${BASE}/${id}`)
