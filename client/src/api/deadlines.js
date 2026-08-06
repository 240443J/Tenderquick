import axios from './axios'

const BASE = '/api/deadlines'

export const getAll  = (params)   => axios.get(BASE, { params })
export const create  = (data)     => axios.post(BASE, data)
export const update  = (id, data) => axios.put(`${BASE}/${id}`, data)
export const remove  = (id)       => axios.delete(`${BASE}/${id}`)

export const getCalendar        = () => axios.get(`${BASE}/calendar`)
export const connectCalendar    = () => axios.post(`${BASE}/calendar/connect`)
export const disconnectCalendar = () => axios.post(`${BASE}/calendar/disconnect`)
export const addToCalendar      = (id) => axios.post(`${BASE}/${id}/calendar`)
export const syncAllToCalendar  = () => axios.post(`${BASE}/calendar/sync-all`)
