import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  Box, Typography, Button, TextField, MenuItem, CircularProgress, Alert,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import * as tendersApi from '../api/tenders'
import DataTable from '../components/common/DataTable'
import StatusChip from '../components/common/StatusChip'
import EmptyState from '../components/common/EmptyState'
import RoleGate from '../components/common/RoleGate'
import { TENDER_STATUSES } from '../utils/tenderStatus'
import { formatCurrency, formatDate } from '../utils/format'
import { monoSx } from '../theme'

export default function TendersListPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState('')
  const [search, setSearch] = useState('')

  const { data, isLoading, isError } = useQuery({
    queryKey: ['tenders', status, search],
    queryFn: () => tendersApi.getAll({
      status: status || undefined,
      search: search || undefined,
    }).then((r) => r.data),
  })

  const columns = [
    { key: 'reference', label: 'Reference', render: (r) => <Box sx={monoSx}>{r.reference}</Box> },
    { key: 'title', label: 'Title' },
    { key: 'agency', label: 'Agency' },
    { key: 'status', label: 'Status', render: (r) => <StatusChip status={r.status} /> },
    {
      key: 'estValue', label: 'Est. Value', align: 'right',
      render: (r) => <Box sx={monoSx}>{formatCurrency(r.estValue)}</Box>,
    },
    { key: 'closingAt', label: 'Closing', align: 'right', render: (r) => formatDate(r.closingAt) },
  ]

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3, flexWrap: 'wrap', gap: 2 }}>
        <Typography variant="h1">Tenders</Typography>
        <RoleGate roles={['Admin', 'Estimator']}>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => navigate('/tenders/new')} sx={{ px: 3, py: 1.25 }}>
            New Tender
          </Button>
        </RoleGate>
      </Box>

      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <TextField
          label="Search"
          size="small"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Reference, title, agency…"
          sx={{ minWidth: 260 }}
        />
        <TextField
          label="Status"
          size="small"
          select
          value={status}
          onChange={(e) => setStatus(e.target.value)}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="">All statuses</MenuItem>
          {TENDER_STATUSES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
        </TextField>
      </Box>

      {isLoading && (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
      )}
      {isError && <Alert severity="error">Could not load tenders. Is the API running?</Alert>}

      {!isLoading && !isError && (
        data.length === 0 ? (
          <EmptyState
            title="No tenders found"
            description={search || status ? 'Try clearing your filters.' : 'Add your first tender to get started.'}
            action={(
              <RoleGate roles={['Admin', 'Estimator']}>
                <Button variant="contained" startIcon={<AddIcon />} onClick={() => navigate('/tenders/new')}>
                  Add Tender
                </Button>
              </RoleGate>
            )}
          />
        ) : (
          <DataTable
            columns={columns}
            rows={data}
            getRowKey={(r) => r.id}
            onRowClick={(r) => navigate(`/tenders/${r.id}`)}
          />
        )
      )}
    </Box>
  )
}
