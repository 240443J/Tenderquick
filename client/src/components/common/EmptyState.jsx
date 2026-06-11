import { Box, Typography } from '@mui/material'

export default function EmptyState({ icon, title, message, action }) {
  return (
    <Box sx={{ textAlign: 'center', py: 8, px: 2, color: 'text.secondary' }}>
      {icon && <Box sx={{ fontSize: 48, mb: 1, '& svg': { fontSize: 48 } }}>{icon}</Box>}
      <Typography variant="h6" gutterBottom color="text.primary">{title}</Typography>
      {message && <Typography variant="body2" sx={{ mb: 2 }}>{message}</Typography>}
      {action}
    </Box>
  )
}
