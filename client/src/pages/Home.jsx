import { Box, Container, Typography } from '@mui/material'

export default function Home() {
  return (
    <Container maxWidth="md">
      <Box sx={{ py: 8, textAlign: 'center' }}>
        <Typography variant="h3" gutterBottom>
          Tenderquick
        </Typography>
        <Typography variant="body1" color="text.secondary">
          React + Vite frontend wired to an ASP.NET Core API.
        </Typography>
      </Box>
    </Container>
  )
}
