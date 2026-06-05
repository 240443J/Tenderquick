import { createTheme } from '@mui/material/styles'

export const tokens = {
  // Surfaces
  white: '#FFFFFF',
  offWhite: '#FAFAFA',
  lightGray: '#F3F4F6',
  borderLight: '#E5E7EB',
  borderMedium: '#D1D5DB',
  // Dark
  navy: '#0F172A',
  darkCharcoal: '#1E293B',
  slate: '#334155',
  // Brand accent
  accentIndigo: '#4F46E5',
  accentIndigoHover: '#4338CA',
  accentIndigoLight: '#E0E7FF',
  accentIndigoSubtle: '#EEF2FF',
  // Status
  statusOverdue: '#DC2626',
  statusUrgent: '#EA580C',
  statusSoon: '#CA8A04',
  statusOnTrack: '#16A34A',
  statusNeutral: '#6B7280',
  statusDraft: '#7C3AED',
  // Status tints (chip / row backgrounds)
  statusOverdueBg: '#FEE2E2',
  statusUrgentBg: '#FFEDD5',
  statusSoonBg: '#FEF9C3',
  statusOnTrackBg: '#DCFCE7',
  statusNeutralBg: '#F3F4F6',
  statusDraftBg: '#EDE9FE',
  // Text on light
  textPrimary: '#111827',
  textSecondary: '#6B7280',
  textMuted: '#9CA3AF',
  // Text on dark
  textOnDark: '#FFFFFF',
  textOnDarkMuted: 'rgba(255,255,255,0.6)',
  textOnDarkSubtle: 'rgba(255,255,255,0.7)',
}

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: tokens.accentIndigo, dark: tokens.accentIndigoHover },
    background: { default: tokens.lightGray, paper: tokens.white },
    text: { primary: tokens.textPrimary, secondary: tokens.textSecondary },
    error: { main: tokens.statusOverdue },
    warning: { main: tokens.statusSoon },
    success: { main: tokens.statusOnTrack },
    divider: tokens.borderLight,
  },
  typography: {
    fontFamily: '"Inter", -apple-system, BlinkMacSystemFont, "system-ui", sans-serif',
    h1: { fontSize: '2.25rem', fontWeight: 700, lineHeight: 1.2, letterSpacing: '-0.02em' },
    h2: { fontSize: '1.75rem', fontWeight: 700, lineHeight: 1.25, letterSpacing: '-0.02em' },
    h3: { fontSize: '1.375rem', fontWeight: 600, lineHeight: 1.3 },
    h4: { fontSize: '1.125rem', fontWeight: 600, lineHeight: 1.4 },
    body1: { fontSize: '1rem', lineHeight: 1.6 },
    body2: { fontSize: '0.875rem', lineHeight: 1.55 },
    button: { fontSize: '0.875rem', fontWeight: 600, letterSpacing: '0.3px', textTransform: 'none' },
  },
  shape: { borderRadius: 12 },
  components: {
    MuiButton: {
      styleOverrides: {
        root: { borderRadius: 8, boxShadow: 'none' },
        contained: { ':hover': { boxShadow: 'none' } },
      },
    },
    MuiPaper: { styleOverrides: { root: { backgroundImage: 'none' } } },
  },
})

// Monospace tabular style for money / quantities / references.
export const monoSx = {
  fontVariantNumeric: 'tabular-nums',
  fontWeight: 500,
}

export default theme
