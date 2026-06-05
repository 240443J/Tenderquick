import { useFormik } from 'formik'
import * as Yup from 'yup'
import {
  Box, TextField, MenuItem, Button, Stack, Alert,
} from '@mui/material'
import { TENDER_STATUSES } from '../../utils/tenderStatus'

const createSchema = Yup.object({
  reference: Yup.string().trim().min(2).max(80).required('Reference is required'),
  title: Yup.string().trim().min(3).max(200).required('Title is required'),
  agency: Yup.string().trim().min(2).max(160).required('Agency is required'),
  estValue: Yup.number().typeError('Must be a number').min(0).nullable(),
  closingAt: Yup.string().nullable(),
  notes: Yup.string().max(2000).nullable(),
})

const editSchema = createSchema.shape({
  status: Yup.string().oneOf(TENDER_STATUSES).required(),
})

function toDateInput(value) {
  if (!value) return ''
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return ''
  return d.toISOString().slice(0, 10)
}

export default function TenderForm({ mode, initial, onSubmit, submitting, serverError }) {
  const isEdit = mode === 'edit'

  const formik = useFormik({
    initialValues: {
      reference: initial?.reference || '',
      title: initial?.title || '',
      agency: initial?.agency || '',
      status: initial?.status || 'Interested',
      estValue: initial?.estValue ?? '',
      closingAt: toDateInput(initial?.closingAt),
      notes: initial?.notes || '',
    },
    validationSchema: isEdit ? editSchema : createSchema,
    onSubmit: (values) => {
      const payload = {
        title: values.title.trim(),
        agency: values.agency.trim(),
        estValue: values.estValue === '' ? null : Number(values.estValue),
        closingAt: values.closingAt ? new Date(values.closingAt).toISOString() : null,
        notes: values.notes?.trim() || null,
      }
      if (isEdit) {
        payload.status = values.status
      } else {
        payload.reference = values.reference.trim()
      }
      onSubmit(payload)
    },
  })

  const field = (name) => ({
    name,
    value: formik.values[name],
    onChange: formik.handleChange,
    onBlur: formik.handleBlur,
    error: formik.touched[name] && Boolean(formik.errors[name]),
    helperText: formik.touched[name] && formik.errors[name],
  })

  return (
    <Box component="form" onSubmit={formik.handleSubmit}>
      <Stack spacing={2} sx={{ maxWidth: 640 }}>
        {serverError && <Alert severity="error">{serverError}</Alert>}

        <TextField
          label="Reference"
          {...field('reference')}
          disabled={isEdit}
          required={!isEdit}
          fullWidth
        />
        <TextField label="Title" {...field('title')} required fullWidth />
        <TextField label="Agency" {...field('agency')} required fullWidth />

        {isEdit && (
          <TextField label="Status" select {...field('status')} fullWidth>
            {TENDER_STATUSES.map((s) => (
              <MenuItem key={s} value={s}>{s}</MenuItem>
            ))}
          </TextField>
        )}

        <TextField
          label="Estimated Value (SGD)"
          type="number"
          {...field('estValue')}
          fullWidth
        />
        <TextField
          label="Closing Date"
          type="date"
          {...field('closingAt')}
          InputLabelProps={{ shrink: true }}
          fullWidth
        />
        <TextField label="Notes" {...field('notes')} multiline minRows={3} fullWidth />

        <Box sx={{ display: 'flex', gap: 1.5 }}>
          <Button type="submit" variant="contained" disabled={submitting} sx={{ px: 3, py: 1.25 }}>
            {isEdit ? 'Save Changes' : 'Create Tender'}
          </Button>
        </Box>
      </Stack>
    </Box>
  )
}
