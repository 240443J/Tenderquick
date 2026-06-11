import { Chip } from '@mui/material'
import { statusPalette } from '../../theme'
import { statusMeta } from '../../utils/tenderStatus'

export default function StatusChip({ status, size = 'small' }) {
  const { token, label } = statusMeta(status)
  const colors = statusPalette[token] ?? statusPalette.neutral
  return (
    <Chip
      size={size}
      label={label}
      sx={{
        bgcolor: colors.bg,
        color: colors.main,
        fontWeight: 600,
        border: `1px solid ${colors.main}22`,
      }}
    />
  )
}
