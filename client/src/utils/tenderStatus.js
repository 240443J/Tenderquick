import { tokens } from '../theme'

export const TENDER_STATUSES = ['Interested', 'Drafting', 'Submitted', 'Won', 'Lost']

// Maps a tender status to its StatusChip colour tokens + label.
export function tenderStatusStyle(status) {
  switch (status) {
    case 'Won':
      return { color: tokens.statusOnTrack, bg: tokens.statusOnTrackBg, label: 'Won' }
    case 'Submitted':
      return { color: tokens.statusOnTrack, bg: tokens.statusOnTrackBg, label: 'Submitted' }
    case 'Drafting':
      return { color: tokens.statusDraft, bg: tokens.statusDraftBg, label: 'Drafting' }
    case 'Interested':
      return { color: tokens.accentIndigo, bg: tokens.accentIndigoSubtle, label: 'Interested' }
    case 'Lost':
      return { color: tokens.statusNeutral, bg: tokens.statusNeutralBg, label: 'Lost' }
    default:
      return { color: tokens.statusNeutral, bg: tokens.statusNeutralBg, label: status || 'Unknown' }
  }
}
