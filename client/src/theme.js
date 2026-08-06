import { createTheme } from '@mui/material/styles'

// Design tokens — the single source of colour for the app (Style Guide §2).
// Import these rather than hardcoding hex values or using MUI's 'text.secondary'.
export const tokens = {
  // Surfaces
  white: '#FFFFFF',
  offWhite: '#FAFAFA',
  lightGray: '#F3F4F6',
  borderLight: '#E5E7EB',
  borderMedium: '#D1D5DB',

  // Dark sections
  navy: '#0F172A',
  darkCharcoal: '#1E293B',
  slate: '#334155',

  // Brand
  accentIndigo: '#4F46E5',
  accentIndigoHover: '#4338CA',
  accentIndigoLight: '#E0E7FF',
  accentIndigoSubtle: '#EEF2FF',

  // Status — drives deadlines, quote state, tender pipeline.
  // `*Bg` are the fixed tints; never compute opacity inline.
  statusOverdue: '#DC2626',
  statusOverdueBg: '#FEF2F2',
  statusUrgent: '#EA580C',
  statusUrgentBg: '#FFF7ED',
  statusSoon: '#CA8A04',
  statusSoonBg: '#FEFCE8',
  statusOnTrack: '#16A34A',
  statusOnTrackBg: '#F0FDF4',
  statusNeutral: '#6B7280',
  statusNeutralBg: '#F3F4F6',
  statusDraft: '#7C3AED',
  statusDraftBg: '#F5F3FF',

  // Text on light
  textPrimary: '#111827',
  textSecondary: '#6B7280',
  textMuted: '#9CA3AF',

  // Text on dark
  textOnDark: '#FFFFFF',
  textOnDarkMuted: 'rgba(255,255,255,0.6)',
  textOnDarkSubtle: 'rgba(255,255,255,0.7)',
}

// Money, quantities and tender refs — tabular figures so columns line up (Style Guide §9).
export const monoSx = {
  fontFamily: 'ui-monospace, SFMono-Regular, "SF Mono", Menlo, Consolas, monospace',
  fontVariantNumeric: 'tabular-nums',
  fontSize: '0.875rem',
  fontWeight: 500,
}

// Status palette — drives StatusChip and any status-coloured UI. `*Bg` are tint backgrounds.
export const statusPalette = {
  overdue: { main: tokens.statusOverdue, bg: tokens.statusOverdueBg },
  urgent: { main: tokens.statusUrgent, bg: tokens.statusUrgentBg },
  soon: { main: tokens.statusSoon, bg: tokens.statusSoonBg },
  onTrack: { main: tokens.statusOnTrack, bg: tokens.statusOnTrackBg },
  draft: { main: tokens.statusDraft, bg: tokens.statusDraftBg },
  neutral: { main: tokens.statusNeutral, bg: tokens.statusNeutralBg },
}

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: tokens.accentIndigo },
    secondary: { main: '#0EA5E9' },
    background: { default: '#F7F8FA', paper: tokens.white },
    text: { primary: tokens.textPrimary, secondary: tokens.textSecondary },
    divider: tokens.borderLight,
  },
  typography: {
    fontFamily: 'Inter, system-ui, Avenir, Helvetica, Arial, sans-serif',
    h1: { fontSize: '2.25rem', fontWeight: 700, lineHeight: 1.2, letterSpacing: '-0.02em' },
    h2: { fontSize: '1.75rem', fontWeight: 700, lineHeight: 1.25, letterSpacing: '-0.02em' },
    h3: { fontSize: '1.375rem', fontWeight: 600, lineHeight: 1.3 },
    h4: { fontSize: '1.125rem', fontWeight: 600, lineHeight: 1.4 },
    h5: { fontSize: '1rem', fontWeight: 600, lineHeight: 1.4 },
    h6: { fontSize: '1rem', fontWeight: 700, lineHeight: 1.4 },
    body1: { fontSize: '1rem', lineHeight: 1.6 },
    body2: { fontSize: '0.875rem', lineHeight: 1.55 },
    button: { fontSize: '0.875rem', fontWeight: 600, letterSpacing: '0.3px', textTransform: 'none' },
  },
  shape: { borderRadius: 10 },
})

export default theme
