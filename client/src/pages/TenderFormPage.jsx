import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Helmet } from 'react-helmet-async'
import { Box, CircularProgress, Container, Typography } from '@mui/material'
import { getById, create, update } from '../api/tenders'
import TenderForm from '../components/tenders/TenderForm'

export default function TenderFormPage() {
  const { id } = useParams()
  const isEdit = Boolean(id)
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [serverError, setServerError] = useState('')

  const { data, isPending } = useQuery({
    queryKey: ['tender', id],
    queryFn: () => getById(id),
    enabled: isEdit,
  })

  const mutation = useMutation({
    mutationFn: (payload) => (isEdit ? update(id, payload) : create(payload)),
    onSuccess: (res) => {
      qc.invalidateQueries({ queryKey: ['tenders'] })
      const newId = isEdit ? id : res.data.id
      navigate(`/tenders/${newId}`)
    },
    onError: (err) => {
      setServerError(err.response?.data?.message || 'Something went wrong. Please try again.')
    },
  })

  if (isEdit && isPending) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Helmet><title>{isEdit ? 'Edit tender' : 'New tender'} · Tenderquick</title></Helmet>
      <Typography variant="h4" gutterBottom>{isEdit ? 'Edit Tender' : 'New Tender'}</Typography>
      <TenderForm
        mode={isEdit ? 'edit' : 'create'}
        initial={isEdit ? data?.data : null}
        serverError={serverError}
        submitting={mutation.isPending}
        onSubmit={(payload) => { setServerError(''); mutation.mutate(payload) }}
      />
    </Container>
  )
}
