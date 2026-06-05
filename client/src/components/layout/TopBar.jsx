import { AppBar, Toolbar, Typography, Box, Button, Chip } from '@mui/material'
import LogoutIcon from '@mui/icons-material/Logout'
import { useAuth } from '../../context/AuthContext'
import { tokens } from '../../theme'

const NAV_WIDTH = 240

export default function TopBar() {
  const { user, logout } = useAuth()

  return (
    <AppBar
      position="fixed"
      elevation={0}
      sx={{
        width: { md: `calc(100% - ${NAV_WIDTH}px)` },
        ml: { md: `${NAV_WIDTH}px` },
        bgcolor: tokens.white,
        color: tokens.textPrimary,
        borderBottom: `1px solid ${tokens.borderLight}`,
      }}
    >
      <Toolbar sx={{ minHeight: 64, justifyContent: 'flex-end', gap: 2 }}>
        {user && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <Box sx={{ textAlign: 'right' }}>
              <Typography variant="body2" sx={{ fontWeight: 600, lineHeight: 1.2 }}>
                {user.name}
              </Typography>
              <Typography variant="caption" sx={{ color: tokens.textMuted }}>
                {user.email}
              </Typography>
            </Box>
            <Chip
              label={user.role}
              size="small"
              sx={{ bgcolor: tokens.accentIndigoSubtle, color: tokens.accentIndigo, fontWeight: 700 }}
            />
            <Button
              size="small"
              startIcon={<LogoutIcon />}
              onClick={logout}
              sx={{ color: tokens.textSecondary }}
            >
              Logout
            </Button>
          </Box>
        )}
      </Toolbar>
    </AppBar>
  )
}
