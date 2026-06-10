import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  Box, Typography, Paper, CircularProgress, Divider, Button, Chip,
} from '@mui/material'
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome'
import TravelExploreIcon from '@mui/icons-material/TravelExplore'
import RequestQuoteIcon from '@mui/icons-material/RequestQuote'
import EventAvailableIcon from '@mui/icons-material/EventAvailable'
import * as tendersApi from '../api/tenders'
import * as auditApi from '../api/audit'
import * as deadlinesApi from '../api/deadlines'
import * as quotationsApi from '../api/quotations'
import * as draftsApi from '../api/drafts'
import { useAuth } from '../context/AuthContext'
import { daysUntil, formatDateTime, formatDate } from '../utils/format'
import { deadlineTone } from '../utils/deadline'
import { tokens } from '../theme'

function CounterCard({ label, value, accent, sub, onClick }) {
  return (
    <Paper
      onClick={onClick}
      sx={{
        p: 2.5, borderRadius: 3, flex: 1, minWidth: 170, cursor: onClick ? 'pointer' : 'default',
        borderTop: `3px solid ${accent}`,
        transition: 'box-shadow .15s, transform .15s',
        '&:hover': onClick ? { boxShadow: 4, transform: 'translateY(-2px)' } : {},
      }}
    >
      <Typography sx={{ fontSize: '2.2rem', fontWeight: 800, color: accent, lineHeight: 1.1 }}>
        {value}
      </Typography>
      <Typography variant="body2" sx={{ color: tokens.textPrimary, fontWeight: 600 }}>{label}</Typography>
      {sub && <Typography variant="caption" sx={{ color: tokens.textMuted }}>{sub}</Typography>}
    </Paper>
  )
}

function QuickAction({ icon, label, desc, onClick }) {
  return (
    <Paper
      onClick={onClick}
      sx={{
        p: 2, borderRadius: 3, flex: 1, minWidth: 200, cursor: 'pointer', display: 'flex',
        gap: 1.5, alignItems: 'flex-start',
        transition: 'box-shadow .15s', '&:hover': { boxShadow: 3, borderColor: tokens.accentIndigo },
        border: `1px solid ${tokens.borderLight}`,
      }}
    >
      <Box
        sx={{
          width: 38, height: 38, borderRadius: 2, flexShrink: 0,
          bgcolor: tokens.accentIndigoSubtle, color: tokens.accentIndigo,
          display: 'flex', alignItems: 'center', justifyContent: 'center',
        }}
      >
        {icon}
      </Box>
      <Box>
        <Typography variant="body2" sx={{ fontWeight: 700 }}>{label}</Typography>
        <Typography variant="caption" sx={{ color: tokens.textMuted }}>{desc}</Typography>
      </Box>
    </Paper>
  )
}

export default function DashboardPage() {
  const navigate = useNavigate()
  const { user } = useAuth()

  const { data: tenders, isLoading } = useQuery({
    queryKey: ['tenders', '', ''],
    queryFn: () => tendersApi.getAll().then((r) => r.data),
  })
  const { data: deadlines } = useQuery({
    queryKey: ['deadlines'],
    queryFn: () => deadlinesApi.getAll().then((r) => r.data),
  })
  const { data: quotes } = useQuery({
    queryKey: ['quotations'],
    queryFn: () => quotationsApi.getAll().then((r) => r.data),
  })
  const { data: drafts } = useQuery({
    queryKey: ['drafts'],
    queryFn: () => draftsApi.getAll().then((r) => r.data),
  })
  const { data: audit } = useQuery({
    queryKey: ['audit'],
    queryFn: () => auditApi.getRecent(6).then((r) => r.data),
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
  const pendingQuotes = (quotes || []).filter((q) => !q.verified).length
  const draftsInProgress = (drafts || []).filter((d) => d.status !== 'Final').length

  const upcoming = (deadlines || [])
    .filter((d) => daysUntil(d.dueAt) >= -3)
    .slice(0, 5)

  return (
    <Box>
      <Typography variant="h1" sx={{ mb: 0.5 }}>Welcome back, {user?.name?.split(' ')[0]}</Typography>
      <Typography variant="body2" sx={{ color: tokens.textSecondary, mb: 3 }}>
        Here’s what needs your attention across TenderQuick today.
      </Typography>

      <Box sx={{ display: 'flex', gap: 2.5, flexWrap: 'wrap', mb: 3 }}>
        <CounterCard label="Active tenders" value={active} accent={tokens.accentIndigo} onClick={() => navigate('/tenders')} />
        <CounterCard label="Closing ≤ 7 days" value={closingSoon} sub="Act soon" accent={tokens.statusUrgent} onClick={() => navigate('/deadlines')} />
        <CounterCard label="Quotes to verify" value={pendingQuotes} sub="Needs human check" accent={tokens.statusSoon} onClick={() => navigate('/quotations')} />
        <CounterCard label="Drafts in progress" value={draftsInProgress} accent={tokens.statusDraft} onClick={() => navigate('/drafting')} />
      </Box>

      <Typography variant="h4" sx={{ mb: 1.5 }}>Quick start</Typography>
      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mb: 4 }}>
        <QuickAction icon={<TravelExploreIcon />} label="Find new tenders" desc="Scrape GeBiz by keyword" onClick={() => navigate('/scraper')} />
        <QuickAction icon={<AutoAwesomeIcon />} label="Draft a proposal" desc="AI writes from specs" onClick={() => navigate('/drafting')} />
        <QuickAction icon={<RequestQuoteIcon />} label="Generate a quote" desc="Priced from inventory" onClick={() => navigate('/quotations')} />
        <QuickAction icon={<EventAvailableIcon />} label="Track deadlines" desc="Sync to Google Calendar" onClick={() => navigate('/deadlines')} />
      </Box>

      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', alignItems: 'flex-start' }}>
        <Paper sx={{ p: 3, borderRadius: 3, flex: 1, minWidth: 320 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1.5 }}>
            <Typography variant="h4">Upcoming deadlines</Typography>
            <Button size="small" onClick={() => navigate('/deadlines')}>View all</Button>
          </Box>
          {upcoming.length === 0 ? (
            <Typography variant="body2" sx={{ color: tokens.textMuted }}>Nothing due soon.</Typography>
          ) : upcoming.map((d, i) => {
            const tone = deadlineTone(d.dueAt)
            return (
              <Box key={d.id}>
                {i > 0 && <Divider />}
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 1.25, gap: 2 }}>
                  <Box sx={{ minWidth: 0 }}>
                    <Typography variant="body2" sx={{ fontWeight: 600, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {d.title}
                    </Typography>
                    <Typography variant="caption" sx={{ color: tokens.textMuted }}>{formatDate(d.dueAt)}</Typography>
                  </Box>
                  <Chip label={tone.label} size="small" sx={{ bgcolor: tone.bg, color: tone.color, fontWeight: 700, flexShrink: 0 }} />
                </Box>
              </Box>
            )
          })}
        </Paper>

        <Paper sx={{ p: 3, borderRadius: 3, flex: 1, minWidth: 320 }}>
          <Typography variant="h4" sx={{ mb: 1.5 }}>Recent activity</Typography>
          {!audit || audit.length === 0 ? (
            <Typography variant="body2" sx={{ color: tokens.textMuted }}>No activity yet.</Typography>
          ) : audit.map((row, i) => (
            <Box key={row.id}>
              {i > 0 && <Divider />}
              <Box sx={{ display: 'flex', justifyContent: 'space-between', py: 1.1, gap: 2 }}>
                <Box>
                  <Typography variant="body2" sx={{ fontWeight: 500 }}>{row.action}</Typography>
                  <Typography variant="caption" sx={{ color: tokens.textMuted }}>
                    {row.entity} · {row.user}
                  </Typography>
                </Box>
                <Typography variant="caption" sx={{ color: tokens.textMuted, whiteSpace: 'nowrap' }}>
                  {formatDateTime(row.at)}
                </Typography>
              </Box>
            </Box>
          ))}
        </Paper>
      </Box>
    </Box>
  )
}
