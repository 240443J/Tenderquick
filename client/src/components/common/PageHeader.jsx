import { Box, Typography, Chip } from '@mui/material'
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome'
import { tokens } from '../../theme'

export default function PageHeader({ title, subtitle, action, ai = false }) {
  return (
    <Box
      sx={{
        display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start',
        gap: 2, mb: 3, flexWrap: 'wrap',
      }}
    >
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25, mb: 0.5 }}>
          <Typography variant="h1">{title}</Typography>
          {ai && (
            <Chip
              icon={<AutoAwesomeIcon sx={{ fontSize: '0.95rem !important' }} />}
              label="AI"
              size="small"
              sx={{
                fontWeight: 800, letterSpacing: '0.04em',
                bgcolor: tokens.accentIndigoSubtle, color: tokens.accentIndigo,
              }}
            />
          )}
        </Box>
        {subtitle && (
          <Typography variant="body2" sx={{ color: tokens.textSecondary }}>
            {subtitle}
          </Typography>
        )}
      </Box>
      {action}
    </Box>
  )
}
