import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Paper, Typography, TextField, MenuItem, Button, Chip, CircularProgress,
} from '@mui/material'
import AutoAwesomeIcon from '@mui/icons-material/AutoAwesome'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import WarningAmberIcon from '@mui/icons-material/WarningAmber'
import * as quotationsApi from '../api/quotations'
import * as tendersApi from '../api/tenders'
import PageHeader from '../components/common/PageHeader'
import AiProgress from '../components/ai/AiProgress'
import DataTable from '../components/common/DataTable'
import { tokens, monoSx } from '../theme'
import { formatCurrency, formatDate } from '../utils/format'
import { computeTotals } from '../utils/quote'

const GEN_STEPS = [
  'Reading tender specification…',
  'Extracting required items & quantities…',
  'Pricing from your inventory database…',
  'Assembling quotation…',
]

export default function QuotationsPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [tenderId, setTenderId] = useState('')

  const { data: quotes, isLoading } = useQuery({
    queryKey: ['quotations'],
    queryFn: () => quotationsApi.getAll().then((r) => r.data),
  })
  const { data: tenders } = useQuery({
    queryKey: ['tenders', '', ''],
    queryFn: () => tendersApi.getAll().then((r) => r.data),
  })

  const generateMutation = useMutation({
    mutationFn: () => quotationsApi.generateFromTender(tenderId),
    onSuccess: (res) => {
      queryClient.invalidateQueries({ queryKey: ['quotations'] })
      navigate(`/quotations/${res.data.id}`)
    },
  })

  const columns = [
    { key: 'quoteNo', label: 'Quote No.', render: (r) => <Box sx={monoSx}>{r.quoteNo}</Box> },
    { key: 'title', label: 'Subject' },
    { key: 'client', label: 'Client' },
    {
      key: 'total', label: 'Total', align: 'right',
      render: (r) => <Box sx={monoSx}>{formatCurrency(computeTotals(r).total)}</Box>,
    },
    {
      key: 'status', label: 'Status', align: 'center',
      render: (r) => (r.verified ? (
        <Chip icon={<CheckCircleIcon />} label="Verified" size="small" sx={{ bgcolor: tokens.statusOnTrackBg, color: tokens.statusOnTrack, fontWeight: 700 }} />
      ) : (
        <Chip icon={<WarningAmberIcon />} label="Needs check" size="small" sx={{ bgcolor: tokens.statusSoonBg, color: tokens.statusSoon, fontWeight: 700 }} />
      )),
    },
    { key: 'createdAt', label: 'Created', align: 'right', render: (r) => formatDate(r.createdAt) },
  ]

  return (
    <Box>
      <PageHeader
        ai
        title="Quotations"
        subtitle="The AI drafts a priced quotation from a tender's specs. A human must verify it before it can be exported."
      />

      <Paper
        sx={{
          p: 3, borderRadius: 3, mb: 4,
          border: `1px solid ${tokens.accentIndigoLight}`, bgcolor: tokens.accentIndigoSubtle,
        }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <AutoAwesomeIcon sx={{ color: tokens.accentIndigo }} />
          <Typography variant="h4" sx={{ color: tokens.accentIndigo }}>Generate a quotation</Typography>
        </Box>

        {generateMutation.isPending ? (
          <AiProgress steps={GEN_STEPS} stepMs={500} />
        ) : (
          <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
            <TextField
              select
              size="small"
              label="Select a tender"
              value={tenderId}
              onChange={(e) => setTenderId(e.target.value)}
              sx={{ minWidth: 360, bgcolor: '#fff', borderRadius: 1 }}
            >
              {(tenders || []).map((t) => (
                <MenuItem key={t.id} value={t.id}>{t.reference} — {t.title}</MenuItem>
              ))}
            </TextField>
            <Button
              variant="contained"
              startIcon={<AutoAwesomeIcon />}
              disabled={!tenderId}
              onClick={() => generateMutation.mutate()}
              sx={{ px: 3, py: 1.1 }}
            >
              Generate draft quote
            </Button>
          </Box>
        )}
      </Paper>

      <Typography variant="h4" sx={{ mb: 1.5 }}>All quotations</Typography>
      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
      ) : (
        <DataTable
          columns={columns}
          rows={quotes || []}
          getRowKey={(r) => r.id}
          onRowClick={(r) => navigate(`/quotations/${r.id}`)}
        />
      )}
    </Box>
  )
}
