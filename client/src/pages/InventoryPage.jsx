import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Paper, Typography, Tabs, Tab, Button, TextField, IconButton, MenuItem,
  CircularProgress, Table, TableBody, TableCell, TableHead, TableRow, Chip,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlineOutlined'
import * as inventoryApi from '../api/inventory'
import PageHeader from '../components/common/PageHeader'
import { tokens, monoSx } from '../theme'
import { formatDate } from '../utils/format'

const CATEGORIES = ['Lighting', 'Switchgear', 'Security', 'Cabling', 'ACMV', 'Fire Safety']
const UNITS = ['each', 'meter', 'box', 'lot']

function EquipmentTab() {
  const queryClient = useQueryClient()
  const { data, isLoading } = useQuery({
    queryKey: ['equipment'],
    queryFn: () => inventoryApi.getEquipment().then((r) => r.data),
  })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['equipment'] })
  const updateMutation = useMutation({ mutationFn: ({ id, patch }) => inventoryApi.updateEquipment(id, patch), onSuccess: invalidate })
  const createMutation = useMutation({
    mutationFn: () => inventoryApi.createEquipment({ name: 'New item', category: 'Lighting', unit: 'each', unitCost: 0, lastTenderRef: '—' }),
    onSuccess: invalidate,
  })
  const removeMutation = useMutation({ mutationFn: (id) => inventoryApi.removeEquipment(id), onSuccess: invalidate })

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 1.5 }}>
        <Button startIcon={<AddIcon />} size="small" variant="outlined" onClick={() => createMutation.mutate()}>
          Add equipment
        </Button>
      </Box>
      <Paper sx={{ borderRadius: 3, overflowX: 'auto' }}>
        <Table size="small">
          <TableHead>
            <TableRow sx={{ bgcolor: tokens.offWhite, '& th': { fontWeight: 700, color: tokens.textSecondary } }}>
              <TableCell>Item</TableCell>
              <TableCell>Category</TableCell>
              <TableCell>Unit</TableCell>
              <TableCell align="right">Unit Cost</TableCell>
              <TableCell>Last Tender</TableCell>
              <TableCell align="right">Updated</TableCell>
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {data.map((e) => (
              <TableRow key={e.id} hover>
                <TableCell sx={{ minWidth: 220 }}>
                  <TextField
                    defaultValue={e.name} variant="standard" fullWidth InputProps={{ disableUnderline: true }}
                    onBlur={(ev) => ev.target.value !== e.name && updateMutation.mutate({ id: e.id, patch: { name: ev.target.value } })}
                  />
                </TableCell>
                <TableCell>
                  <TextField
                    select value={e.category} variant="standard" InputProps={{ disableUnderline: true }} sx={{ minWidth: 110 }}
                    onChange={(ev) => updateMutation.mutate({ id: e.id, patch: { category: ev.target.value } })}
                  >
                    {CATEGORIES.map((c) => <MenuItem key={c} value={c}>{c}</MenuItem>)}
                  </TextField>
                </TableCell>
                <TableCell>
                  <TextField
                    select value={e.unit} variant="standard" InputProps={{ disableUnderline: true }} sx={{ minWidth: 76 }}
                    onChange={(ev) => updateMutation.mutate({ id: e.id, patch: { unit: ev.target.value } })}
                  >
                    {UNITS.map((u) => <MenuItem key={u} value={u}>{u}</MenuItem>)}
                  </TextField>
                </TableCell>
                <TableCell align="right">
                  <TextField
                    defaultValue={e.unitCost} type="number" variant="standard" sx={{ width: 96 }}
                    inputProps={{ style: { textAlign: 'right' } }} InputProps={{ disableUnderline: true, sx: monoSx }}
                    onBlur={(ev) => Number(ev.target.value) !== e.unitCost && updateMutation.mutate({ id: e.id, patch: { unitCost: Number(ev.target.value) } })}
                  />
                </TableCell>
                <TableCell><Typography variant="caption" sx={{ color: tokens.textMuted }}>{e.lastTenderRef}</Typography></TableCell>
                <TableCell align="right"><Typography variant="caption" sx={{ color: tokens.textMuted }}>{formatDate(e.updatedAt)}</Typography></TableCell>
                <TableCell>
                  <IconButton size="small" sx={{ color: tokens.textMuted }} onClick={() => removeMutation.mutate(e.id)}>
                    <DeleteOutlineIcon fontSize="small" />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>
    </Box>
  )
}

function LabourTab() {
  const queryClient = useQueryClient()
  const { data, isLoading } = useQuery({
    queryKey: ['labour'],
    queryFn: () => inventoryApi.getLabour().then((r) => r.data),
  })
  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['labour'] })
  const updateMutation = useMutation({ mutationFn: ({ id, patch }) => inventoryApi.updateLabour(id, patch), onSuccess: invalidate })
  const createMutation = useMutation({ mutationFn: () => inventoryApi.createLabour({ role: 'New role', unit: 'hour', rate: 0 }), onSuccess: invalidate })
  const removeMutation = useMutation({ mutationFn: (id) => inventoryApi.removeLabour(id), onSuccess: invalidate })

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 1.5 }}>
        <Button startIcon={<AddIcon />} size="small" variant="outlined" onClick={() => createMutation.mutate()}>
          Add labour rate
        </Button>
      </Box>
      <Paper sx={{ borderRadius: 3, overflowX: 'auto' }}>
        <Table size="small">
          <TableHead>
            <TableRow sx={{ bgcolor: tokens.offWhite, '& th': { fontWeight: 700, color: tokens.textSecondary } }}>
              <TableCell>Role</TableCell>
              <TableCell>Basis</TableCell>
              <TableCell align="right">Charge-out Rate</TableCell>
              <TableCell />
            </TableRow>
          </TableHead>
          <TableBody>
            {data.map((l) => (
              <TableRow key={l.id} hover>
                <TableCell sx={{ minWidth: 240 }}>
                  <TextField
                    defaultValue={l.role} variant="standard" fullWidth InputProps={{ disableUnderline: true }}
                    onBlur={(ev) => ev.target.value !== l.role && updateMutation.mutate({ id: l.id, patch: { role: ev.target.value } })}
                  />
                </TableCell>
                <TableCell><Chip label={`per ${l.unit}`} size="small" sx={{ bgcolor: tokens.lightGray }} /></TableCell>
                <TableCell align="right">
                  <TextField
                    defaultValue={l.rate} type="number" variant="standard" sx={{ width: 96 }}
                    inputProps={{ style: { textAlign: 'right' } }} InputProps={{ disableUnderline: true, sx: monoSx }}
                    onBlur={(ev) => Number(ev.target.value) !== l.rate && updateMutation.mutate({ id: l.id, patch: { rate: Number(ev.target.value) } })}
                  />
                </TableCell>
                <TableCell>
                  <IconButton size="small" sx={{ color: tokens.textMuted }} onClick={() => removeMutation.mutate(l.id)}>
                    <DeleteOutlineIcon fontSize="small" />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>
    </Box>
  )
}

export default function InventoryPage() {
  const [tab, setTab] = useState(0)
  return (
    <Box>
      <PageHeader
        title="Inventory & Pricing"
        subtitle="Equipment costs from past tenders and labour charge-out rates. These feed the AI quotation engine."
      />
      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2 }}>
        <Tab label="Equipment" />
        <Tab label="Labour rates" />
      </Tabs>
      {tab === 0 ? <EquipmentTab /> : <LabourTab />}
      <Typography variant="caption" sx={{ color: tokens.textMuted, display: 'block', mt: 2 }}>
        Tip: prices edited here are reused the next time the AI drafts a quotation.
      </Typography>
    </Box>
  )
}
