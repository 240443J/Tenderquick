import { Box, Toolbar } from '@mui/material'
import { Outlet } from 'react-router-dom'
import TopBar from './TopBar'
import SideNav from './SideNav'

export default function AppShell() {
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <TopBar />
      <SideNav />
      <Box component="main" sx={{ flexGrow: 1, bgcolor: 'background.default', minWidth: 0 }}>
        <Toolbar />
        <Outlet />
      </Box>
    </Box>
  )
}
