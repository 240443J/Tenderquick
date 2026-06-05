import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  Box, Typography, Paper, CircularProgress, List, ListItem, ListItemText, Divider,
} from '@mui/material'
import * as tendersApi from '../api/tenders'
import * as auditApi from '../api/audit'
import { useAuth } from '../context/AuthContext'
import { daysUntil, formatDateTime } from '../utils/format'
import { tokens } from '../theme'

function CounterCard({ label, value, accent, onClick }) {
  return (
    <Paper
      onClick={onClick}
      sx={{
        p: 3, borderRadius: 3, flex: 1, minWidth: 180, cursor: onClick ? 'pointer' : 'default',
        borderLeft: `4px solid ${accent}`,
        transition: 'box-shadow .15s', '&:hover': onClick ? { boxShadow: 3 } : {},
      }}
    >
      <Typography variant="h1" sx={{ color: accent }}>{value}</Typography>
      <Typography variant="body2" sx={{ color: tokens.textSecondary }}>{label}</Typography>
    </Paper>
  )
}

export default function DashboardPage() {
  const navigate = useNavigate()
  const { user } = useAuth()
  const isAdmin = user?.role === 'Admin'

  const { data: tenders, isLoading } = useQuery({
    queryKey: ['tenders', '', ''],
    queryFn: () => tendersApi.getAll().then((r) => r.data),
  })

  const { data: audit } = useQuery({
    queryKey: ['audit'],
    queryFn: () => auditApi.getRecent(8).then((r) => r.data),
    enabled: isAdmin,
  })

  if (isLoading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
  }

  const list = tenders || []
  const active = list.filter((t) => t.status !== 'Won' && t.status !== 'Lost').length
  const closingSoon = list.filter((t) => {
    const d = daysUntil(t.closingAt)
    return d !== null && d >= 0 && d <= 7 && t.status !== 'Won' && t.status !== 'Lost'
  }).length
  const drafting = list.filter((t) => t.status === 'Drafting').length
  const won = list.filter((t) => t.status === 'Won').length

  return (
    <Box>
      <Typography variant="h1" sx={{ mb: 0.5 }}>Dashboard</Typography>
      <Typography variant="body2" sx={{ color: tokens.textSecondary, mb: 3 }}>
        Welcome back, {user?.name}.
      </Typography>

      <Box sx={{ display: 'flex', gap: 2.5, flexWrap: 'wrap', mb: 4 }}>
        <CounterCard label="Active tenders" value={active} accent={tokens.accentIndigo} onClick={() => navigate('/tenders')} />
        <CounterCard label="Closing within 7 days" value={closingSoon} accent={tokens.statusUrgent} onClick={() => navigate('/tenders')} />
        <CounterCard label="Drafting" value={drafting} accent={tokens.statusDraft} onClick={() => navigate('/tenders?status=Drafting')} />
        <CounterCard label="Won" value={won} accent={tokens.statusOnTrack} />
      </Box>

      {isAdmin && (
        <Paper sx={{ p: 3, borderRadius: 3, maxWidth: 640 }}>
          <Typography variant="h4" sx={{ mb: 1 }}>Recent activity</Typography>
          {!audit || audit.length === 0 ? (
            <Typography variant="body2" sx={{ color: tokens.textMuted }}>No activity yet.</Typography>
          ) : (
            <List disablePadding>
              {audit.map((row, i) => (
                <Box key={row.id}>
                  {i > 0 && <Divider />}
                  <ListItem disableGutters sx={{ py: 1 }}>
                    <ListItemText
                      primary={`${row.action} — ${row.userName}`}
                      secondary={formatDateTime(row.at)}
                      primaryTypographyProps={{ fontSize: '0.9rem', fontWeight: 500 }}
                      secondaryTypographyProps={{ fontSize: '0.78rem' }}
                    />
                  </ListItem>
                </Box>
              ))}
            </List>
          )}
        </Paper>
      )}
    </Box>
  )
}
