import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Helmet } from 'react-helmet-async'
import {
  Alert, Box, Button, Checkbox, Chip, CircularProgress, Container, FormControlLabel,
  FormGroup, Paper, Stack, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, TextField, Typography,
} from '@mui/material'
import { search as searchApi, importResults as importApi } from '../api/tenderSearch'
import { formatCurrency, formatDate } from '../utils/format'

const SOURCES = [
  { key: 'gebiz', label: 'GeBIZ' },
  { key: 'sesami', label: 'Sesami' },
  { key: 'tenderboard', label: 'Tenderboard' },
]

const rowKey = (r) => `${r.source}|${r.reference}`

export default function TenderSearchPage() {
  const qc = useQueryClient()
  const [keyword, setKeyword] = useState('')
  const [sources, setSources] = useState({ gebiz: true, sesami: true, tenderboard: true })
  const [selected, setSelected] = useState({})

  const importMutation = useMutation({
    mutationFn: (items) => importApi(items),
    onSuccess: () => {
      setSelected({})
      qc.invalidateQueries({ queryKey: ['tenders'] })
    },
  })

  const searchMutation = useMutation({
    mutationFn: () => searchApi(keyword.trim(), SOURCES.filter((s) => sources[s.key]).map((s) => s.key)),
    onSuccess: () => {
      setSelected({})
      importMutation.reset()
    },
  })

  const data = searchMutation.data?.data
  const results = data?.results ?? []

  const toggleSource = (key) => setSources((s) => ({ ...s, [key]: !s[key] }))
  const toggleRow = (r) => setSelected((s) => ({ ...s, [rowKey(r)]: !s[rowKey(r)] }))

  const selectedItems = results.filter((r) => selected[rowKey(r)])
  const canSearch = keyword.trim().length > 0 && Object.values(sources).some(Boolean)

  const onSearch = (e) => {
    e.preventDefault()
    if (canSearch) searchMutation.mutate()
  }

  const onImport = () => importMutation.mutate(selectedItems)

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Helmet><title>Tender Search · Tenderquick</title></Helmet>

      <Typography variant="h4" gutterBottom>Tender Search</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Search published tenders by keyword across multiple portals and import matches.
      </Typography>

      <Paper component="form" onSubmit={onSearch} sx={{ p: 3, mb: 3 }}>
        <Stack spacing={2}>
          <TextField
            label="Keyword"
            placeholder="e.g. cleaning, lighting, software"
            value={keyword}
            onChange={(e) => setKeyword(e.target.value)}
            fullWidth
            autoFocus
          />
          <FormGroup row>
            {SOURCES.map((s) => (
              <FormControlLabel
                key={s.key}
                control={<Checkbox checked={sources[s.key]} onChange={() => toggleSource(s.key)} />}
                label={s.label}
              />
            ))}
          </FormGroup>
          <Box>
            <Button type="submit" variant="contained" disabled={!canSearch || searchMutation.isPending}>
              {searchMutation.isPending ? 'Searching…' : 'Search'}
            </Button>
          </Box>
        </Stack>
      </Paper>

      {searchMutation.isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {searchMutation.error?.response?.data?.message ?? 'Search failed. Is the backend running?'}
        </Alert>
      )}

      {data && (
        <Stack direction="row" spacing={1} sx={{ mb: 2 }} flexWrap="wrap" useFlexGap>
          {data.sources.map((s) => (
            <Chip
              key={s.source}
              size="small"
              color={s.ok ? (s.count > 0 ? 'success' : 'default') : 'error'}
              variant={s.count > 0 ? 'filled' : 'outlined'}
              label={`${s.source}: ${s.count}${s.message ? ` · ${s.message}` : ''}`}
            />
          ))}
        </Stack>
      )}

      {searchMutation.isPending && (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
      )}

      {data && results.length === 0 && !searchMutation.isPending && (
        <Alert severity="info">No tenders matched “{data.keyword}”. Try a broader keyword.</Alert>
      )}

      {results.length > 0 && (
        <>
          <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 1 }}>
            <Typography variant="subtitle1">{results.length} result(s)</Typography>
            <Button
              variant="outlined"
              disabled={selectedItems.length === 0 || importMutation.isPending}
              onClick={onImport}
            >
              {importMutation.isPending ? 'Importing…' : `Import selected (${selectedItems.length})`}
            </Button>
          </Stack>

          {importMutation.isSuccess && (
            <Alert severity="success" sx={{ mb: 2 }}>
              Imported {importMutation.data.data.imported}, skipped {importMutation.data.data.skipped} (duplicates).
            </Alert>
          )}
          {importMutation.isError && (
            <Alert severity="error" sx={{ mb: 2 }}>Import failed.</Alert>
          )}

          <TableContainer component={Paper}>
            <Table size="small" stickyHeader>
              <TableHead>
                <TableRow>
                  <TableCell padding="checkbox" />
                  <TableCell>Source</TableCell>
                  <TableCell>Reference</TableCell>
                  <TableCell>Title</TableCell>
                  <TableCell>Agency</TableCell>
                  <TableCell align="right">Value</TableCell>
                  <TableCell>Date</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {results.map((r) => (
                  <TableRow key={rowKey(r)} hover selected={!!selected[rowKey(r)]}>
                    <TableCell padding="checkbox">
                      <Checkbox checked={!!selected[rowKey(r)]} onChange={() => toggleRow(r)} />
                    </TableCell>
                    <TableCell><Chip size="small" label={r.source} /></TableCell>
                    <TableCell>{r.reference}</TableCell>
                    <TableCell sx={{ maxWidth: 360 }}>{r.title}</TableCell>
                    <TableCell>{r.agency}</TableCell>
                    <TableCell align="right">{formatCurrency(r.value)}</TableCell>
                    <TableCell>{formatDate(r.date)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}
    </Container>
  )
}
