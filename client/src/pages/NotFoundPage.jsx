import { Link as RouterLink } from 'react-router-dom'
import { Box, Button, Typography } from '@mui/material'

export default function NotFoundPage() {
  return (
    <Box sx={{ textAlign: 'center', py: 12 }}>
      <Typography variant="h2" sx={{ fontWeight: 800 }}>404</Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
        We couldn’t find that page.
      </Typography>
      <Button variant="contained" component={RouterLink} to="/">Back to dashboard</Button>
    </Box>
  )
}
