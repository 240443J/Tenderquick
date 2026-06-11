import { AppBar, Toolbar, Typography, Box, Chip, Button, Avatar } from '@mui/material'
import { useAuth } from '../../context/AuthContext'

export default function TopBar() {
  const { user, logout } = useAuth()

  return (
    <AppBar
      position="fixed"
      color="inherit"
      elevation={0}
      sx={{ borderBottom: '1px solid', borderColor: 'divider', zIndex: (t) => t.zIndex.drawer + 1 }}
    >
      <Toolbar sx={{ height: 64 }}>
        <Typography variant="h6" color="primary" sx={{ fontWeight: 800, letterSpacing: -0.5 }}>
          Tenderquick
        </Typography>
        <Box sx={{ flexGrow: 1 }} />
        {user && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <Box sx={{ textAlign: 'right', display: { xs: 'none', sm: 'block' } }}>
              <Typography variant="body2" sx={{ fontWeight: 600, lineHeight: 1.2 }}>{user.name}</Typography>
              <Chip size="small" label={user.role} sx={{ height: 18, fontSize: 11 }} />
            </Box>
            <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main', fontSize: 14 }}>
              {user.name?.[0]?.toUpperCase()}
            </Avatar>
            <Button size="small" onClick={logout}>Logout</Button>
          </Box>
        )}
      </Toolbar>
    </AppBar>
  )
}
