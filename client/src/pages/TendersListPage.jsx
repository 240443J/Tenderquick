import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { Helmet } from 'react-helmet-async'
import {
  Alert, Box, Button, CircularProgress, Container, MenuItem, Paper, Stack, Table, TableBody,
  TableCell, TableContainer, TableHead, TableRow, TextField, Typography,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import DescriptionIcon from '@mui/icons-material/Description'
import { getAll } from '../api/tenders'
import { formatCurrency, formatDate } from '../utils/format'
import { TENDER_STATUSES } from '../utils/tenderStatus'
import StatusChip from '../components/common/StatusChip'
import RoleGate from '../components/common/RoleGate'
import EmptyState from '../components/common/EmptyState'
import useDebounce from '../hooks/useDebounce'

export default function TendersListPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState('')
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebounce(search)

  const { data, isPending, isError } = useQuery({
    queryKey: ['tenders', status, debouncedSearch],
    queryFn: () => getAll({ status: status || undefined, search: debouncedSearch || undefined }),
    placeholderData: keepPreviousData,
  })

  const tenders = data?.data ?? []

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Helmet><title>Tenders · Tenderquick</title></Helmet>

      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 3 }}>
        <Typography variant="h4">Tenders</Typography>
        <RoleGate allow={['Admin', 'Estimator']}>
          <Button startIcon={<AddIcon />} variant="contained" onClick={() => navigate('/tenders/new')}>
            New Tender
          </Button>
        </RoleGate>
      </Stack>

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 2 }}>
        <TextField
          label="Search" placeholder="reference, title or agency" size="small" sx={{ minWidth: 280 }}
          value={search} onChange={(e) => setSearch(e.target.value)}
        />
        <TextField
          label="Status" select size="small" sx={{ minWidth: 160 }}
          value={status} onChange={(e) => setStatus(e.target.value)}
        >
          <MenuItem value="">All</MenuItem>
          {TENDER_STATUSES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
        </TextField>
      </Stack>

      {isPending && <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>}
      {isError && <Alert severity="error">Failed to load tenders. Is the backend running?</Alert>}

      {!isPending && !isError && tenders.length === 0 && (
        <Paper>
          <EmptyState
            icon={<DescriptionIcon />}
            title="No tenders found"
            message={search || status ? 'Try clearing the filters.' : 'Add your first tender or import from Tender Search.'}
            action={(
              <RoleGate allow={['Admin', 'Estimator']}>
                <Button variant="contained" startIcon={<AddIcon />} onClick={() => navigate('/tenders/new')}>
                  Add your first tender
                </Button>
              </RoleGate>
            )}
          />
        </Paper>
      )}

      {tenders.length > 0 && (
        <TableContainer component={Paper}>
          <Table size="small" stickyHeader>
            <TableHead>
              <TableRow sx={{ '& th': { bgcolor: '#fafafa', fontWeight: 700 } }}>
                <TableCell>Reference</TableCell>
                <TableCell>Title</TableCell>
                <TableCell>Agency</TableCell>
                <TableCell>Source</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Est. Value</TableCell>
                <TableCell>Closing</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {tenders.map((t) => (
                <TableRow key={t.id} hover sx={{ cursor: 'pointer' }} onClick={() => navigate(`/tenders/${t.id}`)}>
                  <TableCell>{t.reference}</TableCell>
                  <TableCell sx={{ maxWidth: 320 }}>{t.title}</TableCell>
                  <TableCell>{t.agency}</TableCell>
                  <TableCell>{t.source}</TableCell>
                  <TableCell><StatusChip status={t.status} /></TableCell>
                  <TableCell align="right">{formatCurrency(t.estValue)}</TableCell>
                  <TableCell>{formatDate(t.closingAt)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Container>
  )
}
