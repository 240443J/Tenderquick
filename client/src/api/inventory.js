import axios from './axios'

const BASE = '/api/inventory'

// Equipment
export const getEquipment    = (params)   => axios.get(`${BASE}/equipment`, { params })
export const createEquipment = (data)     => axios.post(`${BASE}/equipment`, data)
export const updateEquipment = (id, data) => axios.put(`${BASE}/equipment/${id}`, data)
export const removeEquipment = (id)       => axios.delete(`${BASE}/equipment/${id}`)
export const getPriceHistory = (id)       => axios.get(`${BASE}/equipment/${id}/price-history`)
export const getCurrentPrice = (id)       => axios.get(`${BASE}/equipment/${id}/current-price`)
export const addPrice        = (id, data) => axios.post(`${BASE}/equipment/${id}/prices`, data)

// Labour rates
export const getLabour        = ()         => axios.get(`${BASE}/labour`)
export const createLabour     = (data)     => axios.post(`${BASE}/labour`, data)
export const updateLabour     = (id, data) => axios.put(`${BASE}/labour/${id}`, data)
export const removeLabour     = (id)       => axios.delete(`${BASE}/labour/${id}`)
export const getLabourHistory = (id)       => axios.get(`${BASE}/labour/${id}/history`)
