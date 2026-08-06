import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Paper, Typography, TextField, MenuItem, Button, Chip, CircularProgress,
} from '@mui/material'
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome'
import * as draftsApi from '../api/drafts'
import * as tendersApi from '../api/tenders'
import PageHeader from '../components/common/PageHeader'
import { tokens } from '../theme'
import { formatDateTime } from '../utils/format'
import { asArray } from '../utils/list'

const statusColor = {
  Draft: { c: tokens.statusDraft, b: tokens.statusDraftBg },
  'In Review': { c: tokens.statusSoon, b: tokens.statusSoonBg },
  Final: { c: tokens.statusOnTrack, b: tokens.statusOnTrackBg },
}

export default function DraftingPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [tenderId, setTenderId] = useState('')

  const { data: drafts, isLoading } = useQuery({
    queryKey: ['drafts'],
    queryFn: () => draftsApi.getAll().then((r) => r.data),
  })
  const { data: tenders } = useQuery({
    queryKey: ['tenders', '', ''],
    queryFn: () => tendersApi.getAll().then((r) => r.data),
  })

  const createMutation = useMutation({
    mutationFn: async () => {
      const tender = tenders.find((t) => String(t.id) === String(tenderId))
      const res = await draftsApi.create({
        title: `Technical Proposal — ${tender.title}`,
        tenderId: tender.id,
        tenderRef: tender.reference,
        sections: [],
      })
      return res.data
    },
    onSuccess: (draft) => {
      queryClient.invalidateQueries({ queryKey: ['drafts'] })
      navigate(`/drafting/${draft.id}?generate=1`)
    },
  })

  return (
    <Box>
      <PageHeader
        ai
        title="AI Tender Drafting"
        subtitle="Generate compliant proposal drafts from a tender's specification. The AI learns from your edits every time."
      />

      <Paper
        sx={{
          p: 3, borderRadius: 3, mb: 4,
          border: `1px solid ${tokens.accentIndigoLight}`, bgcolor: tokens.accentIndigoSubtle,
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <AutoAwesomeIcon sx={{ color: tokens.accentIndigo }} />
          <Typography variant="h4" sx={{ color: tokens.accentIndigo }}>Start a new draft</Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
          <TextField
            select
            size="small"
            label="Select a tender"
            value={tenderId}
            onChange={(e) => setTenderId(e.target.value)}
            sx={{ minWidth: 360, bgcolor: '#fff', borderRadius: 1 }}
          >
            {asArray(tenders).map((t) => (
              <MenuItem key={t.id} value={t.id}>{t.reference} — {t.title}</MenuItem>
            ))}
          </TextField>
          <Button
            variant="contained"
            startIcon={<AutoAwesomeIcon />}
            disabled={!tenderId || createMutation.isPending}
            onClick={() => createMutation.mutate()}
            sx={{ px: 3, py: 1.1 }}
          >
            {createMutation.isPending ? 'Preparing…' : 'Generate with AI'}
          </Button>
        </Box>
      </Paper>

      <Typography variant="h4" sx={{ mb: 1.5 }}>Your drafts</Typography>
      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
      ) : (
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 2 }}>
          {asArray(drafts).map((d) => {
            const sc = statusColor[d.status] || statusColor.Draft
            return (
              <Paper
                key={d.id}
                onClick={() => navigate(`/drafting/${d.id}`)}
                sx={{
                  p: 2.5, borderRadius: 3, cursor: 'pointer',
                  transition: 'box-shadow .15s', '&:hover': { boxShadow: 4 },
                }}
              >
                <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 1, mb: 1 }}>
                  <Typography variant="body1" sx={{ fontWeight: 700 }}>{d.title}</Typography>
                  <Chip label={d.status} size="small" sx={{ bgcolor: sc.b, color: sc.c, fontWeight: 700, flexShrink: 0 }} />
                </Box>
                <Typography variant="caption" sx={{ color: tokens.textSecondary, display: 'block' }}>
                  {d.tenderRef} · v{d.version} · {d.sections.length} sections
                </Typography>
                <Typography variant="caption" sx={{ color: tokens.textMuted }}>
                  Updated {formatDateTime(d.updatedAt)}
                </Typography>
              </Paper>
            )
          })}
        </Box>
      )}
    </Box>
  )
}
