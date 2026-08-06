import { useEffect, useRef, useState } from 'react'
import { useParams, useNavigate, useSearchParams } from 'react-router-dom'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Box, Paper, Typography, Button, TextField, Chip, CircularProgress, Divider,
  Snackbar, Alert, LinearProgress,
} from '@mui/material'
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import ReplayIcon from '@mui/icons-material/Replay'
import SaveIcon from '@mui/icons-material/Save'
import PsychologyIcon from '@mui/icons-material/Psychology'
import * as draftsApi from '../api/drafts'
import AiProgress from '../components/ai/AiProgress'
import { streamText } from '../utils/stream'
import { tokens } from '../theme'
import { asArray } from '../utils/list'

const GEN_STEPS = [
  'Reading tender specification…',
  'Recalling your past 14 submissions…',
  'Applying learned drafting preferences…',
  'Writing proposal sections…',
]

export default function DraftWorkspacePage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [searchParams, setSearchParams] = useSearchParams()

  const [sections, setSections] = useState([])
  const [phase, setPhase] = useState('idle') // idle | thinking | writing | ready
  const [toast, setToast] = useState('')
  const [saving, setSaving] = useState(false)
  const cancelRef = useRef(null)
  const startedRef = useRef(false)

  const { data: draft, isLoading } = useQuery({
    queryKey: ['draft', id],
    queryFn: () => draftsApi.getById(id).then((r) => r.data),
  })
  const { data: memory } = useQuery({
    queryKey: ['draftMemory'],
    queryFn: () => draftsApi.getMemory().then((r) => r.data),
  })

  useEffect(() => {
    if (!draft) return
    if (draft.sections.length > 0 && phase === 'idle') {
      setSections(draft.sections)
      setPhase('ready')
    }
  }, [draft, phase])

  async function runGeneration() {
    if (!draft) return
    setPhase('thinking')
    setSections([])
    const res = await draftsApi.generateSections(draft.tenderId)
    const generated = res.data.sections
    setPhase('writing')
    const built = []
    for (const sec of generated) {
      built.push({ heading: sec.heading, body: '' })
      setSections(built.map((s) => ({ ...s })))
      // eslint-disable-next-line no-await-in-loop
      await new Promise((resolve) => {
        cancelRef.current = streamText(
          sec.body,
          (slice) => {
            built[built.length - 1].body = slice
            setSections(built.map((s) => ({ ...s })))
          },
          { stepMs: 8, chunk: 7, onDone: resolve },
        )
      })
    }
    setPhase('ready')
    await draftsApi.update(id, { sections: built, status: 'Draft' })
    queryClient.invalidateQueries({ queryKey: ['drafts'] })
    queryClient.invalidateQueries({ queryKey: ['draft', id] })
  }

  // Auto-run generation when arriving with ?generate=1 (fresh empty draft).
  useEffect(() => {
    if (draft && searchParams.get('generate') === '1' && !startedRef.current) {
      startedRef.current = true
      setSearchParams({}, { replace: true })
      runGeneration()
    }
    return () => cancelRef.current?.()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [draft])

  const editSection = (i, body) => {
    setSections((prev) => prev.map((s, idx) => (idx === i ? { ...s, body } : s)))
  }

  const onSectionBlur = async () => {
    await draftsApi.learnFromEdit()
    queryClient.invalidateQueries({ queryKey: ['draftMemory'] })
    setToast('AI noted your edit — it will draft more like this next time.')
  }

  const save = async () => {
    setSaving(true)
    await draftsApi.update(id, { sections, status: 'In Review', bumpVersion: true })
    queryClient.invalidateQueries({ queryKey: ['drafts'] })
    queryClient.invalidateQueries({ queryKey: ['draft', id] })
    setSaving(false)
    setToast('Draft saved and marked “In Review”.')
  }

  if (isLoading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
  }

  const busy = phase === 'thinking' || phase === 'writing'

  return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/drafting')} sx={{ mb: 2, color: tokens.textSecondary }}>
        Back to drafts
      </Button>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <Box>
          <Typography variant="caption" sx={{ color: tokens.textSecondary }}>{draft.tenderRef}</Typography>
          <Typography variant="h2">{draft.title}</Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="outlined" startIcon={<ReplayIcon />} disabled={busy} onClick={runGeneration}>
            Regenerate
          </Button>
          <Button variant="contained" startIcon={<SaveIcon />} disabled={busy || saving} onClick={save}>
            Save draft
          </Button>
        </Box>
      </Box>

      <Box sx={{ display: 'flex', gap: 3, alignItems: 'flex-start', flexDirection: { xs: 'column', lg: 'row' } }}>
        {/* Document */}
        <Box sx={{ flex: 1, minWidth: 0, width: '100%' }}>
          {phase === 'thinking' && <AiProgress steps={GEN_STEPS} />}

          {(phase === 'writing' || phase === 'ready') && sections.length > 0 && (
            <Paper sx={{ p: { xs: 2.5, md: 4 }, borderRadius: 3 }}>
              {phase === 'writing' && (
                <Box sx={{ mb: 2 }}>
                  <LinearProgress />
                  <Typography variant="caption" sx={{ color: tokens.accentIndigo }}>
                    AI is writing…
                  </Typography>
                </Box>
              )}
              {sections.map((sec, i) => (
                <Box key={sec.heading} sx={{ mb: 3 }}>
                  <Typography variant="h4" sx={{ mb: 1 }}>{sec.heading}</Typography>
                  {phase === 'ready' ? (
                    <TextField
                      value={sec.body}
                      onChange={(e) => editSection(i, e.target.value)}
                      onBlur={onSectionBlur}
                      multiline
                      fullWidth
                      variant="standard"
                      InputProps={{ disableUnderline: true, sx: { fontSize: '0.95rem', lineHeight: 1.7 } }}
                    />
                  ) : (
                    <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap', color: tokens.textPrimary }}>
                      {sec.body}
                      <Box component="span" sx={{ borderRight: `2px solid ${tokens.accentIndigo}`, ml: 0.25, animation: 'blink 1s steps(1) infinite', '@keyframes blink': { '50%': { opacity: 0 } } }} />
                    </Typography>
                  )}
                </Box>
              ))}
            </Paper>
          )}

          {phase === 'idle' && (
            <Paper sx={{ p: 5, borderRadius: 3, textAlign: 'center' }}>
              <AutoAwesomeIcon sx={{ fontSize: 40, color: tokens.accentIndigo, mb: 1 }} />
              <Typography variant="h4" sx={{ mb: 1 }}>This draft is empty</Typography>
              <Typography variant="body2" sx={{ color: tokens.textSecondary, mb: 2 }}>
                Let the AI write a first version from the tender specification.
              </Typography>
              <Button variant="contained" startIcon={<AutoAwesomeIcon />} onClick={runGeneration}>
                Generate with AI
              </Button>
            </Paper>
          )}
        </Box>

        {/* AI memory panel */}
        <Paper sx={{ p: 3, borderRadius: 3, width: { xs: '100%', lg: 340 }, flexShrink: 0, position: { lg: 'sticky' }, top: 88 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
            <PsychologyIcon sx={{ color: tokens.accentIndigo }} />
            <Typography variant="h4">AI Memory</Typography>
          </Box>
          <Typography variant="caption" sx={{ color: tokens.textSecondary, display: 'block', mb: 2 }}>
            Learned from {memory?.samplesLearned ?? '—'} past submissions · improves every time you edit
          </Typography>

          {asArray(memory?.preferences).map((p) => (
            <Box key={p.id} sx={{ mb: 2 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 1, mb: 0.5 }}>
                <Typography variant="body2" sx={{ fontWeight: 600, lineHeight: 1.3 }}>{p.text}</Typography>
                <Typography variant="caption" sx={{ color: tokens.accentIndigo, fontWeight: 700, flexShrink: 0 }}>
                  {Math.round(p.confidence * 100)}%
                </Typography>
              </Box>
              <LinearProgress
                variant="determinate"
                value={p.confidence * 100}
                sx={{ height: 5, borderRadius: 1, mb: 0.5, bgcolor: tokens.lightGray }}
              />
              <Typography variant="caption" sx={{ color: tokens.textMuted }}>{p.source}</Typography>
            </Box>
          ))}
          <Divider sx={{ my: 1.5 }} />
          <Chip
            icon={<AutoAwesomeIcon sx={{ fontSize: '0.9rem !important' }} />}
            label="Edits feed back into memory"
            size="small"
            sx={{ bgcolor: tokens.accentIndigoSubtle, color: tokens.accentIndigo, fontWeight: 600 }}
          />
        </Paper>
      </Box>

      <Snackbar
        open={Boolean(toast)}
        autoHideDuration={3000}
        onClose={() => setToast('')}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity="success" variant="filled" onClose={() => setToast('')} sx={{ alignItems: 'center' }}>
          {toast}
        </Alert>
      </Snackbar>
    </Box>
  )
}
