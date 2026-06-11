import { useFormik } from 'formik'
import * as Yup from 'yup'
import { Alert, Box, Button, MenuItem, Stack, TextField } from '@mui/material'
import { TENDER_STATUSES } from '../../utils/tenderStatus'

const createSchema = Yup.object({
  reference: Yup.string().required('Reference is required'),
  title: Yup.string().required('Title is required'),
  agency: Yup.string().required('Agency is required'),
  estValue: Yup.number().typeError('Must be a number').min(0).nullable(),
})

const editSchema = createSchema.shape({
  status: Yup.string().oneOf(TENDER_STATUSES).required('Status is required'),
})

function toDateInput(v) {
  if (!v) return ''
  const d = new Date(v)
  return Number.isNaN(d.getTime()) ? '' : d.toISOString().slice(0, 10)
}

export default function TenderForm({ initial, mode, serverError, submitting, onSubmit }) {
  const isEdit = mode === 'edit'

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: {
      reference: initial?.reference ?? '',
      title: initial?.title ?? '',
      agency: initial?.agency ?? '',
      status: initial?.status ?? 'Interested',
      estValue: initial?.estValue ?? '',
      closingAt: toDateInput(initial?.closingAt),
      notes: initial?.notes ?? '',
    },
    validationSchema: isEdit ? editSchema : createSchema,
    onSubmit: (values) => {
      const payload = {
        ...values,
        estValue: values.estValue === '' ? null : Number(values.estValue),
        closingAt: values.closingAt === '' ? null : new Date(values.closingAt).toISOString(),
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
    <form onSubmit={formik.handleSubmit}>
      <Stack spacing={2} sx={{ maxWidth: 640 }}>
        {serverError && <Alert severity="error">{serverError}</Alert>}
        <TextField label="Reference" fullWidth disabled={isEdit} {...field('reference')} />
        <TextField label="Title" fullWidth {...field('title')} />
        <TextField label="Agency" fullWidth {...field('agency')} />
        {isEdit && (
          <TextField label="Status" select fullWidth {...field('status')}>
            {TENDER_STATUSES.map((s) => <MenuItem key={s} value={s}>{s}</MenuItem>)}
          </TextField>
        )}
        <TextField label="Estimated value (SGD)" type="number" fullWidth {...field('estValue')} />
        <TextField label="Closing date" type="date" fullWidth InputLabelProps={{ shrink: true }} {...field('closingAt')} />
        <TextField label="Notes" multiline minRows={3} fullWidth {...field('notes')} />
        <Box>
          <Button type="submit" variant="contained" disabled={submitting}>
            {submitting ? 'Saving…' : isEdit ? 'Save changes' : 'Create tender'}
          </Button>
        </Box>
      </Stack>
    </form>
  )
}
