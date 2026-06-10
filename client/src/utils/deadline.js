import { tokens } from '../theme'
import { daysUntil } from './format'

// Tiered urgency styling for a deadline date.
export function deadlineTone(dueAt) {
  const d = daysUntil(dueAt)
  if (d === null) return { label: '—', color: tokens.statusNeutral, bg: tokens.statusNeutralBg, days: null }
  if (d < 0) return { label: `${Math.abs(d)}d overdue`, color: tokens.statusOverdue, bg: tokens.statusOverdueBg, days: d }
  if (d === 0) return { label: 'Due today', color: tokens.statusOverdue, bg: tokens.statusOverdueBg, days: d }
  if (d <= 3) return { label: `${d}d left`, color: tokens.statusUrgent, bg: tokens.statusUrgentBg, days: d }
  if (d <= 7) return { label: `${d}d left`, color: tokens.statusSoon, bg: tokens.statusSoonBg, days: d }
  return { label: `${d}d left`, color: tokens.statusOnTrack, bg: tokens.statusOnTrackBg, days: d }
}
