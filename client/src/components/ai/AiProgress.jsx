import { useEffect, useState } from 'react'
import { Box, Typography, LinearProgress } from '@mui/material'
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import { tokens } from '../../theme'

// Animated "AI is working" panel that walks through a list of step labels.
export default function AiProgress({ steps, stepMs = 650 }) {
  const [active, setActive] = useState(0)

  useEffect(() => {
    if (active >= steps.length - 1) return undefined
    const id = setTimeout(() => setActive((a) => a + 1), stepMs)
    return () => clearTimeout(id)
  }, [active, steps.length, stepMs])

  return (
    <Box
      sx={{
        p: 3, borderRadius: 3, border: `1px solid ${tokens.accentIndigoLight}`,
        bgcolor: tokens.accentIndigoSubtle,
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        <AutoAwesomeIcon sx={{ color: tokens.accentIndigo }} />
        <Typography variant="h4" sx={{ color: tokens.accentIndigo }}>
          AI is working…
        </Typography>
      </Box>
      <LinearProgress sx={{ mb: 2.5, borderRadius: 1 }} />
      {steps.map((label, i) => (
        <Box key={label} sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
          {i < active ? (
            <CheckCircleIcon sx={{ fontSize: '1.1rem', color: tokens.statusOnTrack }} />
          ) : (
            <Box
              sx={{
                width: 16, height: 16, borderRadius: '50%',
                border: `2px solid ${i === active ? tokens.accentIndigo : tokens.borderMedium}`,
                ...(i === active && { animation: 'pulse 1s infinite' }),
                '@keyframes pulse': { '0%,100%': { opacity: 1 }, '50%': { opacity: 0.4 } },
              }}
            />
          )}
          <Typography
            variant="body2"
            sx={{
              color: i <= active ? tokens.textPrimary : tokens.textMuted,
              fontWeight: i === active ? 600 : 400,
            }}
          >
            {label}
          </Typography>
        </Box>
      ))}
    </Box>
  )
}
