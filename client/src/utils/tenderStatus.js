// Maps a Tender.Status string to a status-palette token + display label.
const MAP = {
  Interested: { token: 'neutral', label: 'Interested' },
  Drafting: { token: 'draft', label: 'Drafting' },
  Submitted: { token: 'soon', label: 'Submitted' },
  Won: { token: 'onTrack', label: 'Won' },
  Lost: { token: 'overdue', label: 'Lost' },
}

export const TENDER_STATUSES = Object.keys(MAP)

export function statusMeta(status) {
  return MAP[status] ?? { token: 'neutral', label: status ?? 'Unknown' }
}
