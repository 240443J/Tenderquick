import { createTheme } from '@mui/material/styles'

// Status palette — drives StatusChip and any status-coloured UI. `*Bg` are tint backgrounds.
export const statusPalette = {
  overdue: { main: '#b3261e', bg: '#fbeae9' },
  urgent: { main: '#c2410c', bg: '#fdf0e7' },
  soon: { main: '#a16207', bg: '#fdf6e3' },
  onTrack: { main: '#15803d', bg: '#e9f6ee' },
  draft: { main: '#475569', bg: '#eef1f5' },
  neutral: { main: '#334155', bg: '#eef1f5' },
}

const theme = createTheme({
  palette: {
    mode: 'light',
    primary: { main: '#4f46e5' }, // indigo brand
    secondary: { main: '#0ea5e9' },
    background: { default: '#f7f8fa', paper: '#ffffff' },
  },
  typography: {
    fontFamily: 'Inter, system-ui, Avenir, Helvetica, Arial, sans-serif',
    h4: { fontWeight: 700 },
    h6: { fontWeight: 700 },
  },
  shape: { borderRadius: 10 },
})

export default theme
