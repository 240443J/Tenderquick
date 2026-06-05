import { Box, Typography, Button } from '@mui/material'
import { useNavigate } from 'react-router-dom'

export default function NotFoundPage() {
  const navigate = useNavigate()
  return (
    <Box sx={{ textAlign: 'center', py: 10 }}>
      <Typography variant="h1" sx={{ mb: 1 }}>404</Typography>
      <Typography variant="body1" sx={{ mb: 3 }}>This page does not exist.</Typography>
      <Button variant="contained" onClick={() => navigate('/')}>Go to dashboard</Button>
    </Box>
  )
}
