import { Box } from '@mui/material'
import { tenderStatusStyle } from '../../utils/tenderStatus'

export default function StatusChip({ status }) {
  const { color, bg, label } = tenderStatusStyle(status)
  return (
    <Box
      component="span"
      sx={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 0.75,
        px: 1,
        py: 0.25,
        borderRadius: 1,
        bgcolor: bg,
        color,
        fontSize: '0.7rem',
        fontWeight: 700,
        letterSpacing: '0.06em',
        textTransform: 'uppercase',
        lineHeight: 1.6,
        whiteSpace: 'nowrap',
      }}
    >
      <Box component="span" sx={{ width: 6, height: 6, borderRadius: '50%', bgcolor: color }} />
      {label}
    </Box>
  )
}
