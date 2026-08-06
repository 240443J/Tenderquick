const currency = new Intl.NumberFormat('en-SG', {
  style: 'currency',
  currency: 'SGD',
  currencyDisplay: 'narrowSymbol',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

export function formatCurrency(value) {
  if (value == null || value === '') return '—'
  const n = Number(value)
  return Number.isNaN(n) ? '—' : currency.format(n)
}

export function formatDate(value) {
  if (!value) return '—'
  const d = new Date(value)
  return Number.isNaN(d.getTime())
    ? '—'
    : d.toLocaleDateString('en-SG', { day: '2-digit', month: 'short', year: 'numeric' })
}

export function formatDateTime(value) {
  if (!value) return '—'
  const d = new Date(value)
  return Number.isNaN(d.getTime())
    ? '—'
    : d.toLocaleString('en-SG', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' })
}

// Whole days from now until `value`. Negative means overdue. Compared date-to-date so a
// deadline later today reads as 0 ("Due today"), not 1.
export function daysUntil(value) {
  if (!value) return null
  const target = new Date(value)
  if (Number.isNaN(target.getTime())) return null

  const startOfDay = (d) => new Date(d.getFullYear(), d.getMonth(), d.getDate())
  const diff = startOfDay(target) - startOfDay(new Date())
  return Math.round(diff / 86400000)
}
