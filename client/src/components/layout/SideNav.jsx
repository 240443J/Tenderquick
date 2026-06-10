import {
  Drawer, List, ListItemButton, ListItemIcon, ListItemText, Box, Typography, Divider, Chip,
} from '@mui/material'
import DashboardIcon from '@mui/icons-material/SpaceDashboard'
import DescriptionIcon from '@mui/icons-material/Description'
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome'
import TravelExploreIcon from '@mui/icons-material/TravelExplore'
import RequestQuoteIcon from '@mui/icons-material/RequestQuote'
import Inventory2Icon from '@mui/icons-material/Inventory2'
import EventAvailableIcon from '@mui/icons-material/EventAvailable'
import { useLocation, useNavigate } from 'react-router-dom'
import { tokens } from '../../theme'

const NAV_WIDTH = 240

const items = [
  { label: 'Dashboard', icon: <DashboardIcon />, path: '/' },
  { label: 'Tenders', icon: <DescriptionIcon />, path: '/tenders' },
  { label: 'AI Drafting', icon: <AutoAwesomeIcon />, path: '/drafting', ai: true },
  { label: 'Keyword Scraper', icon: <TravelExploreIcon />, path: '/scraper', ai: true },
  { label: 'Quotations', icon: <RequestQuoteIcon />, path: '/quotations', ai: true },
  { label: 'Inventory', icon: <Inventory2Icon />, path: '/inventory' },
  { label: 'Deadlines', icon: <EventAvailableIcon />, path: '/deadlines' },
]

export default function SideNav() {
  const navigate = useNavigate()
  const location = useLocation()

  const isActive = (path) =>
    path === '/' ? location.pathname === '/' : location.pathname.startsWith(path)

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: NAV_WIDTH,
        flexShrink: 0,
        display: { xs: 'none', md: 'block' },
        '& .MuiDrawer-paper': {
          width: NAV_WIDTH,
          boxSizing: 'border-box',
          bgcolor: tokens.navy,
          color: tokens.textOnDark,
          borderRight: 'none',
        },
      }}
    >
      <Box sx={{ px: 3, py: 2.5, display: 'flex', alignItems: 'center', gap: 1 }}>
        <Box
          sx={{
            width: 34, height: 34, borderRadius: 2, bgcolor: tokens.accentIndigo,
            display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0,
          }}
        >
          <Typography sx={{ color: '#fff', fontWeight: 900, fontSize: '1.1rem' }}>T</Typography>
        </Box>
        <Box>
          <Typography variant="h4" sx={{ color: tokens.textOnDark, fontWeight: 800, lineHeight: 1.1 }}>
            TenderQuick
          </Typography>
          <Typography variant="caption" sx={{ color: tokens.textOnDarkMuted }}>
            AI tender workspace
          </Typography>
        </Box>
      </Box>

      <List sx={{ px: 1.5 }}>
        {items.map((item) => (
          <ListItemButton
            key={item.path}
            selected={isActive(item.path)}
            onClick={() => navigate(item.path)}
            sx={{
              borderRadius: 2,
              mb: 0.5,
              color: tokens.textOnDarkSubtle,
              '& .MuiListItemIcon-root': { color: 'inherit', minWidth: 38 },
              '&.Mui-selected': {
                bgcolor: tokens.accentIndigo,
                color: tokens.textOnDark,
                '&:hover': { bgcolor: tokens.accentIndigoHover },
              },
              '&:hover': { bgcolor: tokens.slate, color: tokens.textOnDark },
            }}
          >
            <ListItemIcon>{item.icon}</ListItemIcon>
            <ListItemText primaryTypographyProps={{ fontWeight: 600, fontSize: '0.9rem' }}>
              {item.label}
            </ListItemText>
            {item.ai && (
              <Chip
                label="AI"
                size="small"
                sx={{
                  height: 18, fontSize: '0.6rem', fontWeight: 800, letterSpacing: '0.05em',
                  bgcolor: 'rgba(255,255,255,0.14)', color: '#fff',
                  '& .MuiChip-label': { px: 0.75 },
                }}
              />
            )}
          </ListItemButton>
        ))}
      </List>

      <Divider sx={{ borderColor: tokens.slate, mx: 2, my: 1.5 }} />
      <Box sx={{ px: 3 }}>
        <Typography variant="caption" sx={{ color: tokens.textOnDarkMuted, display: 'block' }}>
          Prototype build · mock data
        </Typography>
      </Box>
    </Drawer>
  )
}
