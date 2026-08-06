import { useLocation, useNavigate } from 'react-router-dom'
import {
  Drawer, List, ListItemButton, ListItemIcon, ListItemText, Toolbar, Box,
} from '@mui/material'
import DashboardIcon from '@mui/icons-material/Dashboard'
import DescriptionIcon from '@mui/icons-material/Description'
import SearchIcon from '@mui/icons-material/Search'
import Inventory2Icon from '@mui/icons-material/Inventory2'
import RequestQuoteIcon from '@mui/icons-material/RequestQuote'
import EventIcon from '@mui/icons-material/Event'
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome'
import TravelExploreIcon from '@mui/icons-material/TravelExplore'
import { useAuth } from '../../context/AuthContext'

const WIDTH = 240

const EDITORS = ['Admin', 'Estimator']

const NAV = [
  { label: 'Dashboard', icon: <DashboardIcon />, to: '/' },
  { label: 'Tenders', icon: <DescriptionIcon />, to: '/tenders' },
  { label: 'Tender Search', icon: <SearchIcon />, to: '/search', allow: EDITORS },
  { label: 'Discover', icon: <TravelExploreIcon />, to: '/scraper', allow: EDITORS },
  { label: 'Deadlines', icon: <EventIcon />, to: '/deadlines' },
  { label: 'Inventory', icon: <Inventory2Icon />, to: '/inventory' },
  { label: 'Quotations', icon: <RequestQuoteIcon />, to: '/quotations' },
  { label: 'AI Drafting', icon: <AutoAwesomeIcon />, to: '/drafting', allow: EDITORS },
]

export default function SideNav() {
  const navigate = useNavigate()
  const { pathname } = useLocation()
  const { user } = useAuth()

  const isActive = (to) => (to === '/' ? pathname === '/' : pathname.startsWith(to))
  const navItems = NAV.filter((item) => !item.allow || item.allow.includes(user?.role))

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: WIDTH,
        flexShrink: 0,
        '& .MuiDrawer-paper': { width: WIDTH, boxSizing: 'border-box', borderRight: '1px solid', borderColor: 'divider' },
      }}
    >
      <Toolbar />
      <Box sx={{ overflow: 'auto', py: 1 }}>
        <List>
          {navItems.map((item) => (
            <ListItemButton
              key={item.to}
              selected={isActive(item.to)}
              onClick={() => navigate(item.to)}
              sx={{ mx: 1, borderRadius: 2, mb: 0.5 }}
            >
              <ListItemIcon sx={{ minWidth: 40 }}>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItemButton>
          ))}
        </List>
      </Box>
    </Drawer>
  )
}
