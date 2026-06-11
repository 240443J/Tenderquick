import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Paper, Typography, Button, Chip, CircularProgress, Divider, Snackbar, Alert,
} from '@mui/material'
import EventAvailableIcon from '@mui/icons-material/EventAvailable'
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import LinkOffIcon from '@mui/icons-material/LinkOff'
import GroupsIcon from '@mui/icons-material/Groups'
import HelpOutlineIcon from '@mui/icons-material/HelpOutlineOutlined'
import FlagIcon from '@mui/icons-material/Flag'
import * as deadlinesApi from '../api/deadlines'
import PageHeader from '../components/common/PageHeader'
import { tokens } from '../theme'
import { formatDateTime } from '../utils/format'
import { deadlineTone } from '../utils/deadline'

const typeIcon = {
  Closing: <FlagIcon fontSize="small" />,
  Briefing: <GroupsIcon fontSize="small" />,
  Clarification: <HelpOutlineIcon fontSize="small" />,
  Submission: <EventAvailableIcon fontSize="small" />,
}

export default function DeadlinesPage() {
  const queryClient = useQueryClient()
  const [toast, setToast] = useState('')

  const { data: deadlines, isLoading } = useQuery({
    queryKey: ['deadlines'],
    queryFn: () => deadlinesApi.getAll().then((r) => r.data),
  })
  const { data: calendar } = useQuery({
    queryKey: ['calendar'],
    queryFn: () => deadlinesApi.getCalendar().then((r) => r.data),
  })

  const connectMutation = useMutation({
    mutationFn: () => deadlinesApi.connectCalendar(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['calendar'] })
      setToast('Google Calendar connected.')
    },
  })
  const disconnectMutation = useMutation({
    mutationFn: () => deadlinesApi.disconnectCalendar(),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['calendar'] }),
  })
  const addMutation = useMutation({
    mutationFn: (dId) => deadlinesApi.addToCalendar(dId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deadlines'] })
      setToast('Event added to Google Calendar.')
    },
  })
  const syncAllMutation = useMutation({
    mutationFn: () => deadlinesApi.syncAllToCalendar(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['deadlines'] })
      setToast('All deadlines synced to Google Calendar.')
    },
  })

  const connected = calendar?.connected
  const list = deadlines || []
  const notSynced = list.filter((d) => !d.addedToCalendar).length

  return (
    <Box>
      <PageHeader
        ai
        title="Deadline Tracker"
        subtitle="AI tracks every tender milestone and pushes them straight into your Google Calendar with reminders."
      />

      {/* Calendar connection card */}
      <Paper
        sx={{
          p: 3, borderRadius: 3, mb: 3,
          display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 2, flexWrap: 'wrap',
          border: `1px solid ${connected ? tokens.statusOnTrackBg : tokens.borderLight}`,
          bgcolor: connected ? tokens.statusOnTrackBg : tokens.white,
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Box
            sx={{
              width: 46, height: 46, borderRadius: 2,
              bgcolor: connected ? tokens.statusOnTrack : tokens.accentIndigoSubtle,
              color: connected ? '#fff' : tokens.accentIndigo,
              display: 'flex', alignItems: 'center', justifyContent: 'center',
            }}
          >
            <CalendarMonthIcon />
          </Box>
          <Box>
            <Typography variant="h4">Google Calendar</Typography>
            <Typography variant="body2" sx={{ color: tokens.textSecondary }}>
              {connected ? `Connected as ${calendar.account}` : 'Not connected — link your account to sync deadlines.'}
            </Typography>
          </Box>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {connected ? (
            <>
              <Button
                variant="contained" startIcon={<EventAvailableIcon />}
                disabled={syncAllMutation.isPending || notSynced === 0}
                onClick={() => syncAllMutation.mutate()}
              >
                {notSynced === 0 ? 'All synced' : `Sync all (${notSynced})`}
              </Button>
              <Button variant="outlined" startIcon={<LinkOffIcon />} onClick={() => disconnectMutation.mutate()}>
                Disconnect
              </Button>
            </>
          ) : (
            <Button
              variant="contained" startIcon={<CalendarMonthIcon />}
              disabled={connectMutation.isPending}
              onClick={() => connectMutation.mutate()}
            >
              {connectMutation.isPending ? 'Connecting…' : 'Connect Google Calendar'}
            </Button>
          )}
        </Box>
      </Paper>

      <Typography variant="h4" sx={{ mb: 1.5 }}>Tracked deadlines</Typography>

      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
      ) : (
        <Paper sx={{ borderRadius: 3, overflow: 'hidden' }}>
          {list.map((d, i) => {
            const tone = deadlineTone(d.dueAt)
            return (
              <Box key={d.id}>
                {i > 0 && <Divider />}
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, px: 2.5, py: 2, flexWrap: 'wrap' }}>
                  <Box
                    sx={{
                      width: 38, height: 38, borderRadius: 2, flexShrink: 0,
                      bgcolor: tone.bg, color: tone.color,
                      display: 'flex', alignItems: 'center', justifyContent: 'center',
                    }}
                  >
                    {typeIcon[d.type] || <EventAvailableIcon fontSize="small" />}
                  </Box>
                  <Box sx={{ flex: 1, minWidth: 200 }}>
                    <Typography variant="body1" sx={{ fontWeight: 600 }}>{d.title}</Typography>
                    <Typography variant="caption" sx={{ color: tokens.textSecondary }}>
                      {d.tenderRef} · {d.type} · {formatDateTime(d.dueAt)}
                    </Typography>
                  </Box>
                  <Chip label={tone.label} size="small" sx={{ bgcolor: tone.bg, color: tone.color, fontWeight: 700 }} />
                  {d.addedToCalendar ? (
                    <Chip icon={<CheckCircleIcon />} label="In calendar" size="small" variant="outlined" sx={{ color: tokens.statusOnTrack, borderColor: tokens.statusOnTrack }} />
                  ) : (
                    <Button
                      size="small" variant="outlined" startIcon={<CalendarMonthIcon />}
                      disabled={!connected || addMutation.isPending}
                      onClick={() => addMutation.mutate(d.id)}
                    >
                      Add to calendar
                    </Button>
                  )}
                </Box>
              </Box>
            )
          })}
        </Paper>
      )}

      {!connected && (
        <Typography variant="caption" sx={{ color: tokens.textMuted, display: 'block', mt: 1.5 }}>
          Connect Google Calendar above to enable one-click event creation and reminders.
        </Typography>
      )}

      <Snackbar
        open={Boolean(toast)} autoHideDuration={3000} onClose={() => setToast('')}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity="success" variant="filled" onClose={() => setToast('')}>{toast}</Alert>
      </Snackbar>
    </Box>
  )
}
