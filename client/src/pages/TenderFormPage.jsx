import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box, Typography, Button, CircularProgress, Alert,
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import * as tendersApi from '../api/tenders'
import TenderForm from '../components/tenders/TenderForm'
import { tokens } from '../theme'

export default function TenderFormPage() {
  const { id } = useParams()
  const isEdit = Boolean(id)
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [serverError, setServerError] = useState('')

  const { data: tender, isLoading } = useQuery({
    queryKey: ['tender', id],
    queryFn: () => tendersApi.getById(id).then((r) => r.data),
    enabled: isEdit,
  })

  const mutation = useMutation({
    mutationFn: (payload) =>
      isEdit ? tendersApi.update(id, payload) : tendersApi.create(payload),
    onSuccess: (res) => {
      queryClient.invalidateQueries({ queryKey: ['tenders'] })
      if (isEdit) queryClient.invalidateQueries({ queryKey: ['tender', id] })
      const newId = res.data?.id ?? id
      navigate(`/tenders/${newId}`)
    },
    onError: (err) => {
      setServerError(err.response?.data?.message || 'Could not save the tender.')
    },
  })

  if (isEdit && isLoading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
  }

  return (
    <Box>
      <Button startIcon={<ArrowBackIcon />} onClick={() => navigate(-1)} sx={{ mb: 2, color: tokens.textSecondary }}>
        Back
      </Button>
      <Typography variant="h1" sx={{ mb: 3 }}>
        {isEdit ? 'Edit Tender' : 'New Tender'}
      </Typography>

      <TenderForm
        mode={isEdit ? 'edit' : 'create'}
        initial={tender}
        submitting={mutation.isPending}
        serverError={serverError}
        onSubmit={(payload) => { setServerError(''); mutation.mutate(payload) }}
      />
    </Box>
  )
}
