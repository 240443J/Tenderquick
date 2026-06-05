import { Box, Typography } from '@mui/material'
import { tokens } from '../../theme'

export default function EmptyState({ title, description, action, icon }) {
  return (
    <Box
      sx={{
        textAlign: 'center',
        py: 8,
        px: 3,
        border: `1px dashed ${tokens.borderMedium}`,
        borderRadius: 2,
        bgcolor: tokens.offWhite,
      }}
    >
      {icon && <Box sx={{ color: tokens.textMuted, mb: 1.5 }}>{icon}</Box>}
      <Typography variant="h4" sx={{ mb: 1 }}>{title}</Typography>
      {description && (
        <Typography variant="body2" sx={{ color: tokens.textSecondary, mb: action ? 3 : 0 }}>
          {description}
        </Typography>
      )}
      {action}
    </Box>
  )
}
