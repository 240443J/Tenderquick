import { useState } from 'react'
import { useParams, useNavigate, Link as RouterLink } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Helmet } from 'react-helmet-async'
import {
  Alert, Box, Button, Card, CardContent, CircularProgress, Container, Divider,
  Stack, Typography,
} from '@mui/material'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import { getById, remove } from '../api/tenders'
import { formatCurrency, formatDate } from '../utils/format'
import StatusChip from '../components/common/StatusChip'
import RoleGate from '../components/common/RoleGate'
import ConfirmDialog from '../components/common/ConfirmDialog'

function MetaRow({ label, children }) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', py: 0.75 }}>
      <Typography variant="body2" color="text.secondary">{label}</Typography>
      <Box sx={{ textAlign: 'right' }}>{children}</Box>
    </Box>
  )
}

export default function TenderDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [confirm, setConfirm] = useState(false)

  const { data, isPending, isError, error } = useQuery({
    queryKey: ['tender', id],
    queryFn: () => getById(id),
  })

  const del = useMutation({
    mutationFn: () => remove(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['tenders'] })
      navigate('/tenders')
    },
  })

  if (isPending) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
  if (isError) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity={error.response?.status === 404 ? 'warning' : 'error'}>
          {error.response?.status === 404 ? 'Tender not found.' : 'Failed to load tender.'}
        </Alert>
      </Container>
    )
  }

  const t = data.data

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Helmet><title>{t.reference} · Tenderquick</title></Helmet>

      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
        <Box>
          <Typography variant="overline" color="text.secondary">{t.reference}</Typography>
          <Typography variant="h4">{t.title}</Typography>
        </Box>
        <RoleGate allow={['Admin', 'Estimator']}>
          <Stack direction="row" spacing={1}>
            <Button startIcon={<EditIcon />} variant="outlined" component={RouterLink} to={`/tenders/${id}/edit`}>
              Edit
            </Button>
            <RoleGate allow={['Admin']}>
              <Button startIcon={<DeleteIcon />} color="error" variant="outlined" onClick={() => setConfirm(true)}>
                Delete
              </Button>
            </RoleGate>
          </Stack>
        </RoleGate>
      </Stack>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '2fr 1fr' }, gap: 3 }}>
        <Card>
          <CardContent>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>Notes</Typography>
            <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap' }}>
              {t.notes || 'No notes yet.'}
            </Typography>
          </CardContent>
        </Card>
        <Card>
          <CardContent>
            <Typography variant="subtitle2" color="text.secondary" gutterBottom>Details</Typography>
            <MetaRow label="Status"><StatusChip status={t.status} /></MetaRow>
            <Divider />
            <MetaRow label="Agency"><Typography variant="body2">{t.agency}</Typography></MetaRow>
            <Divider />
            <MetaRow label="Source"><Typography variant="body2">{t.source}</Typography></MetaRow>
            <Divider />
            <MetaRow label="Est. value"><Typography variant="body2">{formatCurrency(t.estValue)}</Typography></MetaRow>
            <Divider />
            <MetaRow label="Closing"><Typography variant="body2">{formatDate(t.closingAt)}</Typography></MetaRow>
          </CardContent>
        </Card>
      </Box>

      <ConfirmDialog
        open={confirm}
        title="Delete tender?"
        message={`This permanently removes ${t.reference}. This cannot be undone.`}
        confirmLabel="Delete"
        loading={del.isPending}
        onConfirm={() => del.mutate()}
        onCancel={() => setConfirm(false)}
      />
    </Container>
  )
}
