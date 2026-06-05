import { Box, Toolbar } from '@mui/material'
import { Outlet } from 'react-router-dom'
import SideNav from './SideNav'
import TopBar from './TopBar'
import { tokens } from '../../theme'

const NAV_WIDTH = 240

export default function AppShell() {
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: tokens.lightGray }}>
      <SideNav />
      <TopBar />
      <Box
        component="main"
        sx={{ flexGrow: 1, width: { md: `calc(100% - ${NAV_WIDTH}px)` }, minWidth: 0 }}
      >
        <Toolbar sx={{ minHeight: 64 }} />
        <Box sx={{ p: { xs: 2, md: 4 } }}>
          <Outlet />
        </Box>
      </Box>
    </Box>
  )
}
