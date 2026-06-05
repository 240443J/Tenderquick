import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Typography, Button, Paper, Stack, Divider, CircularProgress, Alert,
} from '@mui/material'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import * as tendersApi from '../api/tenders'
import StatusChip from '../components/common/StatusChip'
import ConfirmDialog from '../components/common/ConfirmDialog'
import RoleGate from '../components/common/RoleGate'
import { formatCurrency, formatDate, formatDateTime } from '../utils/format'
import { tokens, monoSx } from '../theme'

function MetaRow({ label, children }) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2 }}>
      <Typography variant="body2" sx={{ color: tokens.textSecondary }}>{label}</Typography>
      <Box sx={{ textAlign: 'right', fontWeight: 500 }}>{children}</Box>
    </Box>
  )
}

export default function TenderDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [confirmOpen, setConfirmOpen] = useState(false)

  const { data: tender, isLoading, isError } = useQuery({
    queryKey: ['tender', id],
    queryFn: () => tendersApi.getById(id).then((r) => r.data),
  })

  const deleteMutation = useMutation({
    mutationFn: () => tendersApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenders'] })
      navigate('/tenders')
    },
  })

  if (isLoading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
  }
  if (isError || !tender) {
    return <Alert severity="error">Tender not found.</Alert>
  }

  return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/tenders')} sx={{ mb: 2, color: tokens.textSecondary }}>
        Back to tenders
      </Button>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <Box>
          <Box sx={{ ...monoSx, color: tokens.textSecondary, mb: 0.5 }}>{tender.reference}</Box>
          <Typography variant="h1">{tender.title}</Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <RoleGate roles={['Admin', 'Estimator']}>
            <Button variant="outlined" startIcon={<EditIcon />} onClick={() => navigate(`/tenders/${id}/edit`)}>
              Edit
            </Button>
          </RoleGate>
          <RoleGate roles={['Admin']}>
            <Button variant="outlined" color="error" startIcon={<DeleteIcon />} onClick={() => setConfirmOpen(true)}>
              Delete
            </Button>
          </RoleGate>
        </Box>
      </Box>

      <Box sx={{ display: 'flex', gap: 3, flexDirection: { xs: 'column', md: 'row' }, alignItems: 'flex-start' }}>
        <Paper sx={{ p: 3, borderRadius: 3, flex: 1, width: '100%' }}>
          <Typography variant="h4" sx={{ mb: 1.5 }}>Notes</Typography>
          <Typography variant="body1" sx={{ color: tender.notes ? tokens.textPrimary : tokens.textMuted, whiteSpace: 'pre-wrap' }}>
            {tender.notes || 'No notes recorded.'}
          </Typography>
        </Paper>

        <Paper sx={{ p: 3, borderRadius: 3, width: { xs: '100%', md: 320 } }}>
          <Stack spacing={1.5} divider={<Divider flexItem />}>
            <MetaRow label="Status"><StatusChip status={tender.status} /></MetaRow>
            <MetaRow label="Agency">{tender.agency}</MetaRow>
            <MetaRow label="Source">{tender.source}</MetaRow>
            <MetaRow label="Est. Value"><Box sx={monoSx}>{formatCurrency(tender.estValue)}</Box></MetaRow>
            <MetaRow label="Closing">{formatDate(tender.closingAt)}</MetaRow>
            <MetaRow label="Created">{formatDateTime(tender.createdAt)}</MetaRow>
            <MetaRow label="Updated">{formatDateTime(tender.updatedAt)}</MetaRow>
          </Stack>
        </Paper>
      </Box>

      <ConfirmDialog
        open={confirmOpen}
        title="Delete tender?"
        message={`This permanently removes "${tender.title}". This cannot be undone.`}
        confirmLabel="Delete"
        loading={deleteMutation.isPending}
        onConfirm={() => deleteMutation.mutate()}
        onClose={() => setConfirmOpen(false)}
      />
    </Box>
  )
}
