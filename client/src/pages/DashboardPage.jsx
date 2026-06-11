import { useQuery } from '@tanstack/react-query'
import { Helmet } from 'react-helmet-async'
import {
  Box, Card, CardContent, Container, Divider, List, ListItem, ListItemText, Typography,
} from '@mui/material'
import { getAll } from '../api/tenders'
import { recent as auditRecent } from '../api/audit'
import { useAuth } from '../context/AuthContext'
import { formatDateTime } from '../utils/format'

function CounterCard({ label, value }) {
  return (
    <Card>
      <CardContent>
        <Typography variant="overline" color="text.secondary">{label}</Typography>
        <Typography variant="h3" sx={{ fontWeight: 800 }}>{value}</Typography>
      </CardContent>
    </Card>
  )
}

const CLOSED = ['Won', 'Lost']

export default function DashboardPage() {
  const { user } = useAuth()
  const isAdmin = user?.role === 'Admin'

  const { data: tendersRes } = useQuery({ queryKey: ['tenders'], queryFn: () => getAll() })
  const tenders = tendersRes?.data ?? []

  const { data: auditRes } = useQuery({
    queryKey: ['audit', 'recent'],
    queryFn: () => auditRecent(20),
    enabled: isAdmin,
  })

  const now = Date.now()
  const sevenDays = 7 * 24 * 60 * 60 * 1000
  const counters = {
    active: tenders.filter((t) => !CLOSED.includes(t.status)).length,
    closingSoon: tenders.filter((t) => t.closingAt && new Date(t.closingAt).getTime() - now <= sevenDays && new Date(t.closingAt).getTime() >= now).length,
    drafting: tenders.filter((t) => t.status === 'Drafting').length,
    won: tenders.filter((t) => t.status === 'Won').length,
  }

  const activity = isAdmin
    ? (auditRes?.data ?? []).map((a) => ({ key: a.id, primary: a.action, secondary: `${a.userName} · ${formatDateTime(a.at)}` }))
    : [...tenders]
        .sort((a, b) => b.id - a.id)
        .slice(0, 8)
        .map((t) => ({ key: t.id, primary: `${t.reference} — ${t.title}`, secondary: t.status }))

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Helmet><title>Dashboard · Tenderquick</title></Helmet>
      <Typography variant="h4" gutterBottom>Dashboard</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Welcome back, {user?.name}.
      </Typography>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: 'repeat(2, 1fr)', md: 'repeat(4, 1fr)' }, gap: 2, mb: 3 }}>
        <CounterCard label="Active tenders" value={counters.active} />
        <CounterCard label="Closing soon" value={counters.closingSoon} />
        <CounterCard label="Drafting" value={counters.drafting} />
        <CounterCard label="Won" value={counters.won} />
      </Box>

      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            {isAdmin ? 'Recent activity' : 'Recent tenders'}
          </Typography>
          <Divider />
          {activity.length === 0 ? (
            <Box sx={{ py: 3 }}><Typography variant="body2" color="text.secondary">Nothing yet.</Typography></Box>
          ) : (
            <List dense>
              {activity.map((a) => (
                <ListItem key={a.key} disableGutters>
                  <ListItemText primary={a.primary} secondary={a.secondary} />
                </ListItem>
              ))}
            </List>
          )}
        </CardContent>
      </Card>
    </Container>
  )
}
