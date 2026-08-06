import axios from './axios'

const BASE = '/api/quotations'

export const getAll  = (params)   => axios.get(BASE, { params })
export const getById = (id)       => axios.get(`${BASE}/${id}`)
export const update  = (id, data) => axios.put(`${BASE}/${id}`, data)
export const remove  = (id)       => axios.delete(`${BASE}/${id}`)

// Drafts a priced quotation from the tender specs + current inventory pricing.
export const generateFromTender = (tenderId) => axios.post(`${BASE}/generate/${tenderId}`)

// The signer is taken from the bearer token server-side, so nothing is sent in the body.
export const verify      = (id) => axios.post(`${BASE}/${id}/verify`)
export const getSignoffs = (id) => axios.get(`${BASE}/${id}/signoffs`)
