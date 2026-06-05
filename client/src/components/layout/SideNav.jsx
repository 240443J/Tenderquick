import {
  Drawer, List, ListItemButton, ListItemIcon, ListItemText, Box, Typography, Divider,
} from '@mui/material'
import DashboardIcon from '@mui/icons-material/SpaceDashboard'
import DescriptionIcon from '@mui/icons-material/Description'
import Inventory2Icon from '@mui/icons-material/Inventory2'
import RequestQuoteIcon from '@mui/icons-material/RequestQuote'
import ArticleIcon from '@mui/icons-material/Article'
import { useLocation, useNavigate } from 'react-router-dom'
import { tokens } from '../../theme'

const NAV_WIDTH = 240

const items = [
  { label: 'Dashboard', icon: <DashboardIcon />, path: '/' },
  { label: 'Tenders', icon: <DescriptionIcon />, path: '/tenders' },
]

const comingSoon = [
  { label: 'Inventory', icon: <Inventory2Icon /> },
  { label: 'Quotations', icon: <RequestQuoteIcon /> },
  { label: 'Documents', icon: <ArticleIcon /> },
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
      <Box sx={{ px: 3, py: 2.5 }}>
        <Typography variant="h3" sx={{ color: tokens.textOnDark, fontWeight: 800 }}>
          Tenderquick
        </Typography>
        <Typography variant="caption" sx={{ color: tokens.textOnDarkMuted }}>
          Tender lifecycle workspace
        </Typography>
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
          </ListItemButton>
        ))}
      </List>

      <Divider sx={{ borderColor: tokens.slate, mx: 2, my: 1 }} />
      <Typography variant="caption" sx={{ color: tokens.textOnDarkMuted, px: 3 }}>
        Coming soon
      </Typography>
      <List sx={{ px: 1.5 }}>
        {comingSoon.map((item) => (
          <ListItemButton
            key={item.label}
            disabled
            sx={{ borderRadius: 2, mb: 0.5, color: tokens.textOnDarkMuted, opacity: 0.5,
              '& .MuiListItemIcon-root': { color: 'inherit', minWidth: 38 } }}
          >
            <ListItemIcon>{item.icon}</ListItemIcon>
            <ListItemText primaryTypographyProps={{ fontSize: '0.9rem' }}>{item.label}</ListItemText>
          </ListItemButton>
        ))}
      </List>
    </Drawer>
  )
}
