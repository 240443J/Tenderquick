import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Paper, Typography, TextField, Button, Chip, InputAdornment, Tooltip,
  ToggleButton, CircularProgress, Divider,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import TravelExploreIcon from '@mui/icons-material/TravelExplore'
import AddTaskIcon from '@mui/icons-material/AddTask'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import * as scraperApi from '../api/scraper'
import PageHeader from '../components/common/PageHeader'
import AiProgress from '../components/ai/AiProgress'
import { tokens } from '../theme'
import { formatDate } from '../utils/format'
import { deadlineTone } from '../utils/deadline'
import { asArray } from '../utils/list'

const SCAN_STEPS = [
  'Connecting to GeBiz…',
  'Searching live tender listings…',
  'Matching against your keywords…',
  'Ranking by relevance to TenderQuick…',
]

function relevanceColor(score) {
  if (score >= 85) return tokens.statusOnTrack
  if (score >= 70) return tokens.statusSoon
  return tokens.statusNeutral
}

export default function ScraperPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [keyword, setKeyword] = useState('LED lighting, electrical, CCTV')
  const [scanning, setScanning] = useState(false)
  const [results, setResults] = useState(null)
  const [importedIds, setImportedIds] = useState([])

  const { data: sources } = useQuery({
    queryKey: ['scrapeSources'],
    queryFn: () => scraperApi.getSources().then((r) => r.data),
  })
  const [enabled, setEnabled] = useState(['gebiz'])

  const toggle = (sid, status) => {
    if (status !== 'connected') return
    setEnabled((prev) => (prev.includes(sid) ? prev.filter((s) => s !== sid) : [...prev, sid]))
  }

  const runScan = async () => {
    setScanning(true)
    setResults(null)
    const res = await scraperApi.scan(keyword, enabled)
    setResults(res.data)
    setScanning(false)
  }

  const importMutation = useMutation({
    mutationFn: (resultId) => scraperApi.importResult(resultId),
    onSuccess: (res, resultId) => {
      setImportedIds((prev) => [...prev, resultId])
      queryClient.invalidateQueries({ queryKey: ['tenders'] })
      queryClient.invalidateQueries({ queryKey: ['deadlines'] })
    },
  })

  return (
    <Box>
      <PageHeader
        ai
        title="Keyword Scraper"
        subtitle="AI scans live public-tender portals and surfaces the opportunities most relevant to your business."
      />

      <Paper sx={{ p: 3, borderRadius: 3, mb: 3 }}>
        <TextField
          fullWidth
          label="Keywords"
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
          placeholder="e.g. LED lighting, electrical, CCTV, cabling"
          InputProps={{
            startAdornment: (
              <InputAdornment position="start"><SearchIcon sx={{ color: tokens.textMuted }} /></InputAdornment>
            ),
          }}
          sx={{ mb: 2 }}
        />

        <Box sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap', alignItems: 'center', mb: 2 }}>
          <Typography variant="body2" sx={{ color: tokens.textSecondary, mr: 1 }}>Sources:</Typography>
          {asArray(sources).map((s) => (
            <Tooltip key={s.id} title={s.status === 'connected' ? s.note : `${s.note} — coming soon`}>
              <span>
                <ToggleButton
                  value={s.id}
                  selected={enabled.includes(s.id)}
                  disabled={s.status !== 'connected'}
                  onChange={() => toggle(s.id, s.status)}
                  size="small"
                  sx={{
                    textTransform: 'none', borderRadius: 2, px: 1.5,
                    '&.Mui-selected': { bgcolor: tokens.accentIndigoSubtle, color: tokens.accentIndigo, fontWeight: 700 },
                  }}
                >
                  {s.label}
                  {s.status !== 'connected' && <Chip label="soon" size="small" sx={{ ml: 0.75, height: 16, fontSize: '0.6rem' }} />}
                </ToggleButton>
              </span>
            </Tooltip>
          ))}
        </Box>

        <Button
          variant="contained"
          size="large"
          startIcon={<TravelExploreIcon />}
          disabled={scanning || enabled.length === 0}
          onClick={runScan}
          sx={{ px: 3 }}
        >
          {scanning ? 'Scanning…' : 'Scan for tenders'}
        </Button>
      </Paper>

      {scanning && <AiProgress steps={SCAN_STEPS} />}

      {!scanning && results && (
        <Box>
          <Typography variant="body2" sx={{ color: tokens.textSecondary, mb: 1.5 }}>
            Found <b>{results.length}</b> matching tenders, ranked by relevance.
          </Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {results.map((r) => {
              const isImported = importedIds.includes(r.id)
              const tone = deadlineTone(r.closingAt)
              return (
                <Paper key={r.id} sx={{ p: 2.5, borderRadius: 3 }}>
                  <Box sx={{ display: 'flex', gap: 2, justifyContent: 'space-between', flexWrap: 'wrap' }}>
                    <Box sx={{ flex: 1, minWidth: 260 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5, flexWrap: 'wrap' }}>
                        <Chip
                          label={`${r.relevance}% match`}
                          size="small"
                          sx={{ bgcolor: relevanceColor(r.relevance), color: '#fff', fontWeight: 700 }}
                        />
                        <Typography variant="caption" sx={{ color: tokens.textSecondary }}>{r.reference}</Typography>
                        <Chip label="GeBiz" size="small" variant="outlined" sx={{ height: 20 }} />
                      </Box>
                      <Typography variant="body1" sx={{ fontWeight: 700 }}>{r.title}</Typography>
                      <Typography variant="body2" sx={{ color: tokens.textSecondary, mb: 1 }}>{r.agency}</Typography>
                      <Typography variant="body2" sx={{ color: tokens.textMuted, mb: 1.5 }}>{r.summary}</Typography>
                      <Box sx={{ display: 'flex', gap: 0.75, flexWrap: 'wrap' }}>
                        {r.matched.map((m) => (
                          <Chip key={m} label={m} size="small" sx={{ bgcolor: tokens.accentIndigoSubtle, color: tokens.accentIndigo, height: 22 }} />
                        ))}
                      </Box>
                    </Box>

                    <Box sx={{ textAlign: 'right', display: 'flex', flexDirection: 'column', gap: 1, alignItems: 'flex-end' }}>
                      <Typography variant="caption" sx={{ color: tokens.textMuted }}>Est. value</Typography>
                      <Typography variant="body2" sx={{ fontWeight: 700 }}>{r.estValueRange}</Typography>
                      <Divider flexItem sx={{ width: '100%', my: 0.5 }} />
                      <Typography variant="caption" sx={{ color: tokens.textMuted }}>Closes {formatDate(r.closingAt)}</Typography>
                      <Chip label={tone.label} size="small" sx={{ bgcolor: tone.bg, color: tone.color, fontWeight: 700 }} />
                      {isImported ? (
                        <Button size="small" startIcon={<CheckCircleIcon />} disabled sx={{ color: tokens.statusOnTrack }}>
                          Imported
                        </Button>
                      ) : (
                        <Button
                          variant="outlined"
                          size="small"
                          startIcon={<AddTaskIcon />}
                          disabled={importMutation.isPending}
                          onClick={() => importMutation.mutate(r.id)}
                        >
                          Add to workspace
                        </Button>
                      )}
                    </Box>
                  </Box>
                </Paper>
              )
            })}
          </Box>

          {importedIds.length > 0 && (
            <Box sx={{ mt: 2, textAlign: 'center' }}>
              <Button onClick={() => navigate('/tenders')}>View imported tenders →</Button>
            </Box>
          )}
        </Box>
      )}

      {!scanning && !results && (
        <Paper sx={{ p: 5, borderRadius: 3, textAlign: 'center' }}>
          <TravelExploreIcon sx={{ fontSize: 44, color: tokens.accentIndigo, mb: 1 }} />
          <Typography variant="h4" sx={{ mb: 0.5 }}>Discover new tenders</Typography>
          <Typography variant="body2" sx={{ color: tokens.textSecondary }}>
            Enter keywords above and let the AI scan GeBiz for relevant opportunities.
          </Typography>
        </Paper>
      )}
    </Box>
  )
}
